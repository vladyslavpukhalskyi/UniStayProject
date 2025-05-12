// Файл: Application/ListingImages/Commands/DeleteListingImageCommand.cs
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
    /// Команда для видалення зображення оголошення.
    /// </summary>
    public record DeleteListingImageCommand : IRequest<Result<ListingImage, ListingImageException>>
    {
        /// <summary>
        /// ID зображення, яке потрібно видалити.
        /// </summary>
        public required Guid ListingImageId { get; init; }

        /// <summary>
        /// ID користувача, який запитує видалення (для перевірки власності оголошення).
        /// Встановлюється з контексту аутентифікації.
        /// </summary>
        public required Guid RequestingUserId { get; init; }
    }

    /// <summary>
    /// Обробник команди DeleteListingImageCommand.
    /// </summary>
    public class DeleteListingImageCommandHandler(
        IListingImagesRepository listingImagesRepository
        )
        : IRequestHandler<DeleteListingImageCommand, Result<ListingImage, ListingImageException>>
    {
        public async Task<Result<ListingImage, ListingImageException>> Handle(DeleteListingImageCommand request, CancellationToken cancellationToken)
        {
            var listingImageIdToDelete = new ListingImageId(request.ListingImageId);
            var requestingUserIdObj = new UserId(request.RequestingUserId);

            // 1. Отримати зображення за ID, включаючи батьківське оголошення (Listing)
            // Важливо: Реалізація GetById в репозиторії МАЄ включати Listing
            var existingImageOption = await listingImagesRepository.GetById(listingImageIdToDelete, cancellationToken);

            return await existingImageOption.Match<Task<Result<ListingImage, ListingImageException>>>(
                some: async listingImage => // Якщо зображення знайдено
                {
                    // 2. Перевірка авторизації: чи є поточний користувач власником оголошення
                    if (listingImage.Listing == null)
                    {
                        // Це не повинно статися, якщо репозиторій правильно завантажує Listing
                        return new ListingImageOperationFailedException(
                            listingImage.Id,
                            "DeleteListingImage",
                            new InvalidOperationException("Parent Listing was not loaded for authorization check.")
                        );
                    }

                    if (listingImage.Listing.UserId != requestingUserIdObj)
                    {
                        // Користувач не є власником оголошення, до якого належить зображення
                        return new UserNotAuthorizedToManageListingImagesException(requestingUserIdObj, listingImage.ListingId);
                    }

                    // 3. Видалити сутність зображення
                    return await DeleteListingImageEntity(listingImage, cancellationToken);
                },
                none: () => // Якщо зображення не знайдено
                {
                    ListingImageException exception = new ListingImageNotFoundException(listingImageIdToDelete);
                    return Task.FromResult<Result<ListingImage, ListingImageException>>(exception);
                }
            );
        }

        private async Task<Result<ListingImage, ListingImageException>> DeleteListingImageEntity(ListingImage listingImage, CancellationToken cancellationToken)
        {
            try
            {
                // 4. Видалити зображення через репозиторій
                var deletedImage = await listingImagesRepository.Delete(listingImage, cancellationToken);
                // Можливо, також потрібно видалити файл зображення з файлового сховища тут або через подію
                return deletedImage; // Implicit conversion
            }
            catch (Exception exception)
            {
                // 5. Обробити можливі помилки під час видалення
                return new ListingImageOperationFailedException(listingImage.Id, "DeleteListingImage", exception);
            }
        }
    }
}