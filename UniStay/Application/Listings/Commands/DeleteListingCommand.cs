// Файл: Application/Listings/Commands/DeleteListingCommand.cs
using Application.Common; // Для Result
using Application.Common.Interfaces.Repositories; // Для IListingsRepository
using Application.Listings.Exceptions; // Для ListingException та підтипів
using Domain.Listings; // Для Listing, ListingId
using Domain.Users;   // Для UserId
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Listings.Commands
{
    /// <summary>
    /// Команда для видалення оголошення.
    /// </summary>
    public record DeleteListingCommand : IRequest<Result<Listing, ListingException>>
    {
        /// <summary>
        /// ID оголошення, яке потрібно видалити.
        /// </summary>
        public required Guid ListingId { get; init; }

        /// <summary>
        /// ID користувача, який запитує видалення (для перевірки авторизації).
        /// Встановлюється з контексту аутентифікації.
        /// </summary>
        public required Guid RequestingUserId { get; init; }
    }

    /// <summary>
    /// Обробник команди DeleteListingCommand.
    /// </summary>
    public class DeleteListingCommandHandler(
        IListingsRepository listingsRepository)
        : IRequestHandler<DeleteListingCommand, Result<Listing, ListingException>>
    {
        public async Task<Result<Listing, ListingException>> Handle(DeleteListingCommand request, CancellationToken cancellationToken)
        {
            var listingIdToDelete = new ListingId(request.ListingId);
            var requestingUserId = new UserId(request.RequestingUserId);

            // 1. Отримати оголошення за ID
            var existingListingOption = await listingsRepository.GetById(listingIdToDelete, cancellationToken);

            return await existingListingOption.Match<Task<Result<Listing, ListingException>>>(
                some: async listing => // Якщо оголошення знайдено
                {
                    // 2. Перевірка авторизації: чи є поточний користувач власником оголошення
                    // (Для адміністраторських прав потрібна була б додаткова логіка/перевірка ролі)
                    if (listing.UserId != requestingUserId)
                    {
                        return new UserNotAuthorizedForListingOperationException(requestingUserId, listingIdToDelete, "DeleteListing");
                    }

                    // 3. Видалити сутність оголошення
                    return await DeleteListingEntity(listing, cancellationToken);
                },
                none: () => // Якщо оголошення не знайдено
                {
                    ListingException exception = new ListingNotFoundException(listingIdToDelete);
                    return Task.FromResult<Result<Listing, ListingException>>(exception);
                }
            );
        }

        private async Task<Result<Listing, ListingException>> DeleteListingEntity(Listing listing, CancellationToken cancellationToken)
        {
            try
            {
                // 4. Видалити оголошення через репозиторій
                var deletedListing = await listingsRepository.Delete(listing, cancellationToken);
                return deletedListing; // Implicit conversion
            }
            catch (Exception exception)
            {
                // 5. Обробити можливі помилки під час видалення
                return new ListingOperationFailedException(listing.Id, "DeleteListing", exception);
            }
        }
    }
}