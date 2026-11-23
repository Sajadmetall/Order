using Commerce.Application.Features.Payments;
using Commerce.Infrastructure.Persistence;
using Commerce.Integration.Tests.Fixtures;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Commerce.Integration.Tests.Workflows;

[Trait("Category", "Integration")]
public sealed class OutboxDeliveryTests(CommerceFixture infrastructure)
    : PurchaseTestBase(infrastructure), IClassFixture<CommerceFixture>
{
    [Fact]
    public async Task Committed_outcome_survives_host_restart_and_is_delivered_to_RabbitMQ()
    {
        var orderId = await CreateOrderAsync();
        var paymentId = await SendAsync(new PreparePaymentCommand(orderId));
        var attempt = await SendAsync(new StartPaymentAttemptCommand(paymentId));
        var settlement = new SettlePaymentCommand(paymentId, attempt, 10, "EUR", "delivery-result");
        await SendAsync(settlement);

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            Assert.Equal(1, await context.Set<OutboxMessage>().CountAsync());
        }

        // Establish the subscriber before restarting the publisher. RabbitMQ
        // publishes to bound queues; a subscriber must exist to receive an event.
        var received = new TaskCompletionSource<OrderPaidMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var rabbit = new Uri(RabbitMqConnectionString);
        var credentials = rabbit.UserInfo.Split(':', 2);
        var receiver = Bus.Factory.CreateUsingRabbitMq(bus =>
        {
            bus.Host(rabbit.Host, (ushort)rabbit.Port, "/", host =>
            {
                host.Username(Uri.UnescapeDataString(credentials[0]));
                host.Password(Uri.UnescapeDataString(credentials[1]));
            });
            bus.ReceiveEndpoint("purchase-test-" + Guid.NewGuid().ToString("N"), endpoint =>
            {
                endpoint.Durable = false;
                endpoint.AutoDelete = true;
                endpoint.Handler<OrderPaidMessage>(context =>
                {
                    if (context.Message.OrderId == orderId)
                        received.TrySetResult(context.Message);
                    return Task.CompletedTask;
                });
            });
        });

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        await receiver.StartAsync(timeout.Token);
        try
        {
            await RestartHostWithDeliveryAsync();
            var outcome = await received.Task.WaitAsync(timeout.Token);
            Assert.Equal(orderId, outcome.OrderId);
            Assert.Equal(paymentId, outcome.PaymentId);
            Assert.Equal(10m, outcome.Amount);
            Assert.Equal("EUR", outcome.Currency);
            Assert.Equal("Paid", (await ReadOrderAsync(orderId)).Status);

            // Delivery acknowledgement and outbox cleanup are asynchronous.
            while (true)
            {
                await using var scope = Factory.Services.CreateAsyncScope();
                var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                if (!await context.Set<OutboxMessage>().AnyAsync(timeout.Token))
                    break;
                await Task.Delay(TimeSpan.FromMilliseconds(100), timeout.Token);
            }

            await SendAsync(settlement);
            await using var verificationScope = Factory.Services.CreateAsyncScope();
            var saved = verificationScope.ServiceProvider.GetRequiredService<OrderDbContext>();
            Assert.Empty(await saved.Set<OutboxMessage>().ToListAsync(timeout.Token));
        }
        finally
        {
            await receiver.StopAsync(CancellationToken.None);
        }
    }
}
