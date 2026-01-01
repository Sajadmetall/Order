using Domain.Orders;
using Microsoft.AspNetCore.Mvc;
using Order.DTO;
using System.Runtime.CompilerServices;

namespace Order.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _repo;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderRepository repo, ILogger<OrderController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        // POST /orders
        [HttpPost]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
        {

            var validation = ValidateCreateRequest(request);
            if (validation is not null)
                return validation;

            Domain.Orders.Order order;
            try
            {
                order = OrderMapping.ToDomain(request);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            await _repo.AddAsync(order, ct);
            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("Order created. Id={OrderId}, Customer={CustomerName}", order.Id, order.CustomerName);

            var response = OrderMapping.ToDto(order);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, response);
        }

        // GET /orders/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
        {
            var order = await _repo.GetByIdAsync(new OrderId(id), ct);
            if (order is null)
                return NotFound();

            return Ok(OrderMapping.ToDto(order));
        }

        // GET /orders
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var orders = await _repo.GetAllAsync(ct);
            return Ok(orders);
        }

        //// PUT /orders/{id}/status
        //[HttpPut("{id:guid}/status")]
        //[ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> ChangeStatus([FromRoute] Guid id, [FromBody] ChangeOrderStatusRequest request, CancellationToken ct)
        //{
        //    var order = await _repo.GetByIdAsync(new OrderId(id), ct);
        //    if (order is null)
        //        return NotFound();

        //    try
        //    {
        //        ApplyStatusChange(order, request.NewStatus);
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        // Could also be 409 Conflict; for live coding 400 is acceptable
        //        return BadRequest(ex.Message);
        //    }

        //    await _repo.SaveChangesAsync(ct);

        //    _logger.LogInformation("Order status changed. Id={OrderId}, Status={Status}", order.Id, order.Status);

        //    return Ok(MapToResponse(order));
        //}

        // ---------- Helpers ----------

        private static IActionResult? ValidateCreateRequest(CreateOrderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerName))
                return new BadRequestObjectResult("CustomerName is required.");

            if (request.Items is null || request.Items.Count == 0)
                return new BadRequestObjectResult("At least one item is required.");

            if (request.Items.Any(i => string.IsNullOrWhiteSpace(i.ProductName)))
                return new BadRequestObjectResult("ProductName is required.");

            if (request.Items.Any(i => i.Quantity <= 0))
                return new BadRequestObjectResult("Quantity must be greater than zero.");

            if (request.Items.Any(i => i.UnitPriceAmount <= 0))
                return new BadRequestObjectResult("UnitPrice must be greater than zero.");

            return null;
        }


        

    }

}
