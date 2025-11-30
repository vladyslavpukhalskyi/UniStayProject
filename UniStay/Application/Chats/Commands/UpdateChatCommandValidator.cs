using FluentValidation;

namespace Application.Chats.Commands
{
    public class UpdateChatCommandValidator : AbstractValidator<UpdateChatCommand>
    {
        public UpdateChatCommandValidator()
        {
            RuleFor(x => x.ChatId)
                .NotEmpty()
                .WithMessage("Chat ID is required.");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.");

            When(x => x.Name != null, () =>
            {
                RuleFor(x => x.Name!)
                    .NotEmpty()
                    .WithMessage("Chat name must not be empty when provided.")
                    .MaximumLength(100)
                    .WithMessage("Chat name must not exceed 100 characters.");
            });

            When(x => x.Description != null, () =>
            {
                RuleFor(x => x.Description!)
                    .MaximumLength(500)
                    .WithMessage("Chat description must not exceed 500 characters.");
            });

            RuleFor(x => new object?[] { x.Name, x.Description }
                .Any(v => v != null))
                .Equal(true)
                .WithMessage("At least one field must be provided for update.");
        }
    }
}
