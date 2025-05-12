// Файл: Application/Listings/Commands/CreateListingCommandValidator.cs
using Domain.Listings; // Для ListingEnums
using FluentValidation;
using System.Linq;

namespace Application.Listings.Commands
{
    public class CreateListingCommandValidator : AbstractValidator<CreateListingCommand>
    {
        public CreateListingCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Ідентифікатор користувача (UserId) є обов'язковим (має встановлюватися сервером).");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Назва є обов'язковою.")
                .MaximumLength(255).WithMessage("Назва не може перевищувати 255 символів.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Опис є обов'язковим.")
                .MaximumLength(1000).WithMessage("Опис не може перевищувати 1000 символів.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Адреса є обов'язковою.")
                .MaximumLength(500).WithMessage("Адреса не може перевищувати 500 символів.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Ціна має бути більшою за 0.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Неправильний тип оголошення.");

            RuleFor(x => x.CommunalServices)
                .NotNull().WithMessage("Список комунальних послуг не може бути null.")
                .Must(list => list.All(item => Enum.IsDefined(typeof(ListingEnums.CommunalService), item)))
                    .WithMessage("Список комунальних послуг містить неправильні значення.")
                .Must(list => list.Count > 0).WithMessage("Має бути вказаний принаймні один статус комунальних послуг."); // Або інше правило

            RuleFor(x => x.Owners)
                .IsInEnum().WithMessage("Неправильний тип власності.");

            RuleFor(x => x.Neighbours)
                .IsInEnum().WithMessage("Неправильний тип сусідів.");

            RuleFor(x => x.AmenityIds)
                .NotNull().WithMessage("Список ID зручностей не може бути null (може бути порожнім).");
                // Перевірка існування ID зручностей відбувається в обробнику команди
        }
    }
}