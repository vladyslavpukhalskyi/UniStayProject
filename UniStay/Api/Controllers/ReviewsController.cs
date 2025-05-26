using Microsoft.AspNetCore.Mvc;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Api.Dtos;
using Application.Reviews.Commands;
using Application.Reviews.Exceptions;
using Application.Common.Interfaces.Queries;
using Domain.Reviews;
using Domain.Listings;
using Domain.Users;
using Api.Modules.Errors;
using Microsoft.AspNetCore.Authorization;
using Optional;

namespace Api.Controllers
{
    [Route("api")]
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

        [HttpGet("reviews/{reviewId:guid}", Name = "GetReviewById")]
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

        [HttpGet("listings/{listingId:guid}/reviews")]
        [ProducesResponseType(typeof(IReadOnlyList<ReviewDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ReviewDto>>> GetReviewsForListing([FromRoute] Guid listingId, CancellationToken cancellationToken)
        {
            var reviews = await _reviewsQueries.GetAllReviewsForListing(new ListingId(listingId), cancellationToken);
            var reviewDtos = reviews.Select(ReviewDto.FromDomainModel).ToList();
            return Ok(reviewDtos);
        }

        [HttpGet("users/{userId:guid}/reviews")]
        [ProducesResponseType(typeof(IReadOnlyList<ReviewDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ReviewDto>>> GetReviewsByUser([FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            var reviews = await _reviewsQueries.GetAllReviewsByUser(new UserId(userId), cancellationToken);
            var reviewDtos = reviews.Select(ReviewDto.FromDomainModel).ToList();
            return Ok(reviewDtos);
        }

        [HttpPost("listings/{listingId:guid}/reviews")]
        [Authorize]
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateReviewForListing(
            [FromRoute] Guid listingId,
            [FromBody] CreateReviewDto requestDto,
            CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid authenticatedUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

            var command = new CreateReviewCommand
            {
                ListingId = listingId,
                Rating = requestDto.Rating,
                Comment = requestDto.Comment,
                UserId = authenticatedUserId
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                createdReview => CreatedAtRoute("GetReviewById", new { reviewId = createdReview.Id.Value }, ReviewDto.FromDomainModel(createdReview)),
                reviewException => reviewException.ToObjectResult()
            );
        }

        [HttpPut("reviews/{reviewId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateReview(
            [FromRoute] Guid reviewId,
            [FromBody] UpdateReviewDto requestDto,
            CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid authenticatedUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

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

        [HttpDelete("reviews/{reviewId:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReview(
            [FromRoute] Guid reviewId,
            CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid authenticatedUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

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