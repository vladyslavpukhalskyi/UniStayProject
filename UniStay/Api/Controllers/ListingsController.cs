using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using Api.Dtos.Listing;
using Application.Listings.Commands;
using Application.Listings.Queries;
using Application.Common.Interfaces.Queries;
using Domain.Listings;
using Domain.Users;
using Api.Modules.Errors;
using Application.Listings.Services;

namespace Api.Controllers
{
    [Route("api/listings")]
    [ApiController]
    public class ListingsController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IListingsQueries _listingsQueries;
        private readonly IListingComparisonDtoMapper _comparisonDtoMapper;

        public ListingsController(ISender sender, IListingsQueries listingsQueries, IListingComparisonDtoMapper comparisonDtoMapper)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _listingsQueries = listingsQueries ?? throw new ArgumentNullException(nameof(listingsQueries));
            _comparisonDtoMapper = comparisonDtoMapper ?? throw new ArgumentNullException(nameof(comparisonDtoMapper));
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<ListingDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ListingDto>>> GetAllListings(CancellationToken cancellationToken)
        {
            var listings = await _listingsQueries.GetAll(cancellationToken);
            var listingDtos = listings.Select(ListingDto.FromDomainModel).ToList();
            return Ok(listingDtos);
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(IReadOnlyList<ListingDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ListingDto>>> SearchListings([FromQuery] string keyword, CancellationToken cancellationToken)
        {
            var listings = await _listingsQueries.Search(keyword, cancellationToken);
            var listingDtos = listings.Select(ListingDto.FromDomainModel).ToList();
            return Ok(listingDtos);
        }

        [HttpGet("user/{userId:guid}")]
        [ProducesResponseType(typeof(IReadOnlyList<ListingDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ListingDto>>> GetListingsByUserId([FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            var listings = await _listingsQueries.GetByUserId(new UserId(userId), cancellationToken);
            var listingDtos = listings.Select(ListingDto.FromDomainModel).ToList();
            return Ok(listingDtos);
        }

        [HttpGet("{listingId:guid}", Name = "GetListingById")]
        [ProducesResponseType(typeof(ListingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ListingDto>> GetListingById([FromRoute] Guid listingId, CancellationToken cancellationToken)
        {
            var listingOption = await _listingsQueries.GetById(new ListingId(listingId), cancellationToken);

            return listingOption.Match<ActionResult<ListingDto>>(
                listing => Ok(ListingDto.FromDomainModel(listing)),
                () => NotFound(new { Message = $"Listing with id {listingId} not found." })
            );
        }

        [HttpPost]
        [ProducesResponseType(typeof(ListingDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateListing([FromBody] CreateListingDto requestDto, CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid authenticatedUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

            var command = new CreateListingCommand
            {
                Title = requestDto.Title,
                Description = requestDto.Description,
                Address = requestDto.Address,
                Latitude = requestDto.Latitude,
                Longitude = requestDto.Longitude,
                Price = requestDto.Price,
                Type = requestDto.Type,
                CommunalServices = requestDto.CommunalServices,
                Owners = requestDto.Owners,
                Neighbours = requestDto.Neighbours,
                AmenityIds = requestDto.AmenityIds,
                UserId = authenticatedUserId
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                createdListing => CreatedAtRoute("GetListingById", new { listingId = createdListing.Id.Value }, ListingDto.FromDomainModel(createdListing)),
                listingException => listingException.ToObjectResult()
            );
        }

        [HttpPut("{listingId:guid}")]
        [ProducesResponseType(typeof(ListingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateListing(
            [FromRoute] Guid listingId,
            [FromBody] UpdateListingDto requestDto,
            CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid authenticatedUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

            var command = new UpdateListingCommand
            {
                ListingId = listingId,
                Title = requestDto.Title,
                Description = requestDto.Description,
                Address = requestDto.Address,
                Latitude = requestDto.Latitude,
                Longitude = requestDto.Longitude,
                Price = requestDto.Price,
                Type = requestDto.Type,
                CommunalServices = requestDto.CommunalServices,
                Owners = requestDto.Owners,
                Neighbours = requestDto.Neighbours,
                AmenityIds = requestDto.AmenityIds,
                RequestingUserId = authenticatedUserId
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                updatedListing => Ok(ListingDto.FromDomainModel(updatedListing)),
                listingException => listingException.ToObjectResult()
            );
        }

        [HttpDelete("{listingId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteListing(
            [FromRoute] Guid listingId,
            CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid authenticatedUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

            var command = new DeleteListingCommand
            {
                ListingId = listingId,
                RequestingUserId = authenticatedUserId
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                deletedListing => NoContent(),
                listingException => listingException.ToObjectResult()
            );
        }

        [HttpGet("compare")]
        [ProducesResponseType(typeof(ListingComparisonDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CompareListings(
            [FromQuery] Guid listing1Id,
            [FromQuery] Guid listing2Id,
            CancellationToken cancellationToken)
        {
            if (listing1Id == listing2Id)
            {
                return BadRequest(new { Message = "Cannot compare a listing with itself." });
            }

            var query = new CompareListingsQuery
            {
                Listing1Id = listing1Id,
                Listing2Id = listing2Id
            };

            var result = await _sender.Send(query, cancellationToken);

            return await result.Match(
                async comparisonResult => 
                    (IActionResult)Ok(await _comparisonDtoMapper.MapToDto(comparisonResult, cancellationToken)),
                listingException => Task.FromResult<IActionResult>(listingException.ToObjectResult())
            );
        }
    }
}