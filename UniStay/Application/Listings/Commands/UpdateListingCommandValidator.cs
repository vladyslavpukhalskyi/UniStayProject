using Domain.Listings; // Для ListingEnums
using FluentValidation;
using System.Linq;

namespace Application.Listings.Commands
{
    public class UpdateListingCommandValidator : AbstractValidator<UpdateListingCommand>
    {
        public UpdateListingCommandValidator()
        {
            RuleFor(x => x.ListingId)
               .NotEmpty().WithMessage("Ідентифікатор оголошення (ListingId) є обов'язковим.");

            RuleFor(x => x.RequestingUserId)
                .NotEmpty().WithMessage("Ідентифікатор користувача, що запитує (RequestingUserId), є обов'язковим (має встановлюватися сервером).");

            When(x => x.Title != null, () =>
            {
                RuleFor(x => x.Title!)
                    .NotEmpty().WithMessage("Назва є обов'язковою.")
                    .MaximumLength(255).WithMessage("Назва не може перевищувати 255 символів.");
            });

            When(x => x.Description != null, () =>
            {
                RuleFor(x => x.Description!)
                    .NotEmpty().WithMessage("Опис є обов'язковим.")
                    .MaximumLength(1000).WithMessage("Опис не може перевищувати 1000 символів.");
            });

            When(x => x.Address != null, () =>
            {
                RuleFor(x => x.Address!)
                    .NotEmpty().WithMessage("Адреса є обов'язковою.")
                    .MaximumLength(500).WithMessage("Адреса не може перевищувати 500 символів.");
            });

            When(x => x.Latitude.HasValue, () =>
            {
                RuleFor(x => x.Latitude!.Value)
                    .InclusiveBetween(-90, 90).WithMessage("Широта має бути між -90 та 90 градусами.");
            });

            When(x => x.Longitude.HasValue, () =>
            {
                RuleFor(x => x.Longitude!.Value)
                    .InclusiveBetween(-180, 180).WithMessage("Довгота має бути між -180 та 180 градусами.");
            });

            When(x => x.Price.HasValue, () =>
            {
                RuleFor(x => x.Price!.Value)
                    .GreaterThan(0).WithMessage("Ціна має бути більшою за 0.");
            });

            When(x => x.Type.HasValue, () =>
            {
                RuleFor(x => x.Type!.Value)
                    .IsInEnum().WithMessage("Неправильний тип оголошення.");
            });

            When(x => x.CommunalServices != null, () =>
            {
                RuleFor(x => x.CommunalServices!)
                    .NotNull().WithMessage("Список комунальних послуг не може бути null.")
                    .Must(list => list.All(item => Enum.IsDefined(typeof(ListingEnums.CommunalService), item)))
                        .WithMessage("Список комунальних послуг містить неправильні значення.")
                    .Must(list => list.Count > 0).WithMessage("Має бути вказаний принаймні один статус комунальних послуг.");
            });

            When(x => x.Owners.HasValue, () =>
            {
                RuleFor(x => x.Owners!.Value)
                    .IsInEnum().WithMessage("Неправильний тип власності.");
            });

            When(x => x.Neighbours.HasValue, () =>
            {
                RuleFor(x => x.Neighbours!.Value)
                    .IsInEnum().WithMessage("Неправильний тип сусідів.");
            });

            When(x => x.AmenityIds != null, () =>
            {
                RuleFor(x => x.AmenityIds!)
                    .NotNull().WithMessage("Список ID зручностей не може бути null (може бути порожнім).");
            });

            RuleFor(x => new object?[] { x.Title, x.Description, x.Address, x.Latitude, x.Longitude, x.Price, x.Type, x.CommunalServices, x.Owners, x.Neighbours, x.AmenityIds }
                .Any(v => v != null))
                .Equal(true)
                .WithMessage("Потрібно вказати принаймні одне поле для оновлення.");
        }
    }
}