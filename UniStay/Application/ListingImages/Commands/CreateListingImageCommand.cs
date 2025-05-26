using Application.Common; 
using Application.Common.Interfaces.Repositories; 
using Application.ListingImages.Exceptions; 
using Domain.ListingImages; 
using Domain.Listings;    
using Domain.Users;       
using MediatR;
using Optional; 
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.ListingImages.Commands
{
    public record CreateListingImageCommand : IRequest<Result<ListingImage, ListingImageException>>
    {
        public required Guid ListingId { get; init; }

        public required string ImageUrl { get; init; }

        public required Guid RequestingUserId { get; init; }
    }

    public class CreateListingImageCommandHandler(
        IListingImagesRepository listingImagesRepository,
        IListingsRepository listingsRepository 
        )
        : IRequestHandler<CreateListingImageCommand, Result<ListingImage, ListingImageException>>
    {
        public async Task<Result<ListingImage, ListingImageException>> Handle(CreateListingImageCommand request, CancellationToken cancellationToken)
        {
            var listingIdObj = new ListingId(request.ListingId);
            var requestingUserIdObj = new UserId(request.RequestingUserId);

            var listingOption = await listingsRepository.GetById(listingIdObj, cancellationToken);

            return await listingOption.Match<Task<Result<ListingImage, ListingImageException>>>(
                some: async listing => 
                {
                    if (listing.UserId != requestingUserIdObj)
                    {
                        return new UserNotAuthorizedToManageListingImagesException(requestingUserIdObj, listingIdObj);
                    }

                    var listingImageId = ListingImageId.New();
                    try
                    {
                        var listingImage = ListingImage.New(
                            id: listingImageId,
                            listingId: listingIdObj,
                            imageUrl: request.ImageUrl
                        );

                        var addedImage = await listingImagesRepository.Add(listingImage, cancellationToken);
                        return addedImage; 
                    }
                    catch (Exception ex)
                    {
                        return new ListingImageOperationFailedException(listingImageId, "CreateListingImage", ex);
                    }
                },
                none: () => 
                {
                    ListingImageException exception = new ListingNotFoundForImageException(listingIdObj);
                    return Task.FromResult<Result<ListingImage, ListingImageException>>(exception);
                }
            );
        }
    }
}