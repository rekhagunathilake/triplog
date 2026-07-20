using FluentValidation;

namespace Triplog.Media.Application.MediaItems.Commands.CreateMediaItem;

public sealed class CreateMediaItemCommandValidator : AbstractValidator<CreateMediaItemCommand>
{
    public CreateMediaItemCommandValidator()
    {
        RuleFor(x => x.BlobKey).NotEmpty();
        RuleFor(x => x.ContentType).NotEmpty();
        RuleFor(x => x.SizeInBytes).GreaterThan(0)
            .WithMessage("SizeInBytes must be greater than 0.");
        RuleFor(x => x.OriginalFileName).NotEmpty()
            .MaximumLength(255);
    }
}
