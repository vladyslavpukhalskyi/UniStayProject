using FluentValidation;

namespace Application.Reviews.Commands
{
    public class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
    {
        public UpdateReviewCommandValidator()
        {
            RuleFor(x => x.ReviewId)
                .NotEmpty().WithMessage("Ідентифікатор відгуку (ReviewId) є обов'язковим.");

            RuleFor(x => x.RequestingUserId)
                .NotEmpty().WithMessage("Ідентифікатор користувача, що запитує (RequestingUserId), є обов'язковим (має встановлюватися сервером).");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Рейтинг має бути між 1 та 5.");

            RuleFor(x => x.Comment)
                .NotEmpty().WithMessage("Коментар є обов'язковим.")
                .MaximumLength(1000).WithMessage("Коментар не може перевищувати 1000 символів.");
        }
    }
}