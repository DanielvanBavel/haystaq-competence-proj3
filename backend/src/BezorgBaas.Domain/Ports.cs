using BezorgBaas.Domain.Catalog;
using BezorgBaas.Domain.Ordering;
using BezorgBaas.Domain.Promotions;

namespace BezorgBaas.Domain;

/// <summary>Domeinpoorten. De implementaties staan in de infrastructuurlaag.</summary>
public interface IRestaurantRepository
{
    Task<IReadOnlyList<Restaurant>> SearchAsync(string? query, string? cuisine, decimal? maxDeliveryFee,
        int? maxDeliveryMinutes, bool onlyOpen, CancellationToken cancellationToken = default);

    Task<Restaurant?> BySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<Restaurant?> ByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> CuisinesAsync(CancellationToken cancellationToken = default);
}

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    Task<Order?> ByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Order?> ByNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> ForRestaurantAsync(Guid restaurantId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> ForCustomerAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> CustomerUsedPromoAsync(string email, string promoCode, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IPromoCodeRepository
{
    Task<PromoCode?> ByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
