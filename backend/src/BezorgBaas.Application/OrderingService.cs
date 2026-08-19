using BezorgBaas.Domain;
using BezorgBaas.Domain.Catalog;
using BezorgBaas.Domain.Common;
using BezorgBaas.Domain.Ordering;
using BezorgBaas.Domain.Promotions;

namespace BezorgBaas.Application;

/// <summary>
/// Use cases rond bestellen: prijs berekenen, bestelling plaatsen en de status
/// laten doorlopen. Coordineert de aggregates; de regels zelf staan in het domein.
/// </summary>
public class OrderingService
{
    private readonly IRestaurantRepository _restaurants;
    private readonly IOrderRepository _orders;
    private readonly IPromoCodeRepository _promoCodes;

    public OrderingService(IRestaurantRepository restaurants, IOrderRepository orders,
        IPromoCodeRepository promoCodes)
    {
        _restaurants = restaurants;
        _orders = orders;
        _promoCodes = promoCodes;
    }

    public async Task<Quote> QuoteAsync(QuoteRequest request, CancellationToken cancellationToken = default)
    {
        Restaurant restaurant = await RequireRestaurantAsync(request.RestaurantId, cancellationToken);
        List<QuoteLine> lines = BuildLines(restaurant, request.Lines);

        Money subtotal = lines.Aggregate(Money.Zero, (total, line) => total + Money.Of(line.LineTotal));
        Money deliveryFee = restaurant.DeliveryFeeFor(subtotal);
        Money discount = Money.Zero;
        string? promoMessage = null;

        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            try
            {
                discount = await ApplyPromoAsync(request.PromoCode!, subtotal, deliveryFee, restaurant.Id,
                    request.CustomerEmail, cancellationToken);
                promoMessage = $"Actiecode {request.PromoCode!.ToUpperInvariant()} toegepast.";
            }
            catch (DomainException exception)
            {
                // Een ongeldige actiecode blokkeert de bestelling niet; hij telt alleen niet mee.
                promoMessage = exception.Message;
            }
        }

        Money total = (subtotal + deliveryFee).Subtract(discount);

