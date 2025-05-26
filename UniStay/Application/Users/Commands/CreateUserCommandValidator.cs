using FluentValidation;

namespace Application.Users.Commands
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Ім'я є обов'язковим.")
                .MaximumLength(100).WithMessage("Ім'я не може перевищувати 100 символів.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Прізвище є обов'язковим.")
                .MaximumLength(100).WithMessage("Прізвище не може перевищувати 100 символів.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email є обов'язковим.")
                .MaximumLength(255).WithMessage("Email не може перевищувати 255 символів.")
                .EmailAddress().WithMessage("Неправильний формат Email.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Пароль є обов'язковим.")
                .MinimumLength(8).WithMessage("Пароль має містити щонайменше 8 символів.")
                .MaximumLength(100).WithMessage("Пароль не може перевищувати 100 символів.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Номер телефону не може перевищувати 20 символів.");

            RuleFor(x => x.ProfileImage)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.ProfileImage))
                .WithMessage("URL зображення профілю не може перевищувати 500 символів.");
        }
    }
}