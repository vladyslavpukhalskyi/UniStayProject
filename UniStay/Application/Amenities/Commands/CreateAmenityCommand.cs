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
    public record CreateAmenityCommand : IRequest<Result<Amenity, AmenityException>>
    {
        public required string Title { get; init; }
    }

    public class CreateAmenityCommandHandler(
        IAmenitiesRepository amenitiesRepository
    ) : IRequestHandler<CreateAmenityCommand, Result<Amenity, AmenityException>>
    {
        public async Task<Result<Amenity, AmenityException>> Handle(CreateAmenityCommand request, CancellationToken cancellationToken)
        {
            var existingAmenityOption = await amenitiesRepository.GetByTitleAsync(request.Title, cancellationToken);

            return await existingAmenityOption.Match<Task<Result<Amenity, AmenityException>>>(
                some: amenity => 
                {
                    AmenityException exception = new AmenityAlreadyExistsException(request.Title, amenity.Id);
                    return Task.FromResult<Result<Amenity, AmenityException>>(exception);
                },
                none: async () => 
                {
                    var amenityId = AmenityId.New();
                    try
                    {
                        var amenity = Amenity.New(
                            id: amenityId,
                            title: request.Title
                        );

                        var addedAmenity = await amenitiesRepository.Add(amenity, cancellationToken);
                        return addedAmenity; 
                    }
                    catch (Exception ex)
                    {
                        return new AmenityOperationFailedException(amenityId, "CreateAmenity", ex);
                    }
                }
            );
        }
    }
}