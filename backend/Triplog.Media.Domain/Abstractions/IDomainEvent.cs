using MediatR;

namespace Triplog.Media.Domain.Abstractions;

public interface IDomainEvent : INotification
{
    DateTime OccurredOnUtc { get; }
}
