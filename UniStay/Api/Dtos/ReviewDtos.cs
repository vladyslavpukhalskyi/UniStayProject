using Domain.Reviews;
using Domain.Users;
using Domain.Listings;
using System;

namespace Api.Dtos
{
    public record UserSummaryDto(
        Guid Id,
        string FullName)
    {
        public static UserSummaryDto FromDomainModel(User user) =>
            new(
                Id: user.Id.Value,
                FullName: user.FullName
            );
    }

    public record ReviewDto(
        Guid Id,
        Guid ListingId,
        Guid UserId,
        UserSummaryDto? User,
        int Rating,
        string Comment,
        DateTime PublicationDate)
    {
        public static ReviewDto FromDomainModel(Review review) =>
            new(
                Id: review.Id.Value,
                ListingId: review.ListingId.Value,
                UserId: review.UserId.Value,
                User: review.User == null ? null : UserSummaryDto.FromDomainModel(review.User),
                Rating: review.Rating,
                Comment: review.Comment,
                PublicationDate: review.PublicationDate
            );
    }

    public record CreateReviewDto(
        Guid ListingId,
        int Rating,
        string Comment
    );

    public record UpdateReviewDto(
        int Rating,
        string Comment
    );
}