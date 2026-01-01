using System.Collections.Generic;

namespace Order.DTO
{
    public sealed record CreateOrderRequest(
     string CustomerName,
     List<CreateOrderItemRequest> Items);
}
