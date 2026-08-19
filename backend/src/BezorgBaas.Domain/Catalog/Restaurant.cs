using BezorgBaas.Domain.Common;

namespace BezorgBaas.Domain.Catalog;

/// <summary>Aggregate root van het context catalogus: een bezorgrestaurant met zijn menu.</summary>
public class Restaurant
{
    private readonly List<MenuItem> _menu = new();

    private Restaurant()
    {
        // voor EF Core
    }

    public Guid Id { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Cuisine { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public decimal Rating { get; private set; }
    public int EstimatedDeliveryMinutes { get; private set; }
    public Money MinimumOrder { get; private set; }
    public Money DeliveryFee { get; private set; }
    public Money? FreeDeliveryFrom { get; private set; }
    public bool IsOpen { get; private set; }

    public IReadOnlyCollection<MenuItem> Menu => _menu;

    /// <summary>Bezorgkosten voor dit subtotaal. Boven de drempel is bezorgen gratis.</summary>
    public Money DeliveryFeeFor(Money subtotal)
    {
        if (FreeDeliveryFrom is { } threshold && subtotal.IsAtLeast(threshold))
        {
            return Money.Zero;
        }
        return DeliveryFee;
    }

    public void AssertAcceptsOrder(Money subtotal)
    {
        DomainException.RequireState(IsOpen, "restaurant.closed",
            $"{Name} is op dit moment gesloten.");
        DomainException.RequireState(subtotal.IsAtLeast(MinimumOrder), "order.below_minimum",
            $"Het minimale bestelbedrag bij {Name} is {MinimumOrder} euro.");
    }

    public MenuItem RequireItem(Guid menuItemId)
    {
        MenuItem? item = _menu.FirstOrDefault(candidate => candidate.Id == menuItemId);
        if (item is null)
        {
            throw DomainException.NotFound("menu_item.not_found", "Dit gerecht staat niet op het menu.");
        }
        DomainException.RequireState(item.IsAvailable, "menu_item.unavailable",
            $"{item.Name} is vandaag uitverkocht.");
        return item;
    }
}
