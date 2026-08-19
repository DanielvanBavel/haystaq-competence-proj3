using BezorgBaas.Application;
using Microsoft.AspNetCore.Mvc;

namespace BezorgBaas.Api.Controllers;

[ApiController]
[Route("api")]
public class CatalogController : ControllerBase
{
    private readonly CatalogQueryService _catalog;

    public CatalogController(CatalogQueryService catalog)
    {
        _catalog = catalog;
    }

    [HttpGet("restaurants")]
    public Task<IReadOnlyList<RestaurantSummary>> Search(
        [FromQuery] string? query,
        [FromQuery] string? cuisine,
        [FromQuery] decimal? maxDeliveryFee,
        [FromQuery] int? maxDeliveryMinutes,
        [FromQuery] bool onlyOpen = false,
        CancellationToken cancellationToken = default) =>
        _catalog.SearchAsync(query, cuisine, maxDeliveryFee, maxDeliveryMinutes, onlyOpen, cancellationToken);

    [HttpGet("restaurants/{slug}")]
    public Task<RestaurantDetail> BySlug(string slug, CancellationToken cancellationToken) =>
        _catalog.BySlugAsync(slug, cancellationToken);

    [HttpGet("cuisines")]
    public Task<IReadOnlyList<string>> Cuisines(CancellationToken cancellationToken) =>
        _catalog.CuisinesAsync(cancellationToken);

    /// <summary>Bezorgmomenten voor een datum. Vandaag vervallen de momenten die al voorbij zijn.</summary>
    [HttpGet("delivery-slots")]
    public IReadOnlyList<string> Slots([FromQuery] DateOnly? date)
    {
        DateOnly day = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        bool isToday = day == DateOnly.FromDateTime(DateTime.UtcNow);
        TimeOnly now = TimeOnly.FromDateTime(DateTime.UtcNow);

        List<string> slots = new();
        for (int hour = 16; hour <= 21; hour++)
        {
            foreach (int minute in new[] { 0, 30 })
            {
                TimeOnly start = new(hour, minute);
                TimeOnly end = start.AddMinutes(30);
                if (isToday && start < now.AddMinutes(45))
                {
                    continue;
                }
                slots.Add($"{start:HH\\:mm}-{end:HH\\:mm}");
            }
        }
        return slots;
    }
}
