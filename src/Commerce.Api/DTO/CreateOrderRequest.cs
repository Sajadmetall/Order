using System.Collections.Generic;

namespace Commerce.Api.DTO
{
    public sealed record CreateOrderRequest(
     string CustomerName,
     List<CreateOrderItemRequest> Items);
}
