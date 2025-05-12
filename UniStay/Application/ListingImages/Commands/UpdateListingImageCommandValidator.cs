// Файл: Application/ListingImages/Commands/UpdateListingImageCommandValidator.cs
using FluentValidation;
using System; // Для Uri

namespace Application.ListingImages.Commands
{
    public class UpdateListingImageCommandValidator : AbstractValidator<UpdateListingImageCommand>
    {
        public UpdateListingImageCommandValidator()
        {
            RuleFor(x => x.ListingImageId)
                .NotEmpty().WithMessage("Ідентифікатор зображення (ListingImageId) є обов'язковим.");

            RuleFor(x => x.RequestingUserId)
                .NotEmpty().WithMessage("Ідентифікатор користувача (RequestingUserId) є обов'язковим (має встановлюватися сервером).");

            RuleFor(x => x.NewImageUrl)
                .NotEmpty().WithMessage("Новий URL зображення є обов'язковим.")
                .MaximumLength(2048).WithMessage("URL зображення не може перевищувати 2048 символів.")
                .Must(BeAValidUrl).WithMessage("Надано недійсний URL зображення.");
        }

        private bool BeAValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}