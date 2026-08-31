using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Triplog.ServiceDefaults;

public static class AuthExtensions
{
    public const string OwnerPolicy = "OwnerOnly";

    public static IServiceCollection AddTriplogAuth(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        // Late-bind JqtBearer options: read config at resolve time, not DI-registration time.
        // Fixes WebApplicationFactory's InMemory overrides being applied too late.
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IConfiguration>((options, configuration) =>
            {
                var secret = configuration["Auth:JwtSecret"] ??
                    throw new InvalidOperationException("Missing Auth:JwtSecret");
                //var ownerEmail = configuration["Auth:OwnerEmail"] ?? throw new InvalidOperationException("Missing Auth:OwnerEmail");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = false, // single-issuer setup, skip issuer checks
                    ValidateAudience = false, // ditto for audience
                    ValidateLifetime = true, // still enforce exp claim
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        services.AddAuthorization(options =>
            options.AddPolicy(OwnerPolicy, policy =>
                policy.AddRequirements(new OwnerRequirement())));

        services.AddSingleton<IAuthorizationHandler, OwnerHandler>();

        return services;
    }
}

public sealed class OwnerRequirement : IAuthorizationRequirement { }

public sealed class OwnerHandler(IConfiguration configuration)
    : AuthorizationHandler<OwnerRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, OwnerRequirement requirement)
    {
        var ownerEmail = configuration["Auth:OwnerEmail"];
        var email = context.User.FindFirst(ClaimTypes.Email)?.Value
                 ?? context.User.FindFirst("email")?.Value;

        if (string.Equals(email, ownerEmail, StringComparison.OrdinalIgnoreCase))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
