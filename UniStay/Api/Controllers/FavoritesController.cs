using Microsoft.AspNetCore.Mvc;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Api.Dtos;
using Application.Favorites.Commands;
using Application.Favorites.Exceptions;
using Application.Common.Interfaces.Queries;
using Domain.Listings;
using Domain.Users;
using Domain.Favorites;
using Api.Modules.Errors;
using Optional;

namespace Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IFavoritesQueries _favoritesQueries;

        public FavoritesController(ISender sender, IFavoritesQueries favoritesQueries)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _favoritesQueries = favoritesQueries ?? throw new ArgumentNullException(nameof(favoritesQueries));
        }

        [HttpPost("listings/{listingId:guid}/favorite")]
        [ProducesResponseType(typeof(FavoriteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddListingToFavorites([FromRoute] Guid listingId, CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid currentUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

            var command = new CreateFavoriteCommand
            {
                ListingId = listingId,
                UserId = currentUserId
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                favorite => Ok(FavoriteDto.FromDomainModel(favorite)),
                favoriteException => favoriteException.ToObjectResult()
            );
        }

        [HttpDelete("listings/{listingId:guid}/favorite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveListingFromFavorites([FromRoute] Guid listingId, CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid currentUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

            var command = new DeleteFavoriteCommand
            {
                ListingId = listingId,
                UserId = currentUserId
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                _ => NoContent(),
                favoriteException => favoriteException.ToObjectResult()
            );
        }

        [HttpGet("me/favorites")]
        [ProducesResponseType(typeof(IReadOnlyList<ListingSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IReadOnlyList<ListingSummaryDto>>> GetMyFavoriteListings(CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid currentUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

            var favoriteRecords = await _favoritesQueries.GetByUserId(new UserId(currentUserId), cancellationToken);
            
            var listingsFavoritedByUser = favoriteRecords
                .Where(f => f.Listing != null)
                .Select(f => ListingSummaryDto.FromDomainModel(f.Listing!))
                .ToList();

            return Ok(listingsFavoritedByUser);
        }

        [HttpGet("listings/{listingId:guid}/favorites-info")]
        [ProducesResponseType(typeof(FavoriteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FavoriteDto>> GetFavoriteInfoForListing([FromRoute] Guid listingId, CancellationToken cancellationToken)
        {
            var favoriteRecords = await _favoritesQueries.GetByListingId(new ListingId(listingId), cancellationToken);
            var favoriteRecord = favoriteRecords.FirstOrDefault();

            if (favoriteRecord == null)
            {
                return NotFound(new { Message = $"No favorite information found for listing {listingId}." });
            }

            return Ok(FavoriteDto.FromDomainModel(favoriteRecord));
        }
    
        [HttpGet("favorites/{favoriteId:guid}")]
        [ProducesResponseType(typeof(FavoriteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FavoriteDto>> GetFavoriteRecordById([FromRoute] Guid favoriteId, CancellationToken cancellationToken)
        {
            var favoriteOption = await _favoritesQueries.GetById(new FavoriteId(favoriteId), cancellationToken);
            
            return favoriteOption.Match<ActionResult<FavoriteDto>>(
                favorite => Ok(FavoriteDto.FromDomainModel(favorite)),
                () => NotFound(new { Message = $"Favorite record with id {favoriteId} not found." })
            );
        }
    }
}