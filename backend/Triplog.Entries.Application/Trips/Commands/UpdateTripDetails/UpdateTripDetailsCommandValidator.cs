using FluentValidation;

namespace Triplog.Entries.Application.Trips.Commands.UpdateTripDetails;

public sealed class UpdateTripDetailsCommandValidator : AbstractValidator<UpdateTripDetailsCommand>
{
    public UpdateTripDetailsCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after start date.");
    }
}