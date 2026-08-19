using BezorgBaas.Domain.Catalog;
using BezorgBaas.Domain.Ordering;

namespace BezorgBaas.Application;

/// <summary>Leesmodellen en commando's. Domeinobjecten verlaten de applicatielaag niet.</summary>
public record RestaurantSummary(
    Guid Id,
    string Slug,
    string Name,
    string Cuisine,
    string City,
    decimal Rating,
    int EstimatedDeliveryMinutes,
    decimal MinimumOrder,
    decimal DeliveryFee,
    decimal? FreeDeliveryFrom,
    bool IsOpen)
{
    public static RestaurantSummary From(Restaurant restaurant) => new(
        restaurant.Id,
        restaurant.Slug,
        restaurant.Name,
        restaurant.Cuisine,
        restaurant.City,
        restaurant.Rating,
        restaurant.EstimatedDeliveryMinutes,
        restaurant.MinimumOrder.Amount,
        restaurant.DeliveryFee.Amount,
        restaurant.FreeDeliveryFrom?.Amount,
        restaurant.IsOpen);
}

public record MenuItemOptionView(Guid Id, string Name, string Kind, decimal PriceDelta, bool IsDefault);

public record MenuItemView(
    Guid Id,
    string Name,
    string? Description,
    string Category,
    decimal Price,
    bool IsAvailable,
    bool IsVegetarian,
    int SpicinessLevel,
    IReadOnlyList<MenuItemOptionView> Options);

public record RestaurantDetail(RestaurantSummary Restaurant, IReadOnlyList<MenuItemView> Menu)
{
    public static RestaurantDetail From(Restaurant restaurant) => new(
        RestaurantSummary.From(restaurant),
        restaurant.Menu
            .OrderBy(item => item.Category)
            .ThenBy(item => item.Name)
            .Select(item => new MenuItemView(
                item.Id,
                item.Name,
                item.Description,
                item.Category,
                item.Price.Amount,
                item.IsAvailable,
                item.IsVegetarian,
                item.SpicinessLevel,
                item.Options
                    .OrderBy(option => option.Kind)
                    .ThenBy(option => option.PriceDelta.Amount)
                    .Select(option => new MenuItemOptionView(option.Id, option.Name, option.Kind.ToString(),
                        option.PriceDelta.Amount, option.IsDefault))
                    .ToList()))
            .ToList());
}

public record CartLine(Guid MenuItemId, int Quantity, IReadOnlyList<Guid>? OptionIds);

public record QuoteRequest(Guid RestaurantId, IReadOnlyList<CartLine> Lines, string? PromoCode, string? CustomerEmail);

public record QuoteLine(Guid MenuItemId, string ItemName, string? OptionSummary, int Quantity, decimal UnitPrice,
    decimal LineTotal);

public record Quote(
    Guid RestaurantId,
    string RestaurantName,
    IReadOnlyList<QuoteLine> Lines,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal Discount,
    decimal Total,
    decimal MinimumOrder,
    bool MeetsMinimum,
    string? PromoCode,
    string? PromoMessage);

public record AddressInput(string Street, string HouseNumber, string PostalCode, string City, string? Note);

public record PlaceOrderRequest(
    Guid RestaurantId,
    IReadOnlyList<CartLine> Lines,
    string CustomerName,
    string CustomerEmail,
    AddressInput Address,
    DateOnly DeliveryDate,
    string DeliverySlot,
    string PaymentMethod,
    string? PaymentReference,
    string? PromoCode);

public record OrderLineView(string ItemName, string? OptionSummary, int Quantity, decimal UnitPrice, decimal LineTotal);

public record OrderStatusView(string Status, string? Note, DateTimeOffset ChangedAt);

public record OrderView(
    Guid Id,
    string OrderNumber,
    Guid RestaurantId,
    string RestaurantName,
    string CustomerName,
    string CustomerEmail,
    string Address,
    DateOnly DeliveryDate,
    string DeliverySlot,
    string PaymentMethod,
    string Status,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal Discount,
    decimal Total,
    string? PromoCode,
    DateTimeOffset PlacedAt,
    IReadOnlyList<OrderLineView> Lines,
    IReadOnlyList<OrderStatusView> History)
{
    public static OrderView From(Order order, string restaurantName) => new(
        order.Id,
        order.OrderNumber,
        order.RestaurantId,
        restaurantName,
        order.CustomerName,
        order.CustomerEmail,
        $"{order.Address.Street} {order.Address.HouseNumber}, {order.Address.PostalCode} {order.Address.City}",
        order.DeliveryDate,
        $"{order.DeliverySlotStart:HH\\:mm}-{order.DeliverySlotEnd:HH\\:mm}",
        order.PaymentMethod.ToString(),
        order.Status.ToString(),
        order.Subtotal.Amount,
        order.DeliveryFee.Amount,
        order.Discount.Amount,
        order.Total.Amount,
        order.PromoCode,
        order.PlacedAt,
        order.Lines.Select(line => new OrderLineView(line.ItemName, line.OptionSummary, line.Quantity,
            line.UnitPrice.Amount, line.LineTotal.Amount)).ToList(),
        order.History.OrderBy(change => change.ChangedAt)
            .Select(change => new OrderStatusView(change.Status.ToString(), change.Note, change.ChangedAt))
            .ToList());
}
