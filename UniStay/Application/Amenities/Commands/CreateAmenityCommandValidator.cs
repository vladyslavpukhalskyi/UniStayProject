using FluentValidation;

namespace Application.Amenities.Commands
{
    public class CreateAmenityCommandValidator : AbstractValidator<CreateAmenityCommand>
    {
        public CreateAmenityCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Назва зручності є обов'язковою.")
                .MaximumLength(100).WithMessage("Назва зручності не може перевищувати 100 символів.");
        }
    }
}