        return new Quote(
            restaurant.Id,
            restaurant.Name,
            lines,
            subtotal.Amount,
            deliveryFee.Amount,
            discount.Amount,
            total.Amount,
            restaurant.MinimumOrder.Amount,
            subtotal.IsAtLeast(restaurant.MinimumOrder),
            request.PromoCode?.ToUpperInvariant(),
            promoMessage);
    }

    public async Task<OrderView> PlaceAsync(PlaceOrderRequest request, CancellationToken cancellationToken = default)
    {
        Restaurant restaurant = await RequireRestaurantAsync(request.RestaurantId, cancellationToken);
        List<QuoteLine> quoteLines = BuildLines(restaurant, request.Lines);

        Money subtotal = quoteLines.Aggregate(Money.Zero, (total, line) => total + Money.Of(line.LineTotal));
        restaurant.AssertAcceptsOrder(subtotal);

        Money deliveryFee = restaurant.DeliveryFeeFor(subtotal);
        Money discount = Money.Zero;
        PromoCode? promo = null;

        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            promo = await _promoCodes.ByCodeAsync(request.PromoCode!.Trim().ToUpperInvariant(), cancellationToken)
                    ?? throw DomainException.NotFound("promo.unknown",
                        $"Actiecode {request.PromoCode} bestaat niet.");
            bool used = await _orders.CustomerUsedPromoAsync(request.CustomerEmail, promo.Code, cancellationToken);
            discount = promo.DiscountFor(subtotal, deliveryFee, restaurant.Id,
                DateOnly.FromDateTime(DateTime.UtcNow), used);
        }

        (TimeOnly slotStart, TimeOnly slotEnd) = ParseSlot(request.DeliverySlot);
        DomainException.Require(request.DeliveryDate >= DateOnly.FromDateTime(DateTime.UtcNow),
            "slot.in_past", "Kies een bezorgmoment in de toekomst.");

        List<OrderLine> orderLines = quoteLines
            .Select(line => OrderLine.For(line.MenuItemId, line.ItemName, line.OptionSummary, line.Quantity,
                Money.Of(line.UnitPrice)))
            .ToList();

        Order order = Order.Place(
            await NextOrderNumberAsync(cancellationToken),
            restaurant.Id,
            request.CustomerName,
            request.CustomerEmail,
            new DeliveryAddress(request.Address.Street, request.Address.HouseNumber, request.Address.PostalCode,
                request.Address.City, request.Address.Note),
            request.DeliveryDate,
            slotStart,
            slotEnd,
            ParsePaymentMethod(request.PaymentMethod),
            request.PaymentReference,
            orderLines,
            deliveryFee,
            discount,
            promo?.Code);

        promo?.Redeem();

        await _orders.AddAsync(order, cancellationToken);
        await _orders.SaveChangesAsync(cancellationToken);
        return OrderView.From(order, restaurant.Name);
    }

    public async Task<OrderView> AdvanceAsync(Guid orderId, string? targetStatus, string? note,
        CancellationToken cancellationToken = default)
    {
        Order order = await RequireOrderAsync(orderId, cancellationToken);
        OrderStatus next = targetStatus is null
            ? order.NextStatus() ?? throw DomainException.Conflict("order.final_status",
                $"Een bestelling met status {order.Status} verandert niet meer.")
            : ParseStatus(targetStatus);

        order.MoveTo(next, note);
        await _orders.SaveChangesAsync(cancellationToken);
        return await ViewAsync(order, cancellationToken);
    }

    public async Task<OrderView> CancelAsync(Guid orderId, string reason,
        CancellationToken cancellationToken = default)
    {
        Order order = await RequireOrderAsync(orderId, cancellationToken);
        order.Cancel(reason);
        await _orders.SaveChangesAsync(cancellationToken);
        return await ViewAsync(order, cancellationToken);
    }

    public async Task<OrderView> ByNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        Order order = await _orders.ByNumberAsync(orderNumber, cancellationToken)
                      ?? throw DomainException.NotFound("order.not_found",
                          $"Bestelling {orderNumber} bestaat niet.");
        return await ViewAsync(order, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderView>> ForRestaurantAsync(Guid restaurantId,
        CancellationToken cancellationToken = default)
    {
        Restaurant restaurant = await RequireRestaurantAsync(restaurantId, cancellationToken);
        IReadOnlyList<Order> orders = await _orders.ForRestaurantAsync(restaurantId, cancellationToken);
        return orders.Select(order => OrderView.From(order, restaurant.Name)).ToList();
    }

    public async Task<IReadOnlyList<OrderView>> ForCustomerAsync(string email,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Order> orders = await _orders.ForCustomerAsync(email, cancellationToken);
        List<OrderView> views = new();
        foreach (Order order in orders)
        {
            views.Add(await ViewAsync(order, cancellationToken));
        }
        return views;
    }

    private async Task<Money> ApplyPromoAsync(string code, Money subtotal, Money deliveryFee, Guid restaurantId,
        string? customerEmail, CancellationToken cancellationToken)
    {
        PromoCode promo = await _promoCodes.ByCodeAsync(code.Trim().ToUpperInvariant(), cancellationToken)
                          ?? throw DomainException.NotFound("promo.unknown", $"Actiecode {code} bestaat niet.");
        bool used = !string.IsNullOrWhiteSpace(customerEmail)
                    && await _orders.CustomerUsedPromoAsync(customerEmail!, promo.Code, cancellationToken);
        return promo.DiscountFor(subtotal, deliveryFee, restaurantId, DateOnly.FromDateTime(DateTime.UtcNow), used);
    }

    private static List<QuoteLine> BuildLines(Restaurant restaurant, IReadOnlyList<CartLine> lines)
    {
        DomainException.Require(lines.Count > 0, "order.empty", "Je winkelmandje is leeg.");
        List<QuoteLine> result = new();

        foreach (CartLine line in lines)
        {
            MenuItem item = restaurant.RequireItem(line.MenuItemId);
            IReadOnlyList<Guid> optionIds = line.OptionIds ?? Array.Empty<Guid>();
            Money unitPrice = item.PriceWith(optionIds);
            DomainException.Require(line.Quantity is >= 1 and <= 20, "line.quantity_range",
                "Kies een aantal tussen 1 en 20.");

            string? summary = optionIds.Count == 0
                ? null
                : string.Join(", ", item.Options.Where(option => optionIds.Contains(option.Id))
                    .Select(option => option.Name));

            result.Add(new QuoteLine(item.Id, item.Name, summary, line.Quantity, unitPrice.Amount,
                (unitPrice * line.Quantity).Amount));
        }

        return result;
    }

    private async Task<Restaurant> RequireRestaurantAsync(Guid id, CancellationToken cancellationToken) =>
        await _restaurants.ByIdAsync(id, cancellationToken)
        ?? throw DomainException.NotFound("restaurant.not_found", "Dit restaurant bestaat niet.");

    private async Task<Order> RequireOrderAsync(Guid id, CancellationToken cancellationToken) =>
        await _orders.ByIdAsync(id, cancellationToken)
        ?? throw DomainException.NotFound("order.not_found", "Deze bestelling bestaat niet.");

    private async Task<OrderView> ViewAsync(Order order, CancellationToken cancellationToken)
    {
        Restaurant? restaurant = await _restaurants.ByIdAsync(order.RestaurantId, cancellationToken);
        return OrderView.From(order, restaurant?.Name ?? "onbekend");
    }

    private async Task<string> NextOrderNumberAsync(CancellationToken cancellationToken)
    {
        int count = await _orders.CountAsync(cancellationToken);
        return $"BB-{DateTime.UtcNow:yyyy}-{count + 1001:D5}";
    }

    private static (TimeOnly Start, TimeOnly End) ParseSlot(string slot)
    {
        string[] parts = (slot ?? string.Empty).Split('-', StringSplitOptions.TrimEntries);
        if (parts.Length != 2
            || !TimeOnly.TryParse(parts[0], out TimeOnly start)
            || !TimeOnly.TryParse(parts[1], out TimeOnly end))
        {
            throw DomainException.Invalid("slot.invalid", "Kies een bezorgmoment.");
        }
        return (start, end);
    }

    private static PaymentMethod ParsePaymentMethod(string value) =>
        Enum.TryParse(value, true, out PaymentMethod method)
            ? method
            : throw DomainException.Invalid("payment.method_unknown", "Kies een geldige betaalmethode.");

    private static OrderStatus ParseStatus(string value) =>
        Enum.TryParse(value, true, out OrderStatus status)
            ? status
            : throw DomainException.Invalid("order.status_unknown", $"Onbekende status {value}.");
}
