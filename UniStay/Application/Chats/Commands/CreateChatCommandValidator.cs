using FluentValidation;

namespace Application.Chats.Commands
{
    public class CreateChatCommandValidator : AbstractValidator<CreateChatCommand>
    {
        public CreateChatCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Chat name is required.")
                .MaximumLength(100)
                .WithMessage("Chat name must not exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Chat description must not exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid chat type.");

            RuleFor(x => x.CreatedById)
                .NotEmpty()
                .WithMessage("Creator ID is required.");
        }
    }
}
