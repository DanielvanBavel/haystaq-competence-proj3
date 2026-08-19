using BezorgBaas.Domain;
using BezorgBaas.Domain.Catalog;
using BezorgBaas.Domain.Common;

namespace BezorgBaas.Application;

/// <summary>Leeszijde van de catalogus: zoeken en detail ophalen.</summary>
public class CatalogQueryService
{
    private readonly IRestaurantRepository _restaurants;

    public CatalogQueryService(IRestaurantRepository restaurants)
    {
        _restaurants = restaurants;
    }

    public async Task<IReadOnlyList<RestaurantSummary>> SearchAsync(string? query, string? cuisine,
        decimal? maxDeliveryFee, int? maxDeliveryMinutes, bool onlyOpen,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Restaurant> found = await _restaurants.SearchAsync(query, cuisine, maxDeliveryFee,
            maxDeliveryMinutes, onlyOpen, cancellationToken);
        return found.Select(RestaurantSummary.From).ToList();
    }

    public async Task<RestaurantDetail> BySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        Restaurant restaurant = await _restaurants.BySlugAsync(slug, cancellationToken)
                                ?? throw DomainException.NotFound("restaurant.not_found",
                                    $"Restaurant {slug} bestaat niet.");
        return RestaurantDetail.From(restaurant);
    }

    public Task<IReadOnlyList<string>> CuisinesAsync(CancellationToken cancellationToken = default) =>
        _restaurants.CuisinesAsync(cancellationToken);
}
