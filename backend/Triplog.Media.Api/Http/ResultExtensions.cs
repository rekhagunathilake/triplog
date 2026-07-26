using Triplog.Media.Domain.Abstractions;

namespace Triplog.Media.Api.Http;

public static class ResultExtensions
{
    public static IResult ToOkResult<T>(this Result<T> result) =>
        result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblem(result.Error);

    public static IResult ToNoContentResult(this Result result) =>
        result.IsSuccess
            ? Results.NoContent()
            : ToProblem(result.Error);

    public static IResult ToCreatedResult<T>(this Result<T> result, Func<T, string> locationBuilder) =>
        result.IsSuccess
            ? Results.Created(locationBuilder(result.Value), new { id = result.Value })
            : ToProblem(result.Error);

    private static IResult ToProblem(Error error)
    {
        var status = MapErrorToStatus(error.Code);
        return Results.Problem(
            title: error.Code,
            detail: error.Message,
            statusCode: status,
            type: $"https://triplog/errors/{error.Code}");
    }

    private static int MapErrorToStatus(string errorCode)
    {
        if (errorCode.EndsWith(".NotFound", StringComparison.Ordinal))
            return StatusCodes.Status404NotFound;

        // Conflict errors - the requested action collides with the current state of the resource
        if (errorCode.EndsWith(".AlreadyArchived", StringComparison.Ordinal) ||
            errorCode.EndsWith(".AlreadyExists", StringComparison.Ordinal) ||
            errorCode.EndsWith(".MediaAlreadyAttached", StringComparison.Ordinal) ||
            errorCode.EndsWith(".IsArchived", StringComparison.Ordinal) ||
            errorCode.EndsWith(".NotDraft", StringComparison.Ordinal) ||
            errorCode.EndsWith(".InvalidStatusTransition", StringComparison.Ordinal))
            return StatusCodes.Status409Conflict;

        return StatusCodes.Status400BadRequest;
    }
}
