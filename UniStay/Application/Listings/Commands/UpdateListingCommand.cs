// Файл: Application/Listings/Commands/UpdateListingCommand.cs
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
    /// Команда для оновлення існуючого оголошення.
    /// </summary>
    public record UpdateListingCommand : IRequest<Result<Listing, ListingException>>
    {
        /// <summary>
        /// ID оголошення, яке оновлюється.
        /// </summary>
        public required Guid ListingId { get; init; }

        // Поля для оновлення (з UpdateListingDto)
        public required string Title { get; init; }
        public required string Description { get; init; }
        public required string Address { get; init; }
        public required float Price { get; init; }
        public required ListingEnums.ListingType Type { get; init; }
        public required List<ListingEnums.CommunalService> CommunalServices { get; init; }
        public required ListingEnums.OwnershipType Owners { get; init; }
        public required ListingEnums.NeighbourType Neighbours { get; init; }

        /// <summary>
        /// Повний бажаний список ID зручностей для цього оголошення.
        /// Старі зв'язки буде видалено, якщо їх ID немає в цьому списку.
        /// Нові зв'язки буде додано.
        /// </summary>
        public required List<Guid> AmenityIds { get; init; }

        /// <summary>
        /// ID користувача, який запитує оновлення (для авторизації).
        /// </summary>
        public required Guid RequestingUserId { get; init; }
    }

    /// <summary>
    /// Обробник команди UpdateListingCommand.
    /// </summary>
    public class UpdateListingCommandHandler(
        IListingsRepository listingsRepository,
        IAmenitiesRepository amenitiesRepository // Потрібен для валідації та отримання зручностей
        )
        : IRequestHandler<UpdateListingCommand, Result<Listing, ListingException>>
    {
        public async Task<Result<Listing, ListingException>> Handle(UpdateListingCommand request, CancellationToken cancellationToken)
        {
            var listingIdToUpdate = new ListingId(request.ListingId);
            var requestingUserId = new UserId(request.RequestingUserId);

            // 1. Отримати оголошення за ID, включаючи поточні зручності (Amenities)
            // Важливо: Реалізація GetById в репозиторії МАЄ включати Amenities (.Include(l => l.Amenities))
            var existingListingOption = await listingsRepository.GetById(listingIdToUpdate, cancellationToken);

            return await existingListingOption.Match<Task<Result<Listing, ListingException>>>(
                some: async listing => // Якщо оголошення знайдено
                {
                    // 2. Перевірка авторизації: чи є поточний користувач власником оголошення
                    if (listing.UserId != requestingUserId)
                    {
                        return new UserNotAuthorizedForListingOperationException(requestingUserId, listingIdToUpdate, "UpdateListing");
                    }

                    // 3. Перевірити та отримати нові зручності (Amenities) за наданими ID
                    List<Amenity> validAmenities = new List<Amenity>();
                    if (request.AmenityIds != null && request.AmenityIds.Any())
                    {
                        var uniqueAmenityIds = request.AmenityIds.Distinct().Select(id => new AmenityId(id)).ToList();
                        // Тут має бути ефективний метод отримання списку зручностей за списком ID
                        // Симуляція, як у CreateListingCommandHandler:
                         var foundAmenities = new List<Amenity>();
                        foreach(var amenityId in uniqueAmenityIds) {
                            var amenityOpt = await amenitiesRepository.GetById(amenityId, cancellationToken);
                            amenityOpt.Match(
                                some: amenity => foundAmenities.Add(amenity),
                                none: () => { } // Просто ігноруємо неіснуючі ID на цьому етапі перевірки
                            );
                        }
                        // Кінець симуляції

                        if (foundAmenities.Count != uniqueAmenityIds.Count)
                        {
                            var foundIds = foundAmenities.Select(a => a.Id.Value);
                            var invalidIds = request.AmenityIds.Distinct().Where(id => !foundIds.Contains(id));
                            return new InvalidAmenitiesProvidedException(invalidIds);
                        }
                        validAmenities = foundAmenities;
                    }

                    try
                    {
                        // 4. Оновити основні деталі оголошення
                        listing.UpdateDetails(
                            request.Title,
                            request.Description,
                            request.Address,
                            request.Price,
                            request.Type,
                            request.CommunalServices ?? new List<ListingEnums.CommunalService>(),
                            request.Owners,
                            request.Neighbours
                        );

                        // 5. Синхронізувати список зручностей
                        // Визначаємо, які зручності видалити
                        var amenitiesToRemove = listing.Amenities
                            .Where(currentAmenity => !validAmenities.Any(newAmenity => newAmenity.Id == currentAmenity.Id))
                            .ToList(); // ToList() щоб уникнути зміни колекції під час ітерації

                        // Визначаємо, які зручності додати
                        var amenitiesToAdd = validAmenities
                            .Where(newAmenity => !listing.Amenities.Any(currentAmenity => currentAmenity.Id == newAmenity.Id))
                            .ToList();

                        // Видаляємо старі
                        foreach (var amenityToRemove in amenitiesToRemove)
                        {
                            listing.RemoveAmenity(amenityToRemove);
                        }

                        // Додаємо нові
                        foreach (var amenityToAdd in amenitiesToAdd)
                        {
                            listing.AddAmenity(amenityToAdd);
                        }

                        // 6. Зберегти оновлене оголошення в репозиторії
                        // Репозиторій має оновити і саме оголошення, і зв'язки з Amenities
                        var updatedListing = await listingsRepository.Update(listing, cancellationToken);
                        return updatedListing; // Implicit conversion
                    }
                    catch (Exception exception)
                    {
                        // 7. Обробити можливі помилки під час збереження
                        return new ListingOperationFailedException(listing.Id, "UpdateListing", exception);
                    }
                },
                none: () => // Якщо оголошення не знайдено
                {
                    ListingException exception = new ListingNotFoundException(listingIdToUpdate);
                    return Task.FromResult<Result<Listing, ListingException>>(exception);
                }
            );
        }
    }
}