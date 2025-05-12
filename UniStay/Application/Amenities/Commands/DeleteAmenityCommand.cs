// Файл: Application/Amenities/Commands/DeleteAmenityCommand.cs
using Application.Common; // Для Result
using Application.Common.Interfaces.Repositories; // Для IAmenitiesRepository
using Application.Amenities.Exceptions; // Для AmenityException та підтипів
using Domain.Amenities; // Для Amenity, AmenityId
using MediatR;
using Microsoft.EntityFrameworkCore; // Для DbUpdateException
using Optional; // Для Option<> та Match()
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Amenities.Commands
{
    /// <summary>
    /// Команда для видалення зручності.
    /// </summary>
    public record DeleteAmenityCommand : IRequest<Result<Amenity, AmenityException>>
    {
        /// <summary>
        /// ID зручності, яку потрібно видалити.
        /// </summary>
        public required Guid AmenityId { get; init; }
    }

    /// <summary>
    /// Обробник команди DeleteAmenityCommand.
    /// </summary>
    public class DeleteAmenityCommandHandler(
        IAmenitiesRepository amenitiesRepository)
        : IRequestHandler<DeleteAmenityCommand, Result<Amenity, AmenityException>>
    {
        public async Task<Result<Amenity, AmenityException>> Handle(DeleteAmenityCommand request, CancellationToken cancellationToken)
        {
            var amenityIdToDelete = new AmenityId(request.AmenityId);

            // 1. Отримати зручність за ID
            var existingAmenityOption = await amenitiesRepository.GetById(amenityIdToDelete, cancellationToken);

            return await existingAmenityOption.Match<Task<Result<Amenity, AmenityException>>>(
                some: async amenity => // Якщо зручність знайдено
                {
                    // 2. Видалити сутність
                    return await DeleteAmenityEntity(amenity, cancellationToken);
                },
                none: () => // Якщо зручність не знайдено
                {
                    AmenityException exception = new AmenityNotFoundException(amenityIdToDelete);
                    return Task.FromResult<Result<Amenity, AmenityException>>(exception);
                }
            );
        }

        private async Task<Result<Amenity, AmenityException>> DeleteAmenityEntity(Amenity amenity, CancellationToken cancellationToken)
        {
            try
            {
                // 3. Видалити зручність через репозиторій
                var deletedAmenity = await amenitiesRepository.Delete(amenity, cancellationToken);
                return deletedAmenity; // Implicit conversion
            }
            catch (DbUpdateException dbEx) // Спробуємо перехопити помилку FK constraint
            {
                // Тут можна додати логіку для перевірки inner exception або коду помилки БД,
                // щоб точно визначити, що це помилка видалення через використання в Listings.
                // Поки що повертаємо загальну помилку, але з DbUpdateException.
                // Можна створити спеціальний AmenityInUseException.
                 return new AmenityOperationFailedException(amenity.Id, "DeleteAmenity (likely due to FK constraint)", dbEx);
            }
            catch (Exception exception)
            {
                // 4. Обробити інші можливі помилки під час видалення
                return new AmenityOperationFailedException(amenity.Id, "DeleteAmenity", exception);
            }
        }
    }
}