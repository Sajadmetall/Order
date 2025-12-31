using System.Collections.Generic;

namespace Order.DTO
{
    public sealed class CreateOrderRequest
    {
        public string? CustomerName { get; init; }
        public List<CreateOrderItemRequest>? Items { get; init; }
    }
}
