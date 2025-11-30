using Application.Common; 
using Application.Common.Interfaces.Repositories; 
using Application.Amenities.Exceptions; 
using Domain.Amenities; 
using MediatR;
using Optional; 
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Amenities.Commands
{
    public record UpdateAmenityCommand : IRequest<Result<Amenity, AmenityException>>
    {
        public required Guid AmenityId { get; init; }

        public string? Title { get; init; }
    }

    public class UpdateAmenityCommandHandler(
        IAmenitiesRepository amenitiesRepository
        ) : IRequestHandler<UpdateAmenityCommand, Result<Amenity, AmenityException>>
    {
        public async Task<Result<Amenity, AmenityException>> Handle(UpdateAmenityCommand request, CancellationToken cancellationToken)
        {
            var amenityIdToUpdate = new AmenityId(request.AmenityId);

            var existingAmenityOption = await amenitiesRepository.GetById(amenityIdToUpdate, cancellationToken);

            return await existingAmenityOption.Match<Task<Result<Amenity, AmenityException>>>(
                some: async amenityToUpdate => 
                {
                    if (request.Title != null && !string.Equals(amenityToUpdate.Title, request.Title, StringComparison.OrdinalIgnoreCase))
                    {
                        var conflictingAmenityOption = await amenitiesRepository.GetByTitleAsync(request.Title, cancellationToken);

                        return await conflictingAmenityOption.Match<Task<Result<Amenity, AmenityException>>>(
                            some: async conflictingAmenity => 
                            {
                                if (conflictingAmenity.Id != amenityToUpdate.Id)
                                {
                                    AmenityException exception = new AmenityAlreadyExistsException(request.Title, conflictingAmenity.Id);
                                    return exception; 
                                }
                                return await UpdateAmenityEntity(amenityToUpdate, request, cancellationToken);
                            },
                            none: async () => 
                            {
                                return await UpdateAmenityEntity(amenityToUpdate, request, cancellationToken);
                            }
                        );
                    }
                    else
                    {
                        return await UpdateAmenityEntity(amenityToUpdate, request, cancellationToken);
                    }
                },
                none: () => 
                {
                    AmenityException exception = new AmenityNotFoundException(amenityIdToUpdate);
                    return Task.FromResult<Result<Amenity, AmenityException>>(exception);
                }
            );
        }

        private async Task<Result<Amenity, AmenityException>> UpdateAmenityEntity(
            Amenity amenity,
            UpdateAmenityCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (request.Title != null)
                {
                    amenity.UpdateTitle(request.Title);
                }

                var updatedAmenity = await amenitiesRepository.Update(amenity, cancellationToken);
                return updatedAmenity; 
            }
            catch (Exception ex)
            {
                return new AmenityOperationFailedException(amenity.Id, "UpdateAmenity", ex);
            }
        }
    }
}