using Commerce.Infrastructure.Persistence;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Commerce.Integration.Tests.Fixtures;

public sealed class CommerceApiFactory(
    string connectionString, string rabbitMqConnectionString, bool deliverOutbox = false)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var rabbit = new Uri(rabbitMqConnectionString);
        var credentials = rabbit.UserInfo.Split(':', 2);
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["RabbitMq:Host"] = rabbit.Host,
                ["RabbitMq:Port"] = rabbit.Port.ToString(),
                ["RabbitMq:VirtualHost"] = "/",
                ["RabbitMq:Username"] = Uri.UnescapeDataString(credentials[0]),
                ["RabbitMq:Password"] = Uri.UnescapeDataString(credentials[1])
            }));
        builder.ConfigureTestServices(services =>
        {
            // Pause delivery only in tests which inspect committed outbox rows.
            // The recovery test enables the real production delivery service.
            var backgroundServices = services.Where(descriptor =>
                descriptor.ServiceType == typeof(IHostedService) &&
                (descriptor.ImplementationType == typeof(InboxCleanupService<OrderDbContext>) ||
                 (!deliverOutbox && descriptor.ImplementationType == typeof(BusOutboxDeliveryService<OrderDbContext>))))
                .ToArray();
            foreach (var descriptor in backgroundServices)
                services.Remove(descriptor);
        });
    }
}
