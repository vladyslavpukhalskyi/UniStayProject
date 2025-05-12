// Файл: Application/ListingImages/Commands/CreateListingImageCommand.cs
using Application.Common; // Для Result
using Application.Common.Interfaces.Repositories; // Для IListingImagesRepository, IListingsRepository
using Application.ListingImages.Exceptions; // Для ListingImageException та підтипів
using Domain.ListingImages; // Для ListingImage, ListingImageId
using Domain.Listings;    // Для ListingId
using Domain.Users;       // Для UserId
using MediatR;
using Optional; // Для Option<> та Match()
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.ListingImages.Commands
{
    /// <summary>
    /// Команда для додавання нового зображення до оголошення.
    /// </summary>
    public record CreateListingImageCommand : IRequest<Result<ListingImage, ListingImageException>>
    {
        /// <summary>
        /// ID оголошення, до якого додається зображення.
        /// </summary>
        public required Guid ListingId { get; init; }

        /// <summary>
        /// URL зображення.
        /// </summary>
        public required string ImageUrl { get; init; }

        /// <summary>
        /// ID користувача, який додає зображення (для перевірки власності оголошення).
        /// Встановлюється з контексту аутентифікації.
        /// </summary>
        public required Guid RequestingUserId { get; init; }
    }

    /// <summary>
    /// Обробник команди CreateListingImageCommand.
    /// </summary>
    public class CreateListingImageCommandHandler(
        IListingImagesRepository listingImagesRepository,
        IListingsRepository listingsRepository // Потрібен для перевірки власника оголошення
        )
        : IRequestHandler<CreateListingImageCommand, Result<ListingImage, ListingImageException>>
    {
        public async Task<Result<ListingImage, ListingImageException>> Handle(CreateListingImageCommand request, CancellationToken cancellationToken)
        {
            var listingIdObj = new ListingId(request.ListingId);
            var requestingUserIdObj = new UserId(request.RequestingUserId);

            // 1. Перевірити, чи існує оголошення і чи користувач є його власником
            var listingOption = await listingsRepository.GetById(listingIdObj, cancellationToken);

            return await listingOption.Match<Task<Result<ListingImage, ListingImageException>>>(
                some: async listing => // Оголошення знайдено
                {
                    if (listing.UserId != requestingUserIdObj)
                    {
                        return new UserNotAuthorizedToManageListingImagesException(requestingUserIdObj, listingIdObj);
                    }

                    // Оголошення існує і користувач авторизований, продовжуємо створення зображення
                    var listingImageId = ListingImageId.New();
                    try
                    {
                        var listingImage = ListingImage.New(
                            id: listingImageId,
                            listingId: listingIdObj,
                            imageUrl: request.ImageUrl
                        );

                        var addedImage = await listingImagesRepository.Add(listingImage, cancellationToken);
                        return addedImage; // Implicit conversion
                    }
                    catch (Exception ex)
                    {
                        return new ListingImageOperationFailedException(listingImageId, "CreateListingImage", ex);
                    }
                },
                none: () => // Оголошення не знайдено
                {
                    ListingImageException exception = new ListingNotFoundForImageException(listingIdObj);
                    return Task.FromResult<Result<ListingImage, ListingImageException>>(exception);
                }
            );
        }
    }
}