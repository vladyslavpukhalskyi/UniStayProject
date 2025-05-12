// Файл: Application/Reviews/Commands/DeleteReviewCommandValidator.cs
using FluentValidation;

namespace Application.Reviews.Commands
{
    public class DeleteReviewCommandValidator : AbstractValidator<DeleteReviewCommand>
    {
        public DeleteReviewCommandValidator()
        {
            RuleFor(x => x.ReviewId)
                .NotEmpty().WithMessage("Ідентифікатор відгуку (ReviewId) є обов'язковим.");

            RuleFor(x => x.RequestingUserId)
                .NotEmpty().WithMessage("Ідентифікатор користувача, що запитує (RequestingUserId), є обов'язковим (має встановлюватися сервером).");
        }
    }
}