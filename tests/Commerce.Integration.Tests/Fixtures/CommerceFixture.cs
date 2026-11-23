using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace Commerce.Integration.Tests.Fixtures;

public sealed class CommerceFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlServer = new MsSqlBuilder(
        "mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04").Build();
    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder("rabbitmq:4.2.0-alpine")
        .WithUsername("integration")
        .WithPassword(Guid.NewGuid().ToString("N"))
        .Build();

    public string ConnectionString => _sqlServer.GetConnectionString();
    public string RabbitMqConnectionString => _rabbitMq.GetConnectionString();

    public Task InitializeAsync() => Task.WhenAll(_sqlServer.StartAsync(), _rabbitMq.StartAsync());

    public async Task DisposeAsync()
    {
        await _rabbitMq.DisposeAsync();
        await _sqlServer.DisposeAsync();
    }
}
