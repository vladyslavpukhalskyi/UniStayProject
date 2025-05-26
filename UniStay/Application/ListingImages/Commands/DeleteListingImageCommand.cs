using Application.Common; 
using Application.Common.Interfaces.Repositories; 
using Application.ListingImages.Exceptions; 
using Domain.ListingImages; 
using Domain.Users;       
using MediatR;
using Optional; 
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.ListingImages.Commands
{
    public record DeleteListingImageCommand : IRequest<Result<ListingImage, ListingImageException>>
    {
        public required Guid ListingImageId { get; init; }

        public required Guid RequestingUserId { get; init; }
    }

    public class DeleteListingImageCommandHandler(
        IListingImagesRepository listingImagesRepository
        )
        : IRequestHandler<DeleteListingImageCommand, Result<ListingImage, ListingImageException>>
    {
        public async Task<Result<ListingImage, ListingImageException>> Handle(DeleteListingImageCommand request, CancellationToken cancellationToken)
        {
            var listingImageIdToDelete = new ListingImageId(request.ListingImageId);
            var requestingUserIdObj = new UserId(request.RequestingUserId);

            var existingImageOption = await listingImagesRepository.GetById(listingImageIdToDelete, cancellationToken);

            return await existingImageOption.Match<Task<Result<ListingImage, ListingImageException>>>(
                some: async listingImage => 
                {
                    if (listingImage.Listing == null)
                    {
                        return new ListingImageOperationFailedException(
                            listingImage.Id,
                            "DeleteListingImage",
                            new InvalidOperationException("Parent Listing was not loaded for authorization check.")
                        );
                    }

                    if (listingImage.Listing.UserId != requestingUserIdObj)
                    {
                        return new UserNotAuthorizedToManageListingImagesException(requestingUserIdObj, listingImage.ListingId);
                    }

                    return await DeleteListingImageEntity(listingImage, cancellationToken);
                },
                none: () => 
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
                var deletedImage = await listingImagesRepository.Delete(listingImage, cancellationToken);
                return deletedImage; 
            }
            catch (Exception exception)
            {
                return new ListingImageOperationFailedException(listingImage.Id, "DeleteListingImage", exception);
            }
        }
    }
}