using FluentValidation;

namespace Application.Users.Commands
{
    public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
    {
        public DeleteUserCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Ідентифікатор користувача (UserId) є обов'язковим.");
        }
    }
}