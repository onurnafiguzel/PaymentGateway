namespace Shared.Kernel.Domain.Events;

public abstract class DomainEventBase : IDomainEvent
{
    public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;
}