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
    public record CreateFavoriteCommand : IRequest<Result<Favorite, FavoriteException>>
    {
        public required Guid ListingId { get; init; }

        public required Guid UserId { get; init; }
    }

    public class CreateFavoriteCommandHandler(
        IFavoritesRepository favoritesRepository,
        IListingsRepository listingsRepository,
        IUsersRepository usersRepository
        )
        : IRequestHandler<CreateFavoriteCommand, Result<Favorite, FavoriteException>>
    {
        public async Task<Result<Favorite, FavoriteException>> Handle(CreateFavoriteCommand request, CancellationToken cancellationToken)
        {
            var listingId = new ListingId(request.ListingId);
            var userId = new UserId(request.UserId);

            var listingOption = await listingsRepository.GetById(listingId, cancellationToken);
            return await listingOption.Match<Task<Result<Favorite, FavoriteException>>>(
                some: async listing => 
                {
                    var userOption = await usersRepository.GetById(userId, cancellationToken);

                    return await userOption.Match<Task<Result<Favorite, FavoriteException>>>(
                        some: async userToAdd => 
                        {
                            var favoriteOption = await favoritesRepository.GetByListingIdAsync(listingId, cancellationToken);

                            return await favoriteOption.Match<Task<Result<Favorite, FavoriteException>>>(
                                some: async favorite => 
                                {
                                    if (favorite.Users != null && favorite.Users.Any(u => u.Id == userId))
                                    {
                                        return new UserAlreadyFavoritedListingException(userId, listingId);
                                    }

                                    try
                                    {
                                        favorite.AddUser(userToAdd);
                                        var updatedFavorite = await favoritesRepository.Update(favorite, cancellationToken);
                                        return updatedFavorite; 
                                    }
                                    catch (Exception ex)
                                    {
                                        return new FavoriteOperationFailedException(favorite.Id, "UpdateFavoriteWithUser", ex);
                                    }
                                },
                                none: async () => 
                                {
                                    var favoriteId = FavoriteId.New();
                                    try
                                    {
                                        var newFavorite = Favorite.New(favoriteId, listingId);
                                        newFavorite.AddUser(userToAdd); 
                                        var addedFavorite = await favoritesRepository.Add(newFavorite, cancellationToken);
                                        return addedFavorite; 
                                    }
                                    catch (Exception ex)
                                    {
                                        return new FavoriteOperationFailedException(favoriteId, "CreateFavoriteAndAddUser", ex);
                                    }
                                }
                            );
                        },
                        none: () => 
                        {
                             FavoriteException exception = new UserNotFoundForFavoriteException(userId);
                             return Task.FromResult<Result<Favorite, FavoriteException>>(exception);
                        }
                    );
                },
                 none: () => 
                 {
                     FavoriteException exception = new ListingNotFoundForFavoriteException(listingId);
                     return Task.FromResult<Result<Favorite, FavoriteException>>(exception);
                 }
            );
        }
    }
}