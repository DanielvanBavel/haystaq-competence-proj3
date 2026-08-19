using System.Text.RegularExpressions;
using BezorgBaas.Domain.Common;

namespace BezorgBaas.Domain.Ordering;

/// <summary>Waarde-object: het bezorgadres.</summary>
public sealed partial record DeliveryAddress
{
    public string Street { get; }
    public string HouseNumber { get; }
    public string PostalCode { get; }
    public string City { get; }
    public string? Note { get; }

    public DeliveryAddress(string street, string houseNumber, string postalCode, string city, string? note = null)
    {
        DomainException.Require(!string.IsNullOrWhiteSpace(street), "address.street_required",
            "Vul een straatnaam in.");
        DomainException.Require(HouseNumberPattern().IsMatch(houseNumber ?? string.Empty),
            "address.house_number_invalid", "Huisnummer mag alleen cijfers en een toevoeging bevatten.");
        DomainException.Require(PostalCodePattern().IsMatch(postalCode ?? string.Empty),
            "address.postal_code_invalid", "Postcode moet de vorm 1234 AB hebben.");
        DomainException.Require(!string.IsNullOrWhiteSpace(city), "address.city_required",
            "Vul een plaatsnaam in.");

        Street = street.Trim();
        HouseNumber = houseNumber!.Trim().ToUpperInvariant();
        PostalCode = NormalisePostalCode(postalCode!);
        City = city.Trim();
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }

    private static string NormalisePostalCode(string value) =>
        $"{value.Replace(" ", string.Empty).ToUpperInvariant()[..4]} " +
        $"{value.Replace(" ", string.Empty).ToUpperInvariant()[4..]}";

    [GeneratedRegex("^[1-9][0-9]{3} ?[A-Za-z]{2}$")]
    private static partial Regex PostalCodePattern();

    [GeneratedRegex("^[0-9]{1,5}[A-Za-z]?$")]
    private static partial Regex HouseNumberPattern();
}
