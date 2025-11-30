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
    public record UpdateListingCommand : IRequest<Result<Listing, ListingException>>
    {
        public required Guid ListingId { get; init; }

        public string? Title { get; init; }
        public string? Description { get; init; }
        public string? Address { get; init; }
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }
        public float? Price { get; init; }
        public ListingEnums.ListingType? Type { get; init; }
        public List<ListingEnums.CommunalService>? CommunalServices { get; init; }
        public ListingEnums.OwnershipType? Owners { get; init; }
        public ListingEnums.NeighbourType? Neighbours { get; init; }

        public List<Guid>? AmenityIds { get; init; }

        public required Guid RequestingUserId { get; init; }
    }

    public class UpdateListingCommandHandler(
        IListingsRepository listingsRepository,
        IAmenitiesRepository amenitiesRepository 
        )
        : IRequestHandler<UpdateListingCommand, Result<Listing, ListingException>>
    {
        public async Task<Result<Listing, ListingException>> Handle(UpdateListingCommand request, CancellationToken cancellationToken)
        {
            var listingIdToUpdate = new ListingId(request.ListingId);
            var requestingUserId = new UserId(request.RequestingUserId);

            var existingListingOption = await listingsRepository.GetById(listingIdToUpdate, cancellationToken);

            return await existingListingOption.Match<Task<Result<Listing, ListingException>>>(
                some: async listing => 
                {
                    if (listing.UserId != requestingUserId)
                    {
                        return new UserNotAuthorizedForListingOperationException(requestingUserId, listingIdToUpdate, "UpdateListing");
                    }

                    // Prepare amenity updates if provided
                    List<Amenity>? validAmenities = null; // null => no changes; empty => clear all; list => replace
                    if (request.AmenityIds != null)
                    {
                        if (!request.AmenityIds.Any())
                        {
                            validAmenities = new List<Amenity>(); // clear all
                        }
                        else
                        {
                            var uniqueAmenityIds = request.AmenityIds.Distinct().Select(id => new AmenityId(id)).ToList();
                            var foundAmenities = new List<Amenity>();
                            foreach (var amenityId in uniqueAmenityIds)
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
                            validAmenities = foundAmenities;
                        }
                    }

                    try
                    {
                        // Compute effective values (null => keep existing, empty collection => clear)
                        var newTitle = request.Title ?? listing.Title;
                        var newDescription = request.Description ?? listing.Description;
                        var newAddress = request.Address ?? listing.Address;
                        var newLatitude = request.Latitude ?? listing.Latitude;
                        var newLongitude = request.Longitude ?? listing.Longitude;
                        var newPrice = request.Price ?? listing.Price;
                        var newType = request.Type ?? listing.Type;
                        var newCommunal = request.CommunalServices != null
                            ? new List<ListingEnums.CommunalService>(request.CommunalServices)
                            : (listing.CommunalServices ?? new List<ListingEnums.CommunalService>());
                        var newOwners = request.Owners ?? listing.Owners;
                        var newNeighbours = request.Neighbours ?? listing.Neighbours;

                        listing.UpdateDetails(
                            newTitle,
                            newDescription,
                            newAddress,
                            newLatitude,
                            newLongitude,
                            newPrice,
                            newType,
                            newCommunal,
                            newOwners,
                            newNeighbours
                        );

                        if (validAmenities != null)
                        {
                            var amenitiesToRemove = listing.Amenities
                                .Where(currentAmenity => !validAmenities.Any(newAmenity => newAmenity.Id == currentAmenity.Id))
                                .ToList();

                            var amenitiesToAdd = validAmenities
                                .Where(newAmenity => !listing.Amenities.Any(currentAmenity => currentAmenity.Id == newAmenity.Id))
                                .ToList();

                            foreach (var amenityToRemove in amenitiesToRemove)
                            {
                                listing.RemoveAmenity(amenityToRemove);
                            }

                            foreach (var amenityToAdd in amenitiesToAdd)
                            {
                                listing.AddAmenity(amenityToAdd);
                            }
                        }

                        var updatedListing = await listingsRepository.Update(listing, cancellationToken);
                        return updatedListing; 
                    }
                    catch (Exception exception)
                    {
                        return new ListingOperationFailedException(listing.Id, "UpdateListing", exception);
                    }
                },
                none: () => 
                {
                    ListingException exception = new ListingNotFoundException(listingIdToUpdate);
                    return Task.FromResult<Result<Listing, ListingException>>(exception);
                }
            );
        }
    }
}