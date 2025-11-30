using Application.Common.Interfaces.Queries;
using Application.Listings.Exceptions;
using Domain.Listings;
using MediatR;
using Optional;

namespace Application.Listings.Queries
{
    public class CompareListingsQueryHandler : IRequestHandler<CompareListingsQuery, Option<ListingComparisonResult, ListingException>>
    {
        private readonly IListingsQueries _listingsQueries;

        public CompareListingsQueryHandler(IListingsQueries listingsQueries)
        {
            _listingsQueries = listingsQueries ?? throw new ArgumentNullException(nameof(listingsQueries));
        }

        public async Task<Option<ListingComparisonResult, ListingException>> Handle(CompareListingsQuery request, CancellationToken cancellationToken)
        {
            var listing1Id = new ListingId(request.Listing1Id);
            var listing2Id = new ListingId(request.Listing2Id);

            var listing1Option = await _listingsQueries.GetById(listing1Id, cancellationToken);
            var listing2Option = await _listingsQueries.GetById(listing2Id, cancellationToken);

            return listing1Option.Match(
                some: listing1 => listing2Option.Match(
                    some: listing2 => Option.Some<ListingComparisonResult, ListingException>(
                        new ListingComparisonResult
                        {
                            Listing1 = listing1,
                            Listing2 = listing2
                        }
                    ),
                    none: () => Option.None<ListingComparisonResult, ListingException>(
                        new ListingNotFoundException(listing2Id)
                    )
                ),
                none: () => Option.None<ListingComparisonResult, ListingException>(
                    new ListingNotFoundException(listing1Id)
                )
            );
        }
    }
}
