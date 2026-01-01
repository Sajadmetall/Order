using System.Threading;
using System.Threading.Tasks;

namespace Domain.Orders
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct = default);
        Task AddAsync(Order order, CancellationToken ct = default);
        Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
