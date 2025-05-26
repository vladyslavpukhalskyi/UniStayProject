using Application.Common; 
using Application.Common.Interfaces.Repositories; 
using Application.Favorites.Exceptions; 
using Domain.Favorites; 
using Domain.Listings; 
using Domain.Users;   
using MediatR;
using Optional; 
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Favorites.Commands
{
    public record DeleteFavoriteCommand : IRequest<Result<Unit, FavoriteException>>
    {
        public required Guid ListingId { get; init; }

        public required Guid UserId { get; init; }
    }

    public class DeleteFavoriteCommandHandler(
        IFavoritesRepository favoritesRepository,
        IUsersRepository usersRepository
        )
        : IRequestHandler<DeleteFavoriteCommand, Result<Unit, FavoriteException>>
    {
        public async Task<Result<Unit, FavoriteException>> Handle(DeleteFavoriteCommand request, CancellationToken cancellationToken)
        {
            var listingId = new ListingId(request.ListingId);
            var userId = new UserId(request.UserId);

            var userOption = await usersRepository.GetById(userId, cancellationToken);

            return await userOption.Match<Task<Result<Unit, FavoriteException>>>(
                some: async userToRemove => 
                {
                    var favoriteOption = await favoritesRepository.GetByListingIdAsync(listingId, cancellationToken);

                    return await favoriteOption.Match<Task<Result<Unit, FavoriteException>>>(
                        some: async favorite => 
                        {
                            var userInList = favorite.Users?.FirstOrDefault(u => u.Id == userId);
                            if (userInList == null)
                            {
                                return new UserHasNotFavoritedListingException(userId, listingId);
                            }

                            try
                            {
                                favorite.RemoveUser(userToRemove);

                                if (favorite.Users == null || !favorite.Users.Any()) 
                                {
                                    await favoritesRepository.Delete(favorite, cancellationToken);
                                }
                                else
                                {
                                    await favoritesRepository.Update(favorite, cancellationToken);
                                }
                                return Unit.Value;
                            }
                            catch (Exception ex)
                            {
                                return new FavoriteOperationFailedException(favorite.Id, "RemoveUserFromFavorite", ex);
                            }
                        },
                        none: () => 
                        {
                            FavoriteException exception = new UserHasNotFavoritedListingException(userId, listingId);
                            return Task.FromResult<Result<Unit, FavoriteException>>(exception);
                        }
                    );
                },
                none: () => 
                {
                    FavoriteException exception = new UserNotFoundForFavoriteException(userId);
                    return Task.FromResult<Result<Unit, FavoriteException>>(exception);
                }
            );
        }
    }
}