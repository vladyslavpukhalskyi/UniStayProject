// Файл: Application/Listings/Commands/DeleteListingCommandValidator.cs
using FluentValidation;

namespace Application.Listings.Commands
{
    public class DeleteListingCommandValidator : AbstractValidator<DeleteListingCommand>
    {
        public DeleteListingCommandValidator()
        {
            RuleFor(x => x.ListingId)
                .NotEmpty().WithMessage("Ідентифікатор оголошення (ListingId) є обов'язковим.");

            RuleFor(x => x.RequestingUserId)
                .NotEmpty().WithMessage("Ідентифікатор користувача, що запитує (RequestingUserId), є обов'язковим (має встановлюватися сервером).");
        }
    }
}