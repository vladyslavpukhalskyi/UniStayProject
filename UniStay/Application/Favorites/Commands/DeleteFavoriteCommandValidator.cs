// Файл: Application/Favorites/Commands/DeleteFavoriteCommandValidator.cs
using FluentValidation;

namespace Application.Favorites.Commands
{
    public class DeleteFavoriteCommandValidator : AbstractValidator<DeleteFavoriteCommand>
    {
        public DeleteFavoriteCommandValidator()
        {
            RuleFor(x => x.ListingId)
                .NotEmpty().WithMessage("Ідентифікатор оголошення (ListingId) є обов'язковим.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Ідентифікатор користувача (UserId) є обов'язковим (має встановлюватися сервером).");
        }
    }
}