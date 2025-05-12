// Файл: Application/Listings/Commands/CreateListingCommand.cs
using Application.Common; // Для Result
using Application.Common.Interfaces.Repositories; // Для IListingsRepository, IAmenitiesRepository
using Application.Listings.Exceptions; // Для ListingException та підтипів
using Domain.Amenities; // Для Amenity, AmenityId
using Domain.Listings; // Для Listing, ListingId, ListingEnums
using Domain.Users;   // Для UserId
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Listings.Commands
{
    /// <summary>
    /// Команда для створення нового оголошення.
    /// </summary>
    public record CreateListingCommand : IRequest<Result<Listing, ListingException>>
    {
        // Поля з CreateListingDto
        public required string Title { get; init; }
        public required string Description { get; init; }
        public required string Address { get; init; }
        public required float Price { get; init; }
        public required ListingEnums.ListingType Type { get; init; }
        public required List<ListingEnums.CommunalService> CommunalServices { get; init; }
        public required ListingEnums.OwnershipType Owners { get; init; }
        public required ListingEnums.NeighbourType Neighbours { get; init; }
        public required List<Guid> AmenityIds { get; init; } // ID зручностей для прив'язки

        // ID користувача, який створює оголошення (з контексту аутентифікації)
        public required Guid UserId { get; init; }
    }

    /// <summary>
    /// Обробник команди CreateListingCommand.
    /// </summary>
    public class CreateListingCommandHandler(
        IListingsRepository listingsRepository,
        IAmenitiesRepository amenitiesRepository // Потрібен для валідації та отримання зручностей
        )
        : IRequestHandler<CreateListingCommand, Result<Listing, ListingException>>
    {
        public async Task<Result<Listing, ListingException>> Handle(CreateListingCommand request, CancellationToken cancellationToken)
        {
            var listingId = ListingId.New();
            var userId = new UserId(request.UserId);
            List<Amenity> amenitiesToAssociate = new List<Amenity>();

            // 1. Перевірити та отримати зручності (Amenities) за наданими ID
            if (request.AmenityIds != null && request.AmenityIds.Any())
            {
                var uniqueAmenityIds = request.AmenityIds.Distinct().Select(id => new AmenityId(id)).ToList();
                var foundAmenities = new List<Amenity>();

                // Використовуємо Match для безпечного додавання знайдених зручностей
                // В ідеалі тут має бути один виклик до репозиторію для отримання списку ID,
                // але для виправлення поточної помилки використовуємо цикл з Match.
                foreach(var amenityId in uniqueAmenityIds)
                {
                    var amenityOpt = await amenitiesRepository.GetById(amenityId, cancellationToken);
                    amenityOpt.Match(
                        some: amenity => foundAmenities.Add(amenity), // Додаємо розпакований amenity
                        none: () => { } // Якщо не знайдено, нічого не робимо в цій ітерації
                    );
                }

                // Перевіряємо, чи всі запитані ID були знайдені
                if (foundAmenities.Count != uniqueAmenityIds.Count)
                {
                    var foundIds = foundAmenities.Select(a => a.Id.Value); // Доступ до Id.Value тут безпечний, бо foundAmenities містить реальні об'єкти Amenity
                    var invalidIds = request.AmenityIds.Distinct().Where(id => !foundIds.Contains(id));
                    return new InvalidAmenitiesProvidedException(invalidIds);
                }
                amenitiesToAssociate = foundAmenities;
            }

            try
            {
                // 2. Створити сутність Listing
                var listing = Listing.New(
                    id: listingId,
                    title: request.Title,
                    description: request.Description,
                    address: request.Address,
                    price: request.Price,
                    type: request.Type,
                    userId: userId,
                    communalServices: request.CommunalServices ?? new List<ListingEnums.CommunalService>(),
                    owners: request.Owners,
                    neighbours: request.Neighbours,
                    publicationDate: DateTime.UtcNow // Встановлюємо дату публікації
                );

                // 3. Додати перевірені зручності до оголошення
                foreach (var amenity in amenitiesToAssociate)
                {
                    listing.AddAmenity(amenity);
                }

                // 4. Додати оголошення в репозиторій
                // Репозиторій має зберегти і саме оголошення, і зв'язки з Amenities
                var addedListing = await listingsRepository.Add(listing, cancellationToken);
                return addedListing; // Implicit conversion
            }
            catch (Exception exception)
            {
                // 5. Обробити можливі помилки під час збереження
                return new ListingOperationFailedException(listingId, "CreateListing", exception);
            }
        }
    }
}