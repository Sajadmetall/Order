using Domain.Contracts;
using Domain.Orders;
namespace Infrastructure.Repositories
{
    public sealed class InMemoryOrderRepository : IOrderRepository
    {
        private readonly Dictionary<OrderId, Order> _store = new();

        public Task AddAsync(Order order, CancellationToken ct = default)
        {
            _store[order.Id] = order;
            return Task.CompletedTask;
        }

        public Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct = default)
        {
            _store.TryGetValue(id, out var order);
            return Task.FromResult(order);
        }

        public Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default)
        {
            IReadOnlyList<Order> orders = _store.Values.ToList();
            return Task.FromResult(orders);
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
            => Task.CompletedTask;
    }
}
