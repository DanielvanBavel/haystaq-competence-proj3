using BezorgBaas.Domain;
using BezorgBaas.Domain.Catalog;
using BezorgBaas.Domain.Ordering;
using BezorgBaas.Domain.Promotions;
using Microsoft.EntityFrameworkCore;

namespace BezorgBaas.Infrastructure;

public class RestaurantRepository : IRestaurantRepository
{
    private readonly BezorgBaasDbContext _context;

    public RestaurantRepository(BezorgBaasDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Restaurant>> SearchAsync(string? query, string? cuisine, decimal? maxDeliveryFee,
        int? maxDeliveryMinutes, bool onlyOpen, CancellationToken cancellationToken = default)
    {
        IQueryable<Restaurant> restaurants = _context.Restaurants.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            string term = query.Trim().ToLower();
            restaurants = restaurants.Where(restaurant =>
                restaurant.Name.ToLower().Contains(term) || restaurant.Cuisine.ToLower().Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(cuisine))
        {
            string value = cuisine.Trim().ToLower();
            restaurants = restaurants.Where(restaurant => restaurant.Cuisine.ToLower() == value);
        }
        if (maxDeliveryMinutes is { } minutes)
        {
            restaurants = restaurants.Where(restaurant => restaurant.EstimatedDeliveryMinutes <= minutes);
        }
        if (onlyOpen)
        {
            restaurants = restaurants.Where(restaurant => restaurant.IsOpen);
        }

        List<Restaurant> found = await restaurants
            .OrderByDescending(restaurant => restaurant.Rating)
            .ThenBy(restaurant => restaurant.Name)
            .ToListAsync(cancellationToken);

        // De bezorgkostenfilter werkt op het waarde-object en gebeurt daarom in geheugen.
        if (maxDeliveryFee is { } fee)
        {
            found = found.Where(restaurant => restaurant.DeliveryFee.Amount <= fee).ToList();
        }

        return found;
    }

    public Task<Restaurant?> BySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        _context.Restaurants.FirstOrDefaultAsync(restaurant => restaurant.Slug == slug, cancellationToken);

    public Task<Restaurant?> ByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Restaurants.FirstOrDefaultAsync(restaurant => restaurant.Id == id, cancellationToken);

    public async Task<IReadOnlyList<string>> CuisinesAsync(CancellationToken cancellationToken = default) =>
        await _context.Restaurants
            .Select(restaurant => restaurant.Cuisine)
            .Distinct()
            .OrderBy(cuisine => cuisine)
            .ToListAsync(cancellationToken);
}

public class OrderRepository : IOrderRepository
{
    private readonly BezorgBaasDbContext _context;

    public OrderRepository(BezorgBaasDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default) =>
        await _context.Orders.AddAsync(order, cancellationToken);

    public Task<Order?> ByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Orders.FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

    public Task<Order?> ByNumberAsync(string orderNumber, CancellationToken cancellationToken = default) =>
        _context.Orders.FirstOrDefaultAsync(order => order.OrderNumber == orderNumber, cancellationToken);

    public async Task<IReadOnlyList<Order>> ForRestaurantAsync(Guid restaurantId,
        CancellationToken cancellationToken = default) =>
        await _context.Orders
            .Where(order => order.RestaurantId == restaurantId)
            .OrderByDescending(order => order.PlacedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Order>> ForCustomerAsync(string email,
        CancellationToken cancellationToken = default) =>
        await _context.Orders
            .Where(order => order.CustomerEmail == email.ToLower())
            .OrderByDescending(order => order.PlacedAt)
            .ToListAsync(cancellationToken);

    public Task<bool> CustomerUsedPromoAsync(string email, string promoCode,
        CancellationToken cancellationToken = default) =>
        _context.Orders.AnyAsync(order => order.CustomerEmail == email.ToLower() && order.PromoCode == promoCode,
            cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        _context.Orders.CountAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}

public class PromoCodeRepository : IPromoCodeRepository
{
    private readonly BezorgBaasDbContext _context;

    public PromoCodeRepository(BezorgBaasDbContext context)
    {
        _context = context;
    }

    public Task<PromoCode?> ByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        _context.PromoCodes.FirstOrDefaultAsync(promo => promo.Code == code, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
