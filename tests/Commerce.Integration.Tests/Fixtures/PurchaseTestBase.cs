using System.Net;
using System.Net.Http.Json;
using Commerce.Application.Features.Orders.Commands.CreateOrder;
using Commerce.Application.Features.Orders.Dtos;
using Commerce.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Commerce.Integration.Tests.Fixtures;

public abstract class PurchaseTestBase(CommerceFixture infrastructure) : IAsyncLifetime
{
    private string _connectionString = string.Empty;
    protected CommerceApiFactory Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;
    protected string RabbitMqConnectionString => infrastructure.RabbitMqConnectionString;

    public async Task InitializeAsync()
    {
        var connection = new SqlConnectionStringBuilder(infrastructure.ConnectionString)
        {
            InitialCatalog = "CommerceIntegration_" + Guid.NewGuid().ToString("N")
        };
        _connectionString = connection.ConnectionString;
        // Migrate before hosted services start querying the outbox.
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseSqlServer(_connectionString).Options;
        await using (var context = new OrderDbContext(options))
            await context.Database.MigrateAsync();

        StartHost(deliverOutbox: false);
    }

    protected async Task RestartHostWithDeliveryAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
        StartHost(deliverOutbox: true);
    }

    private void StartHost(bool deliverOutbox)
    {
        Factory = new CommerceApiFactory(
            _connectionString, infrastructure.RabbitMqConnectionString, deliverOutbox);
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (Factory is not null)
            await Factory.DisposeAsync();

        // The database name and credentials always originate from the temporary container.
        // A separate context still permits cleanup after a failed web-host initialization.
        if (!string.IsNullOrEmpty(_connectionString))
        {
            var options = new DbContextOptionsBuilder<OrderDbContext>()
                .UseSqlServer(_connectionString).Options;
            await using var context = new OrderDbContext(options);
            await context.Database.EnsureDeletedAsync();
        }
    }

    protected async Task<Guid> CreateOrderAsync(decimal unitPrice = 2.5m, int quantity = 4)
    {
        var command = new CreateOrderCommand("Integration test customer",
            [new CreateOrderItemDto("Premium period", quantity, unitPrice, "EUR")]);
        using var response = await Client.PostAsJsonAsync("/api/Order", command);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    protected async Task<OrderDto> ReadOrderAsync(Guid orderId)
    {
        using var response = await Client.GetAsync($"/api/Order/{orderId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<OrderDto>())!;
    }

    protected async Task<T> SendAsync<T>(IRequest<T> request)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IMediator>().Send(request);
    }

    protected async Task SendAsync(IRequest request)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IMediator>().Send(request);
    }
}
