// Файл: Application/ListingImages/Commands/CreateListingImageCommandValidator.cs
using FluentValidation;
using System; // Для Uri

namespace Application.ListingImages.Commands
{
    public class CreateListingImageCommandValidator : AbstractValidator<CreateListingImageCommand>
    {
        public CreateListingImageCommandValidator()
        {
            RuleFor(x => x.ListingId)
                .NotEmpty().WithMessage("Ідентифікатор оголошення (ListingId) є обов'язковим.");

            RuleFor(x => x.RequestingUserId)
                .NotEmpty().WithMessage("Ідентифікатор користувача (RequestingUserId) є обов'язковим (має встановлюватися сервером).");

            RuleFor(x => x.ImageUrl)
                .NotEmpty().WithMessage("URL зображення є обов'язковим.")
                .MaximumLength(2048).WithMessage("URL зображення не може перевищувати 2048 символів.")
                .Must(BeAValidUrl).WithMessage("Надано недійсний URL зображення.");
        }

        private bool BeAValidUrl(string url)
        {
            // Проста перевірка; для більш надійної можна використовувати регулярні вирази або додаткові бібліотеки
            return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}