using BezorgBaas.Application;
using Microsoft.AspNetCore.Mvc;

namespace BezorgBaas.Api.Controllers;

[ApiController]
[Route("api")]
public class OrdersController : ControllerBase
{
    private readonly OrderingService _ordering;

    public OrdersController(OrderingService ordering)
    {
        _ordering = ordering;
    }

    [HttpPost("orders/quote")]
    public Task<Quote> Quote([FromBody] QuoteRequest request, CancellationToken cancellationToken) =>
        _ordering.QuoteAsync(request, cancellationToken);

    [HttpPost("orders")]
    public async Task<ActionResult<OrderView>> Place([FromBody] PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        OrderView order = await _ordering.PlaceAsync(request, cancellationToken);
        return Created($"/api/orders/{order.OrderNumber}", order);
    }

    [HttpGet("orders/{orderNumber}")]
    public Task<OrderView> ByNumber(string orderNumber, CancellationToken cancellationToken) =>
        _ordering.ByNumberAsync(orderNumber, cancellationToken);

    [HttpGet("orders")]
    public Task<IReadOnlyList<OrderView>> ForCustomer([FromQuery] string email,
        CancellationToken cancellationToken) =>
        _ordering.ForCustomerAsync(email, cancellationToken);

    [HttpGet("restaurants/{restaurantId:guid}/orders")]
    public Task<IReadOnlyList<OrderView>> ForRestaurant(Guid restaurantId, CancellationToken cancellationToken) =>
        _ordering.ForRestaurantAsync(restaurantId, cancellationToken);

    public record AdvanceRequest(string? Status, string? Note);

    [HttpPost("orders/{id:guid}/advance")]
    public Task<OrderView> Advance(Guid id, [FromBody] AdvanceRequest? request,
        CancellationToken cancellationToken) =>
        _ordering.AdvanceAsync(id, request?.Status, request?.Note, cancellationToken);

    public record CancelRequest(string Reason);

    [HttpPost("orders/{id:guid}/cancel")]
    public Task<OrderView> Cancel(Guid id, [FromBody] CancelRequest request, CancellationToken cancellationToken) =>
        _ordering.CancelAsync(id, request.Reason, cancellationToken);
}
