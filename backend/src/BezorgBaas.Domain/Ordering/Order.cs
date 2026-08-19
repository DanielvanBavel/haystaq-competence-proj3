using BezorgBaas.Domain.Common;

namespace BezorgBaas.Domain.Ordering;

public enum OrderStatus
{
    Placed,
    Accepted,
    Preparing,
    OnTheWay,
    Delivered,
    Cancelled,
    Rejected
}

public enum PaymentMethod
{
    Ideal,
    Card,
    Cash
}

/// <summary>Aggregate root van het context bestellen.</summary>
public class Order
{
    /// <summary>Contant afrekenen mag tot en met dit bedrag.</summary>
    public static readonly Money CashLimit = Money.Of(50m);

    private readonly List<OrderLine> _lines = new();
    private readonly List<OrderStatusChange> _history = new();

    private Order()
    {
        // voor EF Core
    }

    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid RestaurantId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public DeliveryAddress Address { get; private set; } = null!;
    public DateOnly DeliveryDate { get; private set; }
    public TimeOnly DeliverySlotStart { get; private set; }
    public TimeOnly DeliverySlotEnd { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public string? PaymentReference { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money Subtotal { get; private set; }
    public Money DeliveryFee { get; private set; }
    public Money Discount { get; private set; }
    public Money Total { get; private set; }
    public string? PromoCode { get; private set; }
    public DateTimeOffset PlacedAt { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines;
    public IReadOnlyCollection<OrderStatusChange> History => _history;

    public static Order Place(
        string orderNumber,
        Guid restaurantId,
        string customerName,
        string customerEmail,
        DeliveryAddress address,
        DateOnly deliveryDate,
        TimeOnly slotStart,
        TimeOnly slotEnd,
        PaymentMethod paymentMethod,
        string? paymentReference,
        IReadOnlyCollection<OrderLine> lines,
        Money deliveryFee,
        Money discount,
        string? promoCode)
    {
        DomainException.Require(!string.IsNullOrWhiteSpace(customerName), "customer.name_required",
            "Vul je naam in.");
        DomainException.Require(customerEmail.Contains('@') && customerEmail.Contains('.'),
            "customer.email_invalid", "Vul een geldig e-mailadres in.");
        DomainException.Require(lines.Count > 0, "order.empty", "Je winkelmandje is leeg.");
        DomainException.Require(slotEnd > slotStart, "slot.invalid", "Het gekozen bezorgmoment klopt niet.");

        Money subtotal = lines.Aggregate(Money.Zero, (total, line) => total + line.LineTotal);
        Money grandTotal = (subtotal + deliveryFee).Subtract(discount);

        // Contant betalen kan alleen bij kleinere bestellingen.
        if (paymentMethod == PaymentMethod.Cash)
        {
            DomainException.RequireState(!CashLimit.IsLessThan(grandTotal), "payment.cash_limit",
                $"Contant betalen kan tot {CashLimit} euro. Kies een andere betaalmethode.");
        }
        else
        {
            DomainException.Require(!string.IsNullOrWhiteSpace(paymentReference), "payment.not_authorised",
                "De betaling is nog niet bevestigd.");
        }

        Order order = new()
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            RestaurantId = restaurantId,
            CustomerName = customerName.Trim(),
            CustomerEmail = customerEmail.Trim().ToLowerInvariant(),
            Address = address,
            DeliveryDate = deliveryDate,
            DeliverySlotStart = slotStart,
            DeliverySlotEnd = slotEnd,
            PaymentMethod = paymentMethod,
            PaymentReference = paymentReference,
            Status = OrderStatus.Placed,
            Subtotal = subtotal,
            DeliveryFee = deliveryFee,
            Discount = discount,
            Total = grandTotal,
            PromoCode = promoCode,
            PlacedAt = DateTimeOffset.UtcNow
        };

        order._lines.AddRange(lines);
        order._history.Add(OrderStatusChange.For(order.Id, OrderStatus.Placed, "Bestelling geplaatst"));
        return order;
    }

    private static readonly Dictionary<OrderStatus, OrderStatus[]> Allowed = new()
    {
        [OrderStatus.Placed] = new[] { OrderStatus.Accepted, OrderStatus.Rejected, OrderStatus.Cancelled },
        [OrderStatus.Accepted] = new[] { OrderStatus.Preparing, OrderStatus.Cancelled },
        [OrderStatus.Preparing] = new[] { OrderStatus.OnTheWay },
        [OrderStatus.OnTheWay] = new[] { OrderStatus.Delivered },
        [OrderStatus.Delivered] = Array.Empty<OrderStatus>(),
        [OrderStatus.Cancelled] = Array.Empty<OrderStatus>(),
        [OrderStatus.Rejected] = Array.Empty<OrderStatus>()
    };

    public void MoveTo(OrderStatus next, string? note = null)
    {
        DomainException.RequireState(Allowed[Status].Contains(next), "order.status_transition",
            $"Een bestelling kan niet van {Status} naar {next}.");
        Status = next;
        _history.Add(OrderStatusChange.For(Id, next, note));
    }

    /// <summary>Annuleren mag zolang de keuken nog niet is begonnen.</summary>
    public void Cancel(string reason)
    {
        DomainException.RequireState(Status is OrderStatus.Placed or OrderStatus.Accepted, "order.cannot_cancel",
            "Deze bestelling is al in bereiding en kan niet meer geannuleerd worden.");
        DomainException.Require(!string.IsNullOrWhiteSpace(reason), "order.cancel_reason_required",
            "Geef een reden voor de annulering.");
        Status = OrderStatus.Cancelled;
        _history.Add(OrderStatusChange.For(Id, OrderStatus.Cancelled, reason));
    }

    /// <summary>De volgende stap in de normale gang van zaken.</summary>
    public OrderStatus? NextStatus() => Status switch
    {
        OrderStatus.Placed => OrderStatus.Accepted,
        OrderStatus.Accepted => OrderStatus.Preparing,
        OrderStatus.Preparing => OrderStatus.OnTheWay,
        OrderStatus.OnTheWay => OrderStatus.Delivered,
        _ => null
    };
}

/// <summary>Entiteit binnen het aggregate <see cref="Order"/>.</summary>
public class OrderLine
{
    private OrderLine()
    {
        // voor EF Core
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid MenuItemId { get; private set; }
    public string ItemName { get; private set; } = string.Empty;
    public string? OptionSummary { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money LineTotal { get; private set; }

    public static OrderLine For(Guid menuItemId, string itemName, string? optionSummary, int quantity, Money unitPrice)
    {
        DomainException.Require(quantity is >= 1 and <= 20, "line.quantity_range",
            "Kies een aantal tussen 1 en 20.");
        return new OrderLine
        {
            Id = Guid.NewGuid(),
            MenuItemId = menuItemId,
            ItemName = itemName,
            OptionSummary = optionSummary,
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = unitPrice * quantity
        };
    }
}

/// <summary>Historieregel: wanneer ging de bestelling naar welke status.</summary>
public class OrderStatusChange
{
    private OrderStatusChange()
    {
        // voor EF Core
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public OrderStatus Status { get; private set; }
    public string? Note { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }

    public static OrderStatusChange For(Guid orderId, OrderStatus status, string? note) => new()
    {
        Id = Guid.NewGuid(),
        OrderId = orderId,
        Status = status,
        Note = note,
        ChangedAt = DateTimeOffset.UtcNow
    };
}
