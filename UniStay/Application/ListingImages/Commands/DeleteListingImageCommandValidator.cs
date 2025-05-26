using FluentValidation;

namespace Application.ListingImages.Commands
{
    public class DeleteListingImageCommandValidator : AbstractValidator<DeleteListingImageCommand>
    {
        public DeleteListingImageCommandValidator()
        {
            RuleFor(x => x.ListingImageId)
                .NotEmpty().WithMessage("Ідентифікатор зображення (ListingImageId) є обов'язковим.");

            RuleFor(x => x.RequestingUserId)
                .NotEmpty().WithMessage("Ідентифікатор користувача (RequestingUserId) є обов'язковим (має встановлюватися сервером).");
        }
    }
}