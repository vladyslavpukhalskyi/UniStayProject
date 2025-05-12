// Файл: Application/Amenities/Commands/CreateAmenityCommand.cs
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
    /// Команда для створення нової зручності.
    /// </summary>
    public record CreateAmenityCommand : IRequest<Result<Amenity, AmenityException>>
    {
        public required string Title { get; init; }
    }

    /// <summary>
    /// Обробник команди CreateAmenityCommand.
    /// </summary>
    public class CreateAmenityCommandHandler(
        IAmenitiesRepository amenitiesRepository
        ) : IRequestHandler<CreateAmenityCommand, Result<Amenity, AmenityException>>
    {
        public async Task<Result<Amenity, AmenityException>> Handle(CreateAmenityCommand request, CancellationToken cancellationToken)
        {
            // 1. Перевірити, чи зручність з такою назвою вже існує
            // ПОТРІБЕН МЕТОД В РЕПОЗИТОРІЇ: Task<Option<Amenity>> GetByTitleAsync(...)
            var existingAmenityOption = await amenitiesRepository.GetByTitleAsync(request.Title, cancellationToken);

            return await existingAmenityOption.Match<Task<Result<Amenity, AmenityException>>>(
                some: amenity => // Зручність з такою назвою вже існує
                {
                    AmenityException exception = new AmenityAlreadyExistsException(request.Title, amenity.Id);
                    return Task.FromResult<Result<Amenity, AmenityException>>(exception);
                },
                none: async () => // Зручності з такою назвою ще немає
                {
                    var amenityId = AmenityId.New();
                    try
                    {
                        // 2. Створити сутність Amenity
                        var amenity = Amenity.New(
                            id: amenityId,
                            title: request.Title
                        );

                        // 3. Додати зручність в репозиторій
                        var addedAmenity = await amenitiesRepository.Add(amenity, cancellationToken);
                        return addedAmenity; // Implicit conversion
                    }
                    catch (Exception ex)
                    {
                        // 4. Обробити можливі помилки збереження
                        return new AmenityOperationFailedException(amenityId, "CreateAmenity", ex);
                    }
                }
            );
        }
    }
}