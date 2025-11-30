using Microsoft.AspNetCore.Mvc;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Api.Dtos;
using Application.Listings.Commands;
using Application.Listings.Exceptions;
using Application.Common.Interfaces.Queries;
using Domain.Listings;
using Domain.Users;
using Api.Modules.Errors;
using Optional;

namespace Api.Controllers
{
    [Route("api/listings")]
    [ApiController]
    public class ListingsController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IListingsQueries _listingsQueries;

        public ListingsController(ISender sender, IListingsQueries listingsQueries)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _listingsQueries = listingsQueries ?? throw new ArgumentNullException(nameof(listingsQueries));
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
    }
}