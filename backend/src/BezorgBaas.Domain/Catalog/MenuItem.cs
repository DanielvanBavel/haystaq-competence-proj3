using BezorgBaas.Domain.Common;

namespace BezorgBaas.Domain.Catalog;

/// <summary>Entiteit binnen het aggregate <see cref="Restaurant"/>.</summary>
public class MenuItem
{
    private readonly List<MenuItemOption> _options = new();

    private MenuItem()
    {
        // voor EF Core
    }

    public Guid Id { get; private set; }
    public Guid RestaurantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public Money Price { get; private set; }
    public bool IsAvailable { get; private set; }
    public bool IsVegetarian { get; private set; }
    public int SpicinessLevel { get; private set; }

    public IReadOnlyCollection<MenuItemOption> Options => _options;

    /// <summary>Prijs inclusief de gekozen opties.</summary>
    public Money PriceWith(IEnumerable<Guid> optionIds)
    {
        Money total = Price;
        List<Guid> chosen = optionIds.ToList();

        foreach (Guid optionId in chosen)
        {
            MenuItemOption option = _options.FirstOrDefault(candidate => candidate.Id == optionId)
                                   ?? throw DomainException.NotFound("option.not_found",
                                       "Deze optie hoort niet bij dit gerecht.");
            total += option.PriceDelta;
        }

        // Bij een gerecht met maatvoering moet er precies een maat gekozen zijn.
        bool hasSizes = _options.Any(option => option.Kind == OptionKind.Size);
        if (hasSizes)
        {
            int chosenSizes = _options.Count(option => option.Kind == OptionKind.Size && chosen.Contains(option.Id));
            DomainException.Require(chosenSizes == 1, "option.size_required",
                $"Kies een maat voor {Name}.");
        }

        return total;
    }
}

public enum OptionKind
{
    Size,
    Extra
}

/// <summary>Waarde-object: een keuzemogelijkheid bij een gerecht.</summary>
public class MenuItemOption
{
    private MenuItemOption()
    {
        // voor EF Core
    }

    public Guid Id { get; private set; }
    public Guid MenuItemId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public OptionKind Kind { get; private set; }
    public Money PriceDelta { get; private set; }
    public bool IsDefault { get; private set; }
}
