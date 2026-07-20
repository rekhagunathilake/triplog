using Triplog.Media.Domain.Common;

namespace Triplog.Media.Application.Abstractions;

public interface ICurrentUser
{
    public OwnerId UserId { get; }
}
