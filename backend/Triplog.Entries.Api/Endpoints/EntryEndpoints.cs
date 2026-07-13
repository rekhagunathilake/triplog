using MediatR;
using Triplog.Entries.Api.Endpoints.Requests;
using Triplog.Entries.Api.Http;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Application.Entries.Commands.ArchiveEntry;
using Triplog.Entries.Application.Entries.Commands.AttachMedia;
using Triplog.Entries.Application.Entries.Commands.CompletePublish;
using Triplog.Entries.Application.Entries.Commands.CreateEntry;
using Triplog.Entries.Application.Entries.Commands.FailPublish;
using Triplog.Entries.Application.Entries.Commands.PublishEntry;
using Triplog.Entries.Application.Entries.Commands.RemoveMedia;
using Triplog.Entries.Application.Entries.Commands.UpdateContent;
using Triplog.Entries.Application.Entries.Queries.GetEntryById;
using Triplog.Entries.Application.Entries.Queries.ListEntriesByTrip;
using Triplog.Entries.Domain.Entries;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Api.Endpoints;

public static class EntryEndpoints
{
    public static void MapEntryEndpoints(this IEndpointRouteBuilder app)
    {
        // Entry-scoped operations under /entries
        var entries = app.MapGroup("/entries").WithTags("Entries");

        entries.MapPut("/{id:guid}/content", async (
        Guid id, UpdateEntryContentRequest req, ICurrentUser user, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateContentCommand(
                new EntryId(id), user.UserId, req.Title, req.Body, req.VisitedOn,
                req.LocationName, req.Latitude, req.Longitude), ct);
            return result.ToNoContentResult();
        });

        entries.MapPost("/{id:guid}/media/{mediaId:guid}", async (
            Guid id, Guid mediaId, ICurrentUser user, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new AttachMediaCommand(
                new EntryId(id), user.UserId, new MediaReferenceId(mediaId)), ct);
            return result.ToNoContentResult();
        });

        entries.MapDelete("/{id:guid}/media/{mediaId:guid}", async (
            Guid id, Guid mediaId, ICurrentUser user, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new RemoveMediaCommand(
                new EntryId(id), user.UserId, new MediaReferenceId(mediaId)), ct);
            return result.ToNoContentResult();
        });

        entries.MapPost("/{id:guid}/publish", async (
            Guid id, ICurrentUser user, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new PublishEntryCommand(new EntryId(id), user.UserId), ct);
            return result.ToNoContentResult();
        });

        // Saga-called endpoints — no OwnerId (internal caller)
        entries.MapPost("/{id:guid}/publish/complete", async (
            Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CompletePublishCommand(new EntryId(id)), ct);
            return result.ToNoContentResult();
        });

        entries.MapPost("/{id:guid}/publish/fail", async (
            Guid id, FailPublishRequest req, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new FailPublishCommand(new EntryId(id), req.Reason), ct);
            return result.ToNoContentResult();
        });

        entries.MapPost("/{id:guid}/archive", async (
            Guid id, ICurrentUser user, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ArchiveEntryCommand(new EntryId(id), user.UserId), ct);
            return result.ToNoContentResult();
        });

        entries.MapGet("/{id:guid}", async (
            Guid id, ICurrentUser user, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetEntryByIdQuery(new EntryId(id), user.UserId), ct);
            return result.ToOkResult();
        });

        // Trip-scoped entry operations under /trips/{tripId}/entries
        var tripEntries = app.MapGroup("/trips/{tripId:guid}/entries").WithTags("Entries");

        tripEntries.MapPost("/", async (
            Guid tripId, CreateEntryRequest req, ICurrentUser user, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateEntryCommand(
                new TripId(tripId), user.UserId, req.Title, req.Body, req.VisitedOn,
                req.LocationName, req.Latitude, req.Longitude), ct);
            return result.ToCreatedResult(id => $"/entries/{id.Value}");
        });

        tripEntries.MapGet("/", async (
            Guid tripId, ICurrentUser user, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ListEntriesByTripQuery(
                new TripId(tripId), user.UserId), ct);
            return result.ToOkResult();
        });
    }
}
