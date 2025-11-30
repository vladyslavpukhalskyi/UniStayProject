using Microsoft.AspNetCore.Mvc;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Api.Dtos;
using Api.Dtos.Listing;
using Application.ListingImages.Commands;
using Application.ListingImages.Exceptions;
using Application.Common.Interfaces.Queries;
using Domain.ListingImages;
using Domain.Listings;
using Api.Modules.Errors;
using Optional;

namespace Api.Controllers
{
    [Route("api")] 
    [ApiController]
    public class ListingImagesController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IListingImagesQueries _listingImagesQueries;

        public ListingImagesController(ISender sender, IListingImagesQueries listingImagesQueries)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _listingImagesQueries = listingImagesQueries ?? throw new ArgumentNullException(nameof(listingImagesQueries));
        }

        [HttpGet("listingimages/{imageId:guid}", Name = "GetListingImageById")]
        [ProducesResponseType(typeof(ListingImageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ListingImageDto>> GetListingImageById([FromRoute] Guid imageId, CancellationToken cancellationToken)
        {
            var imageOption = await _listingImagesQueries.GetById(new ListingImageId(imageId), cancellationToken);

            return imageOption.Match<ActionResult<ListingImageDto>>(
                image => Ok(ListingImageDto.FromDomainModel(image)),
                () => NotFound(new { Message = $"Listing image with id {imageId} not found." })
            );
        }

        [HttpGet("listings/{listingId:guid}/images")]
        [ProducesResponseType(typeof(IReadOnlyList<ListingImageDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ListingImageDto>>> GetImagesForListing([FromRoute] Guid listingId, CancellationToken cancellationToken)
        {
            var images = await _listingImagesQueries.GetByListingId(new ListingId(listingId), cancellationToken);
            var imageDtos = images.Select(ListingImageDto.FromDomainModel).ToList();
            return Ok(imageDtos);
        }

        [HttpPost("listings/{listingId:guid}/images")]
        [ProducesResponseType(typeof(ListingImageDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddImageToListing(
            [FromRoute] Guid listingId,
            [FromBody] CreateListingImageDto requestDto,
            CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid authenticatedUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

            var command = new CreateListingImageCommand
            {
                ListingId = listingId,
                ImageUrl = requestDto.ImageUrl,
                RequestingUserId = authenticatedUserId
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                createdImage => CreatedAtRoute("GetListingImageById", new { imageId = createdImage.Id.Value }, ListingImageDto.FromDomainModel(createdImage)),
                listingImageException => listingImageException.ToObjectResult()
            );
        }

        [HttpPut("listingimages/{imageId:guid}")]
        [ProducesResponseType(typeof(ListingImageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateListingImage(
            [FromRoute] Guid imageId,
            [FromBody] UpdateListingImageDto requestDto,
            CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid authenticatedUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

            var command = new UpdateListingImageCommand
            {
                ListingImageId = imageId,
                NewImageUrl = requestDto.ImageUrl,
                RequestingUserId = authenticatedUserId
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                updatedImage => Ok(ListingImageDto.FromDomainModel(updatedImage)),
                listingImageException => listingImageException.ToObjectResult()
            );
        }

        [HttpDelete("listingimages/{imageId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteListingImage(
            [FromRoute] Guid imageId,
            CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid authenticatedUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

            var command = new DeleteListingImageCommand
            {
                ListingImageId = imageId,
                RequestingUserId = authenticatedUserId
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                deletedImage => NoContent(),
                listingImageException => listingImageException.ToObjectResult()
            );
        }
        
        [HttpGet("listingimages")]
        [ProducesResponseType(typeof(IReadOnlyList<ListingImageDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ListingImageDto>>> GetAllListingImages(CancellationToken cancellationToken)
        {
            var images = await _listingImagesQueries.GetAll(cancellationToken);
            var imageDtos = images.Select(ListingImageDto.FromDomainModel).ToList();
            return Ok(imageDtos);
        }
    }
}