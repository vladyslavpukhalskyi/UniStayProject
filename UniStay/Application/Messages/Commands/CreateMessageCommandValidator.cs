using FluentValidation;

namespace Application.Messages.Commands
{
    public class CreateMessageCommandValidator : AbstractValidator<CreateMessageCommand>
    {
        public CreateMessageCommandValidator()
        {
            RuleFor(x => x.ReceiverId)
                .NotEmpty().WithMessage("Ідентифікатор отримувача (ReceiverId) є обов'язковим.");

            RuleFor(x => x.SenderId)
                .NotEmpty().WithMessage("Ідентифікатор відправника (SenderId) є обов'язковим (має встановлюватися сервером).");

            RuleFor(x => x.Text)
                .NotEmpty().WithMessage("Текст повідомлення не може бути порожнім.")
                .MaximumLength(1000).WithMessage("Текст повідомлення не може перевищувати 1000 символів."); 
        }
    }
}