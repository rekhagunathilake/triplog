using MediatR;
using Triplog.Entries.Api.Endpoints.Requests;
using Triplog.Entries.Api.Http;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Application.Trips.Commands.ActivateTrip;
using Triplog.Entries.Application.Trips.Commands.ArchiveTrip;
using Triplog.Entries.Application.Trips.Commands.CompleteTrip;
using Triplog.Entries.Application.Trips.Commands.CreateTrip;
using Triplog.Entries.Application.Trips.Commands.UpdateTripDetails;
using Triplog.Entries.Application.Trips.Queries.GetTripById;
using Triplog.Entries.Application.Trips.Queries.ListTripsByOwner;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Api.Endpoints;

public static class TripEndpoints
{
    public static void MapTripEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/trips")
            .WithTags("Trips");

        group.MapPost("/", async (CreateTripRequest request, ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateTripCommand(currentUser.UserId, request.Title, request.Description, request.StartDate, request.EndDate), ct);

            return result.ToCreatedResult(id => $"/trips/{id.Value}");
        });

        group.MapPut("/{id:guid}", async (
            Guid id, UpdateTripDetailsRequest request, ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateTripDetailsCommand(new TripId(id), currentUser.UserId, request.Title, request.Description, request.StartDate, request.EndDate), ct);

            return result.ToNoContentResult();
        });

        group.MapPost("/{id:guid}/activate", async (
            Guid id, ICurrentUser user, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ActivateTripCommand(new TripId(id), user.UserId), ct);
            return result.ToNoContentResult();
        });

        group.MapPost("/{id:guid}/complete", async (
            Guid id, ICurrentUser user, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CompleteTripCommand(new TripId(id), user.UserId), ct);
            return result.ToNoContentResult();
        });

        group.MapPost("/{id:guid}/archive", async (
            Guid id, ICurrentUser user, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ArchiveTripCommand(new TripId(id), user.UserId), ct);
            return result.ToNoContentResult();
        });

        group.MapGet("/{id:guid}", async (
            Guid id, ICurrentUser user, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetTripByIdQuery(new TripId(id), user.UserId), ct);
            return result.ToOkResult();
        });

        group.MapGet("/", async (
            ICurrentUser user, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ListTripsByOwnerQuery(user.UserId), ct);
            return result.ToOkResult();
        });
    }
}
