using FluentValidation;

namespace Application.Chats.Commands
{
    public class SendChatMessageCommandValidator : AbstractValidator<SendChatMessageCommand>
    {
        public SendChatMessageCommandValidator()
        {
            RuleFor(x => x.ChatId)
                .NotEmpty()
                .WithMessage("Chat ID is required.");

            RuleFor(x => x.SenderId)
                .NotEmpty()
                .WithMessage("Sender ID is required.");

            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Message content is required.")
                .MaximumLength(2000)
                .WithMessage("Message content must not exceed 2000 characters.");
        }
    }
}
