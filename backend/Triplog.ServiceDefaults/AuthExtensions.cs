using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Triplog.ServiceDefaults;

public static class AuthExtensions
{
    public const string OwnerPolicy = "OwnerOnly";

    public static IServiceCollection AddTriplogAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var secret = configuration["Auth:JwtSecret"] ?? throw new InvalidOperationException("Missing Auth:JwtSecret");
        var ownerEmail = configuration["Auth:OwnerEmail"] ?? throw new InvalidOperationException("Missing Auth:OwnerEmail");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
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

        services.AddAuthorization(options => {
            options.AddPolicy(OwnerPolicy, policy =>
            {
                policy.RequireAssertion(ctx =>
                {
                    var email = ctx.User.FindFirst(ClaimTypes.Email)?.Value ??
                        ctx.User.FindFirst("email")?.Value;
                    return string.Equals(email, ownerEmail, StringComparison.OrdinalIgnoreCase);
                });
            });
        });

        return services;
    }
}
