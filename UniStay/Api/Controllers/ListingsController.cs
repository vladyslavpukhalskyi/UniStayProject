using Microsoft.AspNetCore.Mvc;
using MediatR; // Для ISender
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // Для отримання UserId з Claims
using System.Threading;
using System.Threading.Tasks;
using Api.Dtos; // Розташування ваших ListingDto, CreateListingDto, UpdateListingDto та інших DTO
using Application.Listings.Commands; // Розташування ваших команд для Listing
using Application.Listings.Exceptions; // Для ListingException
using Application.Common.Interfaces.Queries; // Припускаємо, що IListingsQueries тут
using Domain.Listings; // Для ListingId, ListingEnums
using Domain.Users;   // Для UserId
using Api.Modules.Errors; // Для ListingErrorHandler.ToObjectResult()
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

        // GET: api/listings
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<ListingDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ListingDto>>> GetAllListings(CancellationToken cancellationToken)
        {
            var listings = await _listingsQueries.GetAll(cancellationToken);
            var listingDtos = listings.Select(ListingDto.FromDomainModel).ToList();
            return Ok(listingDtos);
        }

        // GET: api/listings/search?keyword=example
        [HttpGet("search")]
        [ProducesResponseType(typeof(IReadOnlyList<ListingDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ListingDto>>> SearchListings([FromQuery] string keyword, CancellationToken cancellationToken)
        {
            var listings = await _listingsQueries.Search(keyword, cancellationToken);
            var listingDtos = listings.Select(ListingDto.FromDomainModel).ToList();
            return Ok(listingDtos);
        }

        // GET: api/listings/user/{userId}
        [HttpGet("user/{userId:guid}")]
        [ProducesResponseType(typeof(IReadOnlyList<ListingDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ListingDto>>> GetListingsByUserId([FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            var listings = await _listingsQueries.GetByUserId(new UserId(userId), cancellationToken);
            var listingDtos = listings.Select(ListingDto.FromDomainModel).ToList();
            return Ok(listingDtos);
        }

        // GET: api/listings/{listingId}
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

        // POST: api/listings
        // [Authorize] // TODO: Додайте авторизацію
        [HttpPost]
        [ProducesResponseType(typeof(ListingDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Також для InvalidAmenitiesProvidedException
        [ProducesResponseType(StatusCodes.Status403Forbidden)] // Якщо авторизація на рівні створення
        public async Task<IActionResult> CreateListing([FromBody] CreateListingDto requestDto, CancellationToken cancellationToken)
        {
            // TODO: Отримати UserId з контексту аутентифікованого користувача
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

        // PUT: api/listings/{listingId}
        // [Authorize] // TODO: Додайте авторизацію
        [HttpPut("{listingId:guid}")]
        [ProducesResponseType(typeof(ListingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Також для InvalidAmenitiesProvidedException
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateListing(
            [FromRoute] Guid listingId,
            [FromBody] UpdateListingDto requestDto,
            CancellationToken cancellationToken)
        {
            // TODO: Отримати RequestingUserId з контексту аутентифікованого користувача
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

        // DELETE: api/listings/{listingId}
        // [Authorize] // TODO: Додайте авторизацію
        [HttpDelete("{listingId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteListing(
            [FromRoute] Guid listingId,
            CancellationToken cancellationToken)
        {
            // TODO: Отримати RequestingUserId з контексту аутентифікованого користувача
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