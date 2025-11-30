using FluentValidation;

namespace Application.Messages.Commands
{
    public class UpdateMessageCommandValidator : AbstractValidator<UpdateMessageCommand>
    {
        public UpdateMessageCommandValidator()
        {
            RuleFor(x => x.MessageId)
                .NotEmpty().WithMessage("Ідентифікатор повідомлення (MessageId) є обов'язковим.");

            RuleFor(x => x.RequestingUserId)
                .NotEmpty().WithMessage("Ідентифікатор користувача, що запитує (RequestingUserId), є обов'язковим (має встановлюватися сервером).");

            When(x => x.Text != null, () =>
            {
                RuleFor(x => x.Text!)
                    .NotEmpty().WithMessage("Текст повідомлення не може бути порожнім.")
                    .MaximumLength(1000).WithMessage("Текст повідомлення не може перевищувати 1000 символів.");
            });

            RuleFor(x => new object?[] { x.Text }
                .Any(v => v != null))
                .Equal(true)
                .WithMessage("Потрібно вказати принаймні одне поле для оновлення.");
        }
    }
}