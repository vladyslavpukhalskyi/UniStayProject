using FluentValidation;

namespace Application.Amenities.Commands
{
    public class UpdateAmenityCommandValidator : AbstractValidator<UpdateAmenityCommand>
    {
        public UpdateAmenityCommandValidator()
        {
            RuleFor(x => x.AmenityId)
                .NotEmpty().WithMessage("Ідентифікатор зручності (AmenityId) є обов'язковим.");

            When(x => x.Title != null, () =>
            {
                RuleFor(x => x.Title!)
                    .NotEmpty().WithMessage("Нова назва зручності не може бути порожньою.")
                    .MaximumLength(100).WithMessage("Назва зручності не може перевищувати 100 символів.");
            });

            RuleFor(x => new object?[] { x.Title }
                .Any(v => v != null))
                .Equal(true)
                .WithMessage("Потрібно вказати принаймні одне поле для оновлення.");
        }
    }
}