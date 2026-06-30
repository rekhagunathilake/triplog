using Triplog.Entries.Domain.Common;

namespace Triplog.Entries.Application.Abstractions;

public interface ICurrentUser
{
    public OwnerId UserId { get; }
}
