using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Domain.Common;

namespace Triplog.Entries.Api.Auth;

public sealed class HeaderCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private const string HeaderName = "X-User-Id";

    public OwnerId UserId
    {
        get 
        {
            var context = accessor.HttpContext ?? throw new InvalidOperationException("No HTTP context available.");

            if (!context.Request.Headers.TryGetValue(HeaderName, out var userIdHeader))
                throw new MissingUserHeaderException(HeaderName);

            if (!Guid.TryParse(userIdHeader.ToString(), out var userId))
                throw new InvalidUserHeaderException(HeaderName, userIdHeader.ToString());

            return new OwnerId(userId);
        }
    }
}

public sealed class MissingUserHeaderException(string headerName)
    : Exception($"Missing required '{headerName}' header.");

public sealed class InvalidUserHeaderException(string headerName, string headerValue)
    : Exception($"Invalid value '{headerValue}' for '{headerName}' header. Expected a valid GUID.");
