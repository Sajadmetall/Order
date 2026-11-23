using Commerce.Application.Abstractions.Messaging;
using MassTransit;

namespace Commerce.Infrastructure.Messaging;

// The scoped endpoint is intercepted by the EF Bus Outbox. Publishing through
// IBus instead would bypass the transaction associated with OrderDbContext.
public sealed class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class => publishEndpoint.Publish(@event, cancellationToken);
}
