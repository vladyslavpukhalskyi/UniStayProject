// Файл: Application/Reviews/Commands/CreateReviewCommandValidator.cs
using FluentValidation;

namespace Application.Reviews.Commands
{
    public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
    {
        public CreateReviewCommandValidator()
        {
            RuleFor(x => x.ListingId)
                .NotEmpty().WithMessage("Ідентифікатор оголошення (ListingId) є обов'язковим.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Ідентифікатор користувача (UserId) є обов'язковим (має встановлюватися сервером).");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Рейтинг має бути між 1 та 5.");

            RuleFor(x => x.Comment)
                .NotEmpty().WithMessage("Коментар є обов'язковим.")
                .MaximumLength(1000).WithMessage("Коментар не може перевищувати 1000 символів.");
        }
    }
}