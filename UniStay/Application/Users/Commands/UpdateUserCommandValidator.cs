using FluentValidation;

namespace Application.Users.Commands
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Ідентифікатор користувача (UserId) є обов'язковим.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Ім'я є обов'язковим.")
                .MaximumLength(100).WithMessage("Ім'я не може перевищувати 100 символів.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Прізвище є обов'язковим.")
                .MaximumLength(100).WithMessage("Прізвище не може перевищувати 100 символів.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Номер телефону не може перевищувати 20 символів.");

            RuleFor(x => x.ProfileImage)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.ProfileImage))
                .WithMessage("URL зображення профілю не може перевищувати 500 символів.");
        }
    }
}