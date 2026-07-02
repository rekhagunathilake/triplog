using FluentValidation;

namespace Triplog.Entries.Application.Entries.Commands.UpdateContent;

public sealed class UpdateContentValidator : AbstractValidator<UpdateContentCommand>
{
    public UpdateContentValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(50_000);

        // Location: all-or-nothing — either all three fields present or none
        When(x => x.LocationName is not null || x.Latitude.HasValue || x.Longitude.HasValue, () =>
        {
            RuleFor(x => x.LocationName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Latitude).NotNull().InclusiveBetween(-90, 90);
            RuleFor(x => x.Longitude).NotNull().InclusiveBetween(-180, 180);
        });
    }
}
