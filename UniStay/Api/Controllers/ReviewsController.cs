using Microsoft.AspNetCore.Mvc;
using MediatR; // Для ISender
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // Для отримання UserId з Claims
using System.Threading;
using System.Threading.Tasks;
using Api.Dtos; // Розташування ваших ReviewDto, CreateReviewDto, UpdateReviewDto
using Application.Reviews.Commands; // Розташування ваших команд для Review
using Application.Reviews.Exceptions; // Для ReviewException
using Application.Common.Interfaces.Queries; // Припускаємо, що IReviewsQueries тут
using Domain.Reviews; // Для ReviewId
using Domain.Listings; // Для ListingId
using Domain.Users;   // Для UserId
using Api.Modules.Errors; // Для ReviewErrorHandler.ToObjectResult()
using Optional;

namespace Api.Controllers
{
    [Route("api")] // Базовий маршрут для гнучкості
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IReviewsQueries _reviewsQueries;

        public ReviewsController(ISender sender, IReviewsQueries reviewsQueries)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _reviewsQueries = reviewsQueries ?? throw new ArgumentNullException(nameof(reviewsQueries));
        }

        // GET: api/reviews/{reviewId}
        [HttpGet("reviews/{reviewId:guid}", Name = "GetReviewById")] // Додано Name для CreatedAtAction
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReviewDto>> GetReviewById([FromRoute] Guid reviewId, CancellationToken cancellationToken)
        {
            var reviewOption = await _reviewsQueries.GetById(new ReviewId(reviewId), cancellationToken);

            return reviewOption.Match<ActionResult<ReviewDto>>(
                review => Ok(ReviewDto.FromDomainModel(review)),
                () => NotFound(new { Message = $"Review with id {reviewId} not found." })
            );
        }

        // GET: api/listings/{listingId}/reviews
        [HttpGet("listings/{listingId:guid}/reviews")]
        [ProducesResponseType(typeof(IReadOnlyList<ReviewDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ReviewDto>>> GetReviewsForListing([FromRoute] Guid listingId, CancellationToken cancellationToken)
        {
            var reviews = await _reviewsQueries.GetAllReviewsForListing(new ListingId(listingId), cancellationToken);
            var reviewDtos = reviews.Select(ReviewDto.FromDomainModel).ToList();
            return Ok(reviewDtos);
        }

        // GET: api/users/{userId}/reviews
        [HttpGet("users/{userId:guid}/reviews")]
        [ProducesResponseType(typeof(IReadOnlyList<ReviewDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ReviewDto>>> GetReviewsByUser([FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            var reviews = await _reviewsQueries.GetAllReviewsByUser(new UserId(userId), cancellationToken);
            var reviewDtos = reviews.Select(ReviewDto.FromDomainModel).ToList();
            return Ok(reviewDtos);
        }

        // POST: api/listings/{listingId}/reviews
        [HttpPost("listings/{listingId:guid}/reviews")]
        // [Authorize] // TODO: Додайте авторизацію
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Якщо оголошення не знайдено
        [ProducesResponseType(StatusCodes.Status409Conflict)] // Якщо користувач не може залишити відгук
        public async Task<IActionResult> CreateReviewForListing(
            [FromRoute] Guid listingId,
            [FromBody] CreateReviewDto requestDto,
            CancellationToken cancellationToken)
        {
            // TODO: Отримати UserId з контексту аутентифікованого користувача
            // var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid authenticatedUserId))
            // {
            //     return Unauthorized(new { Message = "User is not authenticated." });
            // }
            // Для прикладу, тимчасово:
            Guid authenticatedUserId = Guid.NewGuid(); // ЗАМІНІТЬ ЦЕ НА РЕАЛЬНЕ ОТРИМАННЯ ID

            var command = new CreateReviewCommand
            {
                ListingId = listingId,
                Rating = requestDto.Rating,
                Comment = requestDto.Comment,
                UserId = authenticatedUserId // Передаємо ID аутентифікованого користувача
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                createdReview => CreatedAtRoute("GetReviewById", new { reviewId = createdReview.Id.Value }, ReviewDto.FromDomainModel(createdReview)),
                reviewException => reviewException.ToObjectResult()
            );
        }

        // PUT: api/reviews/{reviewId}
        [HttpPut("reviews/{reviewId:guid}")]
        // [Authorize] // TODO: Додайте авторизацію
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateReview(
            [FromRoute] Guid reviewId,
            [FromBody] UpdateReviewDto requestDto,
            CancellationToken cancellationToken)
        {
            // TODO: Отримати RequestingUserId з контексту аутентифікованого користувача
            // var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid authenticatedUserId))
            // {
            //     return Unauthorized(new { Message = "User is not authenticated." });
            // }
            // Для прикладу, тимчасово:
            Guid authenticatedUserId = Guid.NewGuid(); // ЗАМІНІТЬ ЦЕ НА РЕАЛЬНЕ ОТРИМАННЯ ID

            var command = new UpdateReviewCommand
            {
                ReviewId = reviewId,
                Rating = requestDto.Rating,
                Comment = requestDto.Comment,
                RequestingUserId = authenticatedUserId
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                updatedReview => Ok(ReviewDto.FromDomainModel(updatedReview)),
                reviewException => reviewException.ToObjectResult()
            );
        }

        // DELETE: api/reviews/{reviewId}
        [HttpDelete("reviews/{reviewId:guid}")]
        // [Authorize] // TODO: Додайте авторизацію
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReview(
            [FromRoute] Guid reviewId,
            CancellationToken cancellationToken)
        {
            // TODO: Отримати RequestingUserId з контексту аутентифікованого користувача
            // var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid authenticatedUserId))
            // {
            //     return Unauthorized(new { Message = "User is not authenticated." });
            // }
            // Для прикладу, тимчасово:
            Guid authenticatedUserId = Guid.NewGuid(); // ЗАМІНІТЬ ЦЕ НА РЕАЛЬНЕ ОТРИМАННЯ ID

            var command = new DeleteReviewCommand
            {
                ReviewId = reviewId,
                RequestingUserId = authenticatedUserId
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                deletedReview => NoContent(),
                reviewException => reviewException.ToObjectResult()
            );
        }
    }
}