using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Domain.Common;

namespace Triplog.Entries.Api.Auth;

public sealed class ConfiguredOwnerCurrentUser : ICurrentUser
{
    public OwnerId UserId { get; }

    public ConfiguredOwnerCurrentUser(IConfiguration configuration)
    {
        var raw = configuration["Auth:PublicOwnerId"]
            ?? throw new InvalidOperationException("Missing Auth:PublicOwnerId");
        UserId = new OwnerId(Guid.Parse(raw));
    }
}