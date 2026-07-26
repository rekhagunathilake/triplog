using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Triplog.Media.Api.Auth;

namespace Triplog.Media.Api.Http;

public sealed class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            ValidationException v => BuildValidationProblem(v),
            MissingUserHeaderException m => BuildAuthProblem(m.Message),
            InvalidUserHeaderException i => BuildAuthProblem(i.Message),
            _ => (ProblemDetails?)null
        };

        if (problem is null)
            return false;

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }

    private static ValidationProblemDetails BuildValidationProblem(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Title = "Validation failed",
            Status = StatusCodes.Status400BadRequest,
            Type = "https://triplog/errors/validation"
        };
    }

    private static ProblemDetails BuildAuthProblem(string detail) => new()
    {
        Status = StatusCodes.Status401Unauthorized,
        Title = "Unauthorized",
        Detail = detail,
        Type = "https://triplog/errors/unauthorized"
    };
}
