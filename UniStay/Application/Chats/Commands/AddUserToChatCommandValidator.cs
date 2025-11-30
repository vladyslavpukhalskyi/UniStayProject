using FluentValidation;

namespace Application.Chats.Commands
{
    public class AddUserToChatCommandValidator : AbstractValidator<AddUserToChatCommand>
    {
        public AddUserToChatCommandValidator()
        {
            RuleFor(x => x.ChatId)
                .NotEmpty()
                .WithMessage("Chat ID is required.");

            RuleFor(x => x.RequestorUserId)
                .NotEmpty()
                .WithMessage("Requestor user ID is required.");

            RuleFor(x => x.TargetUserId)
                .NotEmpty()
                .WithMessage("Target user ID is required.");
        }
    }
}
