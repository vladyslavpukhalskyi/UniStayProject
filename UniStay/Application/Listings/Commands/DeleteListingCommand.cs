using Application.Common; 
using Application.Common.Interfaces.Repositories; 
using Application.Listings.Exceptions; 
using Domain.Listings; 
using Domain.Users;   
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Listings.Commands
{
    public record DeleteListingCommand : IRequest<Result<Listing, ListingException>>
    {
        public required Guid ListingId { get; init; }

        public required Guid RequestingUserId { get; init; }
    }

    public class DeleteListingCommandHandler(
        IListingsRepository listingsRepository)
        : IRequestHandler<DeleteListingCommand, Result<Listing, ListingException>>
    {
        public async Task<Result<Listing, ListingException>> Handle(DeleteListingCommand request, CancellationToken cancellationToken)
        {
            var listingIdToDelete = new ListingId(request.ListingId);
            var requestingUserId = new UserId(request.RequestingUserId);

            var existingListingOption = await listingsRepository.GetById(listingIdToDelete, cancellationToken);

            return await existingListingOption.Match<Task<Result<Listing, ListingException>>>(
                some: async listing => 
                {
                    if (listing.UserId != requestingUserId)
                    {
                        return new UserNotAuthorizedForListingOperationException(requestingUserId, listingIdToDelete, "DeleteListing");
                    }

                    return await DeleteListingEntity(listing, cancellationToken);
                },
                none: () => 
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
                var deletedListing = await listingsRepository.Delete(listing, cancellationToken);
                return deletedListing; 
            }
            catch (Exception exception)
            {
                return new ListingOperationFailedException(listing.Id, "DeleteListing", exception);
            }
        }
    }
}