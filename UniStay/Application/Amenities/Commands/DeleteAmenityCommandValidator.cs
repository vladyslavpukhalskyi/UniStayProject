using FluentValidation;

namespace Application.Amenities.Commands
{
    public class DeleteAmenityCommandValidator : AbstractValidator<DeleteAmenityCommand>
    {
        public DeleteAmenityCommandValidator()
        {
            RuleFor(x => x.AmenityId)
                .NotEmpty().WithMessage("Ідентифікатор зручності (AmenityId) є обов'язковим.");
        }
    }
}