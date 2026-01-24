using Domain.ValueObjects;

namespace Domain.Tests;

/// <summary>
/// Provides predefined test identifiers for consistent testing.
/// </summary>
public static class TestIds
{
    public static OrderId DefaultOrderId => OrderId.Create(new Guid("11111111-1111-1111-1111-111111111111"));
    
    public static OrderId OrderId1 => OrderId.Create(new Guid("22222222-2222-2222-2222-222222222222"));
    
    public static OrderId OrderId2 => OrderId.Create(new Guid("33333333-3333-3333-3333-333333333333"));
    
    public static OrderId OrderId3 => OrderId.Create(new Guid("44444444-4444-4444-4444-444444444444"));
    
    public static Guid DefaultGuid => new Guid("11111111-1111-1111-1111-111111111111");
    
    public static Guid Guid1 => new Guid("22222222-2222-2222-2222-222222222222");
    
    public static Guid Guid2 => new Guid("33333333-3333-3333-3333-333333333333");
    
    public static Guid Guid3 => new Guid("44444444-4444-4444-4444-444444444444");
}