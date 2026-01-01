namespace Domain.Orders;

public enum OrderStatus
{
    Created = 0,
    Submitted = 1,
    Paid = 2,
    Shipped = 3,
    Confirmed = 4,
    Cancelled = 5
}
