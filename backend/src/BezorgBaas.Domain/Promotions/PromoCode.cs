using BezorgBaas.Domain.Common;

namespace BezorgBaas.Domain.Promotions;

public enum DiscountKind
{
    Percentage,
    FixedAmount,
    FreeDelivery
}

/// <summary>Aggregate root van het context acties: een kortingscode met zijn voorwaarden.</summary>
public class PromoCode
{
    private PromoCode()
    {
        // voor EF Core
    }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public DiscountKind Kind { get; private set; }
    public int Percentage { get; private set; }
    public Money FixedAmount { get; private set; }
    public Money MinimumSubtotal { get; private set; }
    public DateOnly ValidUntil { get; private set; }
    public int MaxRedemptions { get; private set; }
    public int TimesRedeemed { get; private set; }
    public bool OncePerCustomer { get; private set; }
    public Guid? RestaurantId { get; private set; }

    /// <summary>
    /// Berekent de korting. Gooit als de code niet gebruikt mag worden, met een
    /// code die de UI kan tonen.
    /// </summary>
    public Money DiscountFor(Money subtotal, Money deliveryFee, Guid restaurantId, DateOnly today,
        bool customerUsedBefore)
    {
        DomainException.RequireState(ValidUntil >= today, "promo.expired",
            $"Actiecode {Code} is verlopen.");
        DomainException.RequireState(TimesRedeemed < MaxRedemptions, "promo.exhausted",
            $"Actiecode {Code} is niet meer geldig.");
        DomainException.RequireState(RestaurantId is null || RestaurantId == restaurantId,
            "promo.other_restaurant", $"Actiecode {Code} geldt niet bij dit restaurant.");
        DomainException.RequireState(subtotal.IsAtLeast(MinimumSubtotal), "promo.minimum_not_reached",
            $"Actiecode {Code} geldt vanaf {MinimumSubtotal} euro.");
        DomainException.RequireState(!(OncePerCustomer && customerUsedBefore), "promo.already_used",
            $"Je hebt actiecode {Code} al eerder gebruikt.");

        return Kind switch
        {
            DiscountKind.Percentage => subtotal.Percentage(Percentage),
            DiscountKind.FixedAmount => FixedAmount.IsLessThan(subtotal) ? FixedAmount : subtotal,
            DiscountKind.FreeDelivery => deliveryFee,
            _ => Money.Zero
        };
    }

    public void Redeem()
    {
        TimesRedeemed++;
    }
}
