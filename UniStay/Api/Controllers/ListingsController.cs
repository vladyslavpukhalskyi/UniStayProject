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
using Application.Listings.Queries;
using Application.Listings.Services;
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
        private readonly IListingComparisonService _comparisonService;

        public ListingsController(ISender sender, IListingsQueries listingsQueries, IListingComparisonService comparisonService)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _listingsQueries = listingsQueries ?? throw new ArgumentNullException(nameof(listingsQueries));
            _comparisonService = comparisonService ?? throw new ArgumentNullException(nameof(comparisonService));
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

            return result.Match<IActionResult>(
                comparisonResult => Ok(BuildComparisonDto(comparisonResult)),
                listingException => listingException.ToObjectResult()
            );
        }

        private ListingComparisonDto BuildComparisonDto(ListingComparisonResult result)
        {
            var listing1 = result.Listing1;
            var listing2 = result.Listing2;

            var priceComp = _comparisonService.ComparePrices(listing1, listing2);
            var locationComp = _comparisonService.CompareLocations(listing1, listing2);
            var typeComp = _comparisonService.CompareTypes(listing1, listing2);
            var communalComp = _comparisonService.CompareCommunalServices(listing1, listing2);
            var ownershipComp = _comparisonService.CompareOwnership(listing1, listing2);
            var neighbourComp = _comparisonService.CompareNeighbours(listing1, listing2);
            var dateComp = _comparisonService.ComparePublicationDates(listing1, listing2);
            var amenitiesComp = _comparisonService.CompareAmenities(listing1, listing2);
            var reviewsComp = _comparisonService.CompareReviews(listing1, listing2);
            var imagesComp = _comparisonService.CompareImages(listing1, listing2);
            var favoritesComp = _comparisonService.CompareFavorites(listing1, listing2);

            return new ListingComparisonDto(
                Listing1: ListingDto.FromDomainModel(listing1),
                Listing2: ListingDto.FromDomainModel(listing2),
                PriceComparison: new PriceComparisonDto(priceComp.Listing1Price, priceComp.Listing2Price, priceComp.Cheaper, priceComp.PriceDifference, priceComp.PriceDifferencePercentage),
                LocationComparison: new LocationComparisonDto(locationComp.Listing1Latitude, locationComp.Listing1Longitude, locationComp.Listing2Latitude, locationComp.Listing2Longitude, locationComp.DirectDistanceKm, locationComp.LandmarkComparisons.Select(l => new LandmarkDistanceComparisonDto(l.LandmarkName, l.LandmarkAddress, l.Listing1DistanceKm, l.Listing2DistanceKm, l.CloserListing, l.DistanceDifferenceKm)).ToList()),
                TypeComparison: new TypeComparisonDto(typeComp.Listing1Type, typeComp.Listing2Type, typeComp.AreSameType),
                CommunalServicesComparison: new CommunalServicesComparisonDto(communalComp.Listing1Services, communalComp.Listing2Services, communalComp.HasIncluded1, communalComp.HasIncluded2, communalComp.HasSeparate1, communalComp.HasSeparate2),
                OwnershipComparison: new OwnershipComparisonDto(ownershipComp.Listing1Ownership, ownershipComp.Listing2Ownership, ownershipComp.AreSame),
                NeighbourComparison: new NeighbourComparisonDto(neighbourComp.Listing1Neighbours, neighbourComp.Listing2Neighbours, neighbourComp.AreSame),
                PublicationDateComparison: new PublicationDateComparisonDto(dateComp.Listing1Date, dateComp.Listing2Date, dateComp.NewerListing, dateComp.DaysDifference),
                AmenitiesComparison: new AmenitiesComparisonDto(amenitiesComp.Listing1Count, amenitiesComp.Listing2Count, amenitiesComp.MoreAmenities, amenitiesComp.CommonAmenities, amenitiesComp.OnlyInListing1, amenitiesComp.OnlyInListing2),
                ReviewsComparison: new ReviewsComparisonDto(reviewsComp.Listing1Count, reviewsComp.Listing2Count, reviewsComp.MoreReviews, reviewsComp.Listing1AverageRating, reviewsComp.Listing2AverageRating, reviewsComp.BetterRated),
                ImagesComparison: new ImagesComparisonDto(imagesComp.Listing1Count, imagesComp.Listing2Count, imagesComp.MoreImages),
                FavoritesComparison: new FavoritesComparisonDto(favoritesComp.Listing1Count, favoritesComp.Listing2Count, favoritesComp.MorePopular)
            );
        }
    }
}