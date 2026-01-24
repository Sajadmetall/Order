using Domain.Orders;

namespace Domain.Tests.TestDoubles;

public static class FakeIds
{
    public static OrderId OrderId1 => new OrderId(GuidId1);
    public static OrderId OrderId2 => new OrderId(GuidId2);
    public static OrderId OrderId3 => new OrderId(GuidId3);
    
    public static Guid GuidId1 => new Guid("11111111-1111-1111-1111-111111111111");
    public static Guid GuidId2 => new Guid("22222222-2222-2222-2222-222222222222");
    public static Guid GuidId3 => new Guid("33333333-3333-3333-3333-333333333333");
}
