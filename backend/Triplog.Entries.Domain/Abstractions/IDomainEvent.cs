using MediatR;

namespace Triplog.Entries.Domain.Abstractions;

public interface IDomainEvent : INotification
{
    DateTime OccurredOnUtc { get; }
}
