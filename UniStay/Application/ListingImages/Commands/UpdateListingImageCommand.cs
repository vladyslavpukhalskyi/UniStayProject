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
    public record UpdateListingImageCommand : IRequest<Result<ListingImage, ListingImageException>>
    {
        public required Guid ListingImageId { get; init; }

        public required string NewImageUrl { get; init; }

        public required Guid RequestingUserId { get; init; }
    }

    public class UpdateListingImageCommandHandler(
        IListingImagesRepository listingImagesRepository
        )
        : IRequestHandler<UpdateListingImageCommand, Result<ListingImage, ListingImageException>>
    {
        public async Task<Result<ListingImage, ListingImageException>> Handle(UpdateListingImageCommand request, CancellationToken cancellationToken)
        {
            var listingImageIdToUpdate = new ListingImageId(request.ListingImageId);
            var requestingUserIdObj = new UserId(request.RequestingUserId);

            var existingImageOption = await listingImagesRepository.GetById(listingImageIdToUpdate, cancellationToken);

             return await existingImageOption.Match<Task<Result<ListingImage, ListingImageException>>>(
                some: async listingImage => 
                {
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

                    return await UpdateListingImageEntity(listingImage, request, cancellationToken);
                },
                none: () => 
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
                listingImage.UpdateImageUrl(request.NewImageUrl);

                var updatedImage = await listingImagesRepository.Update(listingImage, cancellationToken);
                return updatedImage; 
            }
            catch (Exception exception)
            {
                return new ListingImageOperationFailedException(listingImage.Id, "UpdateListingImage", exception);
            }
        }
    }
}