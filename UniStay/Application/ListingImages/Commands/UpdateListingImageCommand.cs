// Файл: Application/ListingImages/Commands/UpdateListingImageCommand.cs
using Application.Common; // Для Result
using Application.Common.Interfaces.Repositories; // Для IListingImagesRepository
using Application.ListingImages.Exceptions; // Для ListingImageException та підтипів
using Domain.ListingImages; // Для ListingImage, ListingImageId
using Domain.Users;       // Для UserId
using MediatR;
using Optional; // Для Option<> та Match()
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.ListingImages.Commands
{
    /// <summary>
    /// Команда для оновлення URL існуючого зображення оголошення.
    /// </summary>
    public record UpdateListingImageCommand : IRequest<Result<ListingImage, ListingImageException>>
    {
        /// <summary>
        /// ID зображення, яке оновлюється.
        /// </summary>
        public required Guid ListingImageId { get; init; }

        /// <summary>
        /// Новий URL зображення.
        /// </summary>
        public required string NewImageUrl { get; init; }

        /// <summary>
        /// ID користувача, який запитує оновлення (для перевірки власності оголошення).
        /// Встановлюється з контексту аутентифікації.
        /// </summary>
        public required Guid RequestingUserId { get; init; }
    }

    /// <summary>
    /// Обробник команди UpdateListingImageCommand.
    /// </summary>
    public class UpdateListingImageCommandHandler(
        IListingImagesRepository listingImagesRepository
        )
        : IRequestHandler<UpdateListingImageCommand, Result<ListingImage, ListingImageException>>
    {
        public async Task<Result<ListingImage, ListingImageException>> Handle(UpdateListingImageCommand request, CancellationToken cancellationToken)
        {
            var listingImageIdToUpdate = new ListingImageId(request.ListingImageId);
            var requestingUserIdObj = new UserId(request.RequestingUserId);

            // 1. Отримати зображення за ID, включаючи батьківське оголошення (Listing)
            // Важливо: Реалізація GetById в репозиторії МАЄ включати Listing
            var existingImageOption = await listingImagesRepository.GetById(listingImageIdToUpdate, cancellationToken);

             return await existingImageOption.Match<Task<Result<ListingImage, ListingImageException>>>(
                some: async listingImage => // Якщо зображення знайдено
                {
                    // 2. Перевірка авторизації: чи є поточний користувач власником оголошення
                    if (listingImage.Listing == null)
                    {
                        return new ListingImageOperationFailedException(
                            listingImage.Id,
                            "UpdateListingImage",
                            new InvalidOperationException("Parent Listing was not loaded for authorization check.")
                        );
                    }

                    if (listingImage.Listing.UserId != requestingUserIdObj)
                    {
                         return new UserNotAuthorizedToManageListingImagesException(requestingUserIdObj, listingImage.ListingId);
                    }

                    // 3. Оновити сутність зображення
                    return await UpdateListingImageEntity(listingImage, request, cancellationToken);
                },
                none: () => // Якщо зображення не знайдено
                {
                    ListingImageException exception = new ListingImageNotFoundException(listingImageIdToUpdate);
                    return Task.FromResult<Result<ListingImage, ListingImageException>>(exception);
                }
            );
        }

        private async Task<Result<ListingImage, ListingImageException>> UpdateListingImageEntity(
            ListingImage listingImage,
            UpdateListingImageCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // 4. Оновити URL в доменній моделі
                listingImage.UpdateImageUrl(request.NewImageUrl);

                // 5. Зберегти зміни через репозиторій
                var updatedImage = await listingImagesRepository.Update(listingImage, cancellationToken);
                return updatedImage; // Implicit conversion
            }
            catch (Exception exception)
            {
                // 6. Обробити можливі помилки під час збереження
                return new ListingImageOperationFailedException(listingImage.Id, "UpdateListingImage", exception);
            }
        }
    }
}