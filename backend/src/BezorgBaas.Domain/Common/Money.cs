using System.Globalization;

namespace BezorgBaas.Domain.Common;

/// <summary>Bedrag in euro's, altijd met twee decimalen en nooit negatief.</summary>
public readonly record struct Money : IComparable<Money>
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        DomainException.Require(amount >= 0, "money.negative", "Bedrag kan niet negatief zijn.");
        DomainException.Require(decimal.Round(amount, 2) == amount, "money.scale",
            "Bedrag mag maximaal twee decimalen hebben.");
        Amount = amount;
    }

    public static Money Zero => new(0m);

    public static Money Of(decimal amount) => new(decimal.Round(amount, 2, MidpointRounding.AwayFromZero));

    public static Money operator +(Money left, Money right) => new(left.Amount + right.Amount);

    public static Money operator *(Money value, int quantity) => new(value.Amount * quantity);

    public Money Subtract(Money other) => new(Math.Max(0m, Amount - other.Amount));

    public Money Percentage(int percent) =>
        Of(Amount * percent / 100m);

    public bool IsLessThan(Money other) => Amount < other.Amount;

    public bool IsAtLeast(Money other) => Amount >= other.Amount;

    public int CompareTo(Money other) => Amount.CompareTo(other.Amount);

    public override string ToString() => Amount.ToString("0.00", CultureInfo.InvariantCulture);
}
