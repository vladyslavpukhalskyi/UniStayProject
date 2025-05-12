// Файл: Application/Amenities/Commands/UpdateAmenityCommand.cs
using Application.Common; // Для Result
using Application.Common.Interfaces.Repositories; // Для IAmenitiesRepository
using Application.Amenities.Exceptions; // Для AmenityException та підтипів
using Domain.Amenities; // Для Amenity, AmenityId
using MediatR;
using Optional; // Для Option<> та Match()
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Amenities.Commands
{
    /// <summary>
    /// Команда для оновлення назви існуючої зручності.
    /// </summary>
    public record UpdateAmenityCommand : IRequest<Result<Amenity, AmenityException>>
    {
        /// <summary>
        /// ID зручності, яку оновлюємо.
        /// </summary>
        public required Guid AmenityId { get; init; }

        /// <summary>
        /// Нова назва зручності.
        /// </summary>
        public required string Title { get; init; }
    }

    /// <summary>
    /// Обробник команди UpdateAmenityCommand.
    /// </summary>
    public class UpdateAmenityCommandHandler(
        IAmenitiesRepository amenitiesRepository
        ) : IRequestHandler<UpdateAmenityCommand, Result<Amenity, AmenityException>>
    {
        public async Task<Result<Amenity, AmenityException>> Handle(UpdateAmenityCommand request, CancellationToken cancellationToken)
        {
            var amenityIdToUpdate = new AmenityId(request.AmenityId);

            // 1. Отримати зручність, яку потрібно оновити
            var existingAmenityOption = await amenitiesRepository.GetById(amenityIdToUpdate, cancellationToken);

            return await existingAmenityOption.Match<Task<Result<Amenity, AmenityException>>>(
                some: async amenityToUpdate => // Якщо зручність для оновлення знайдено
                {
                    // Перевіряємо, чи назва дійсно змінюється (ігноруючи регістр)
                    if (!string.Equals(amenityToUpdate.Title, request.Title, StringComparison.OrdinalIgnoreCase))
                    {
                        // Назва змінюється, тому потрібно перевірити, чи нова назва вже зайнята іншою зручністю
                        var conflictingAmenityOption = await amenitiesRepository.GetByTitleAsync(request.Title, cancellationToken);

                        // Внутрішній Match для обробки результату пошуку конфліктуючої зручності
                        return await conflictingAmenityOption.Match<Task<Result<Amenity, AmenityException>>>(
                            some: async conflictingAmenity => // Знайдено зручність з такою ж новою назвою
                            {
                                // Перевіряємо, чи це не та сама зручність, яку ми оновлюємо
                                if (conflictingAmenity.Id != amenityToUpdate.Id)
                                {
                                    // Так, це інша зручність - конфлікт
                                    AmenityException exception = new AmenityAlreadyExistsException(request.Title, conflictingAmenity.Id);
                                    return exception; // Неявне перетворення в Task<Result<...>>
                                }
                                // Якщо Id співпадають, це означає, що нова назва - це стара назва (можливо, інший регістр),
                                // або ми намагаємося "оновити" на ту саму назву, що вже є. Конфлікту немає.
                                // Продовжуємо оновлення.
                                return await UpdateAmenityEntity(amenityToUpdate, request, cancellationToken);
                            },
                            none: async () => // Нова назва унікальна
                            {
                                // Конфлікту немає, оновлюємо сутність
                                return await UpdateAmenityEntity(amenityToUpdate, request, cancellationToken);
                            }
                        );
                    }
                    else
                    {
                        // Назва не змінюється (або змінюється тільки регістр, що ми вважаємо тією ж назвою)
                        // Можна або нічого не робити, або все одно викликати Update для оновлення дати, якщо є
                        // В даному випадку, оскільки UpdateTitle змінює тільки Title,
                        // якщо назва не змінилася, UpdateAmenityEntity фактично нічого не змінить.
                        return await UpdateAmenityEntity(amenityToUpdate, request, cancellationToken);
                    }
                },
                none: () => // Якщо зручність для оновлення не знайдено
                {
                    AmenityException exception = new AmenityNotFoundException(amenityIdToUpdate);
                    return Task.FromResult<Result<Amenity, AmenityException>>(exception);
                }
            );
        }

        private async Task<Result<Amenity, AmenityException>> UpdateAmenityEntity(
            Amenity amenity,
            UpdateAmenityCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // 3. Оновити назву в доменній моделі
                amenity.UpdateTitle(request.Title);

                // 4. Зберегти зміни через репозиторій
                var updatedAmenity = await amenitiesRepository.Update(amenity, cancellationToken);
                return updatedAmenity; // Implicit conversion
            }
            catch (Exception ex)
            {
                // 5. Обробити можливі помилки збереження
                return new AmenityOperationFailedException(amenity.Id, "UpdateAmenity", ex);
            }
        }
    }
}