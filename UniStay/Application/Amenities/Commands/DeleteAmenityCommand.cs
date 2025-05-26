using Application.Common; 
using Application.Common.Interfaces.Repositories; 
using Application.Amenities.Exceptions; 
using Domain.Amenities; 
using MediatR;
using Microsoft.EntityFrameworkCore; 
using Optional; 
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Amenities.Commands
{
    public record DeleteAmenityCommand : IRequest<Result<Amenity, AmenityException>>
    {
        public required Guid AmenityId { get; init; }
    }

    public class DeleteAmenityCommandHandler(
        IAmenitiesRepository amenitiesRepository)
        : IRequestHandler<DeleteAmenityCommand, Result<Amenity, AmenityException>>
    {
        public async Task<Result<Amenity, AmenityException>> Handle(DeleteAmenityCommand request, CancellationToken cancellationToken)
        {
            var amenityIdToDelete = new AmenityId(request.AmenityId);

            var existingAmenityOption = await amenitiesRepository.GetById(amenityIdToDelete, cancellationToken);

            return await existingAmenityOption.Match<Task<Result<Amenity, AmenityException>>>(
                some: async amenity => 
                {
                    return await DeleteAmenityEntity(amenity, cancellationToken);
                },
                none: () => 
                {
                    AmenityException exception = new AmenityNotFoundException(amenityIdToDelete);
                    return Task.FromResult<Result<Amenity, AmenityException>>(exception);
                }
            );
        }

        private async Task<Result<Amenity, AmenityException>> DeleteAmenityEntity(Amenity amenity, CancellationToken cancellationToken)
        {
            try
            {
                var deletedAmenity = await amenitiesRepository.Delete(amenity, cancellationToken);
                return deletedAmenity; 
            }
            catch (DbUpdateException dbEx) 
            {
                 return new AmenityOperationFailedException(amenity.Id, "DeleteAmenity (likely due to FK constraint)", dbEx);
            }
            catch (Exception exception)
            {
                return new AmenityOperationFailedException(amenity.Id, "DeleteAmenity", exception);
            }
        }
    }
}