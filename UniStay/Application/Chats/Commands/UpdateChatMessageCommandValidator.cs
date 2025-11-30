using FluentValidation;

namespace Application.Chats.Commands
{
    public class UpdateChatMessageCommandValidator : AbstractValidator<UpdateChatMessageCommand>
    {
        public UpdateChatMessageCommandValidator()
        {
            RuleFor(x => x.ChatId)
                .NotEmpty()
                .WithMessage("Chat ID is required.");

            RuleFor(x => x.MessageId)
                .NotEmpty()
                .WithMessage("Message ID is required.");

            RuleFor(x => x.RequestingUserId)
                .NotEmpty()
                .WithMessage("Requesting user ID is required.");

            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Message content is required.")
                .MaximumLength(2000)
                .WithMessage("Message content must not exceed 2000 characters.");
        }
    }
}
