using Application.Listings.Exceptions;
using Domain.Listings;
using MediatR;
using Optional;

namespace Application.Listings.Queries
{
    public class CompareListingsQuery : IRequest<Option<ListingComparisonResult, ListingException>>
    {
        public Guid Listing1Id { get; set; }
        public Guid Listing2Id { get; set; }
    }

    public class ListingComparisonResult
    {
        public Listing Listing1 { get; set; } = null!;
        public Listing Listing2 { get; set; } = null!;
    }
}
