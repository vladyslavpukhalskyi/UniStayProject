using Application.Common; 
using Application.Common.Interfaces.Repositories; 
using Application.Listings.Exceptions; 
using Domain.Amenities; 
using Domain.Listings; 
using Domain.Users;   
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Listings.Commands
{
    public record CreateListingCommand : IRequest<Result<Listing, ListingException>>
    {
        public required string Title { get; init; }
        public required string Description { get; init; }
        public required string Address { get; init; }
        public required double Latitude { get; init; }
        public required double Longitude { get; init; }
        public required float Price { get; init; }
        public required ListingEnums.ListingType Type { get; init; }
        public required List<ListingEnums.CommunalService> CommunalServices { get; init; }
        public required ListingEnums.OwnershipType Owners { get; init; }
        public required ListingEnums.NeighbourType Neighbours { get; init; }
        public required List<Guid> AmenityIds { get; init; } 

        public required Guid UserId { get; init; }
    }

    public class CreateListingCommandHandler(
        IListingsRepository listingsRepository,
        IAmenitiesRepository amenitiesRepository 
        )
        : IRequestHandler<CreateListingCommand, Result<Listing, ListingException>>
    {
        public async Task<Result<Listing, ListingException>> Handle(CreateListingCommand request, CancellationToken cancellationToken)
        {
            var listingId = ListingId.New();
            var userId = new UserId(request.UserId);
            List<Amenity> amenitiesToAssociate = new List<Amenity>();

            if (request.AmenityIds != null && request.AmenityIds.Any())
            {
                var uniqueAmenityIds = request.AmenityIds.Distinct().Select(id => new AmenityId(id)).ToList();
                var foundAmenities = new List<Amenity>();

                foreach(var amenityId in uniqueAmenityIds)
                {
                    var amenityOpt = await amenitiesRepository.GetById(amenityId, cancellationToken);
                    amenityOpt.Match(
                        some: amenity => foundAmenities.Add(amenity), 
                        none: () => { } 
                    );
                }

                if (foundAmenities.Count != uniqueAmenityIds.Count)
                {
                    var foundIds = foundAmenities.Select(a => a.Id.Value); 
                    var invalidIds = request.AmenityIds.Distinct().Where(id => !foundIds.Contains(id));
                    return new InvalidAmenitiesProvidedException(invalidIds);
                }
                amenitiesToAssociate = foundAmenities;
            }

            try
            {
                var listing = Listing.New(
                    id: listingId,
                    title: request.Title,
                    description: request.Description,
                    address: request.Address,
                    latitude: request.Latitude,
                    longitude: request.Longitude,
                    price: request.Price,
                    type: request.Type,
                    userId: userId,
                    communalServices: request.CommunalServices ?? new List<ListingEnums.CommunalService>(),
                    owners: request.Owners,
                    neighbours: request.Neighbours,
                    publicationDate: DateTime.UtcNow 
                );

                foreach (var amenity in amenitiesToAssociate)
                {
                    listing.AddAmenity(amenity);
                }

                var addedListing = await listingsRepository.Add(listing, cancellationToken);
                return addedListing; 
            }
            catch (Exception exception)
            {
                return new ListingOperationFailedException(listingId, "CreateListing", exception);
            }
        }
    }
}