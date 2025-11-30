using FluentValidation;

namespace Application.Chats.Commands
{
    public class DeleteChatMessageCommandValidator : AbstractValidator<DeleteChatMessageCommand>
    {
        public DeleteChatMessageCommandValidator()
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
        }
    }
}
