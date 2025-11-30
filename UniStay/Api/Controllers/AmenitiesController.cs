using Microsoft.AspNetCore.Mvc;
using MediatR;
using Api.Dtos;
using Application.Amenities.Commands;
using Application.Common.Interfaces.Queries;
using Domain.Amenities;
using Api.Modules.Errors;

namespace Api.Controllers
{
    [Route("api/amenities")]
    [ApiController]
    public class AmenitiesController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IAmenitiesQueries _amenitiesQueries;

        public AmenitiesController(ISender sender, IAmenitiesQueries amenitiesQueries)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _amenitiesQueries = amenitiesQueries ?? throw new ArgumentNullException(nameof(amenitiesQueries));
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<AmenityDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<AmenityDto>>> GetAllAmenities(CancellationToken cancellationToken)
        {
            var amenities = await _amenitiesQueries.GetAll(cancellationToken);
            var amenityDtos = amenities.Select(AmenityDto.FromDomainModel).ToList();
            return Ok(amenityDtos);
        }

        [HttpGet("{amenityId:guid}", Name = "GetAmenityById")]
        [ProducesResponseType(typeof(AmenityDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AmenityDto>> GetAmenityById([FromRoute] Guid amenityId, CancellationToken cancellationToken)
        {
            var amenityOption = await _amenitiesQueries.GetById(new AmenityId(amenityId), cancellationToken);

            return amenityOption.Match<ActionResult<AmenityDto>>(
                amenity => Ok(AmenityDto.FromDomainModel(amenity)),
                () => NotFound(new { Message = $"Amenity with id {amenityId} not found." })
            );
        }

        [HttpGet("by-title")]
        [ProducesResponseType(typeof(AmenityDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AmenityDto>> GetAmenityByTitle([FromQuery] string title, CancellationToken cancellationToken)
        {
            var amenityOption = await _amenitiesQueries.GetByTitle(title, cancellationToken);

            return amenityOption.Match<ActionResult<AmenityDto>>(
                amenity => Ok(AmenityDto.FromDomainModel(amenity)),
                () => NotFound(new { Message = $"Amenity with title '{title}' not found." })
            );
        }

        [HttpPost]
        [ProducesResponseType(typeof(AmenityDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateAmenity([FromBody] CreateAmenityDto requestDto, CancellationToken cancellationToken)
        {
            var command = new CreateAmenityCommand
            {
                Title = requestDto.Title
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                createdAmenity => CreatedAtRoute("GetAmenityById", new { amenityId = createdAmenity.Id.Value }, AmenityDto.FromDomainModel(createdAmenity)),
                amenityException => amenityException.ToObjectResult()
            );
        }

        [HttpPut("{amenityId:guid}")]
        [ProducesResponseType(typeof(AmenityDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateAmenity(
            [FromRoute] Guid amenityId,
            [FromBody] UpdateAmenityDto requestDto,
            CancellationToken cancellationToken)
        {
            var command = new UpdateAmenityCommand
            {
                AmenityId = amenityId,
                Title = requestDto.Title
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                updatedAmenity => Ok(AmenityDto.FromDomainModel(updatedAmenity)),
                amenityException => amenityException.ToObjectResult()
            );
        }

        [HttpDelete("{amenityId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAmenity([FromRoute] Guid amenityId, CancellationToken cancellationToken)
        {
            var command = new DeleteAmenityCommand
            {
                AmenityId = amenityId
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                deletedAmenity => NoContent(),
                amenityException => amenityException.ToObjectResult()
            );
        }
    }
}