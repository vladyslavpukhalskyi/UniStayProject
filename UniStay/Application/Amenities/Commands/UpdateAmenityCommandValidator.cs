// Файл: Application/Amenities/Commands/UpdateAmenityCommandValidator.cs
using FluentValidation;

namespace Application.Amenities.Commands
{
    public class UpdateAmenityCommandValidator : AbstractValidator<UpdateAmenityCommand>
    {
        public UpdateAmenityCommandValidator()
        {
            RuleFor(x => x.AmenityId)
                .NotEmpty().WithMessage("Ідентифікатор зручності (AmenityId) є обов'язковим.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Нова назва зручності є обов'язковою.")
                .MaximumLength(100).WithMessage("Назва зручності не може перевищувати 100 символів.");
        }
    }
}