using FluentValidation;

namespace Application.Chats.Commands
{
    public class JoinChatByInviteCommandValidator : AbstractValidator<JoinChatByInviteCommand>
    {
        public JoinChatByInviteCommandValidator()
        {
            RuleFor(x => x.InviteCode)
                .NotEmpty()
                .WithMessage("Invite code is required.")
                .Length(8)
                .WithMessage("Invite code must be 8 characters long.");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.");
        }
    }
}
