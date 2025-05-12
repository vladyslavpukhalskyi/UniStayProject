// Файл: Application/Favorites/Commands/CreateFavoriteCommandValidator.cs
using FluentValidation;

namespace Application.Favorites.Commands
{
    public class CreateFavoriteCommandValidator : AbstractValidator<CreateFavoriteCommand>
    {
        public CreateFavoriteCommandValidator()
        {
            RuleFor(x => x.ListingId)
                .NotEmpty().WithMessage("Ідентифікатор оголошення (ListingId) є обов'язковим.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Ідентифікатор користувача (UserId) є обов'язковим (має встановлюватися сервером).");
        }
    }
}