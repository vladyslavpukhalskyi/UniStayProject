using Microsoft.AspNetCore.Mvc;
using MediatR; // Для ISender
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // Для отримання UserId з Claims
using System.Threading;
using System.Threading.Tasks;
using Api.Dtos; // Розташування ваших FavoriteDto, CreateFavoriteDto, ListingSummaryDto
using Application.Favorites.Commands; // Розташування ваших команд для Favorite
using Application.Favorites.Exceptions; // Для FavoriteException
using Application.Common.Interfaces.Queries; // Припускаємо, що IFavoritesQueries тут
using Domain.Listings; // Для ListingId
using Domain.Users;   // Для UserId
using Domain.Favorites; // Для FavoriteId
using Api.Modules.Errors; // Для FavoriteErrorHandler.ToObjectResult()
using Optional;

namespace Api.Controllers
{
    [Route("api")] // Базовий маршрут
    [ApiController]
    // [Authorize] // TODO: Загальна авторизація для всього контролера, якщо потрібно
    public class FavoritesController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IFavoritesQueries _favoritesQueries;

        public FavoritesController(ISender sender, IFavoritesQueries favoritesQueries)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _favoritesQueries = favoritesQueries ?? throw new ArgumentNullException(nameof(favoritesQueries));
        }

        // POST: api/listings/{listingId}/favorite (Додати оголошення в обране поточного користувача)
        // [Authorize] // TODO: Додайте авторизацію
        [HttpPost("listings/{listingId:guid}/favorite")]
        [ProducesResponseType(typeof(FavoriteDto), StatusCodes.Status200OK)] // Або 201, якщо вважати Favorite запис новим
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Якщо оголошення або користувач не знайдений
        [ProducesResponseType(StatusCodes.Status409Conflict)] // Якщо вже в обраному
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

            // CreateFavoriteCommand повертає Result<Favorite, FavoriteException>
            // Favorite тут - це оновлений або новостворений запис Favorite для ListingId
            return result.Match<IActionResult>(
                favorite => Ok(FavoriteDto.FromDomainModel(favorite)), // Повертаємо стан обраного для оголошення
                favoriteException => favoriteException.ToObjectResult()
            );
        }

        // DELETE: api/listings/{listingId}/favorite (Видалити оголошення з обраного поточного користувача)
        // [Authorize] // TODO: Додайте авторизацію
        [HttpDelete("listings/{listingId:guid}/favorite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Якщо користувач/оголошення не знайдено або не було в обраному
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

            var result = await _sender.Send(command, cancellationToken); // Повертає Result<Unit, FavoriteException>

            return result.Match<IActionResult>(
                _ => NoContent(), // Успішне видалення
                favoriteException => favoriteException.ToObjectResult()
            );
        }

        // GET: api/me/favorites (Отримати список оголошень, доданих поточним користувачем в обране)
        // [Authorize] // TODO: Додайте авторизацію
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

            // GetByUserId повертає IReadOnlyList<Favorite>
            // Кожен Favorite містить ListingId та список Users, які додали це Listing в обране.
            // Якщо ми хочемо список оголошень, які САМЕ ЦЕЙ користувач додав в обране:
            var favoriteRecords = await _favoritesQueries.GetByUserId(new UserId(currentUserId), cancellationToken);
            
            // favoriteRecords - це список Favorite об'єктів, де поточний користувач є одним з Users.
            // Кожен такий Favorite об'єкт пов'язаний з Listing.
            // Нам потрібно витягнути ці Listing.
            var listingsFavoritedByUser = favoriteRecords
                .Where(f => f.Listing != null) // Переконуємось, що Listing завантажено
                .Select(f => ListingSummaryDto.FromDomainModel(f.Listing!)) // ! для довіри, що Listing не null
                .ToList();

            return Ok(listingsFavoritedByUser);
        }

        // GET: api/listings/{listingId}/favorites-info (Отримати інформацію про те, хто додав оголошення в обране)
        [HttpGet("listings/{listingId:guid}/favorites-info")]
        [ProducesResponseType(typeof(FavoriteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FavoriteDto>> GetFavoriteInfoForListing([FromRoute] Guid listingId, CancellationToken cancellationToken)
        {
            // Ваш IFavoritesQueries.GetByListingId повертає IReadOnlyList<Favorite>.
            // Але згідно вашої моделі Favorite (де один запис Favorite на ListingId),
            // тут має бути лише один або нуль записів.
            // Давайте припустимо, що GetByListingId повертає Option<Favorite> або ми беремо FirstOrDefault.
            // Якщо ваш IFavoritesQueries.GetByListingId дійсно повертає список, то вам треба буде взяти FirstOrDefault.
            // Для прикладу, я припускаю, що він має бути Task<Option<Favorite>>:
            
            // Якщо GetByListingId повертає IReadOnlyList<Favorite>:
            var favoriteRecords = await _favoritesQueries.GetByListingId(new ListingId(listingId), cancellationToken);
            var favoriteRecord = favoriteRecords.FirstOrDefault(); // Беремо перший, оскільки очікуємо один

            if (favoriteRecord == null)
            {
                // Якщо запису Favorite для цього ListingId немає, це означає, що ніхто його не додавав.
                // Можна повернути 404 або порожній FavoriteDto/спеціальний статус.
                // Для консистентності з тим, що "ресурс Favorite для цього ListingId не знайдено":
                return NotFound(new { Message = $"No favorite information found for listing {listingId}." });
            }

            return Ok(FavoriteDto.FromDomainModel(favoriteRecord));
        }

        // GET: api/favorites/{favoriteId} (Отримати конкретний запис Favorite за його ID)
        // Цей ендпоінт може бути менш корисним для клієнта, але може бути для адмін. цілей.
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