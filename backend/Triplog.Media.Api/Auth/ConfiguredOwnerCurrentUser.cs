using Triplog.Media.Application.Abstractions;
using Triplog.Media.Domain.Common;

namespace Triplog.Media.Api.Auth;

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