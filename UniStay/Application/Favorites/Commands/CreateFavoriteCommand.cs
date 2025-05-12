// Файл: Application/Favorites/Commands/CreateFavoriteCommand.cs
using Application.Common; // Для Result
using Application.Common.Interfaces.Repositories; // Для IFavoritesRepository, IListingsRepository, IUsersRepository
using Application.Favorites.Exceptions; // Для FavoriteException та підтипів
using Domain.Favorites; // Для Favorite, FavoriteId
using Domain.Listings; // Для ListingId
using Domain.Users;   // Для User, UserId
using MediatR;
using Optional; // Потрібен для Option<> та Match()
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Application.Favorites.Commands
{
    /// <summary>
    /// Команда для додавання оголошення в обране користувачем.
    /// </summary>
    public record CreateFavoriteCommand : IRequest<Result<Favorite, FavoriteException>>
    {
        /// <summary>
        /// ID оголошення, яке додається в обране.
        /// </summary>
        public required Guid ListingId { get; init; }

        /// <summary>
        /// ID користувача, який додає в обране. Встановлюється з контексту аутентифікації.
        /// </summary>
        public required Guid UserId { get; init; }
    }

    /// <summary>
    /// Обробник команди CreateFavoriteCommand.
    /// </summary>
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

            // 1. Перевірити, чи існує оголошення
            var listingOption = await listingsRepository.GetById(listingId, cancellationToken);
            // Використовуємо Match для перевірки Listing
            return await listingOption.Match<Task<Result<Favorite, FavoriteException>>>(
                some: async listing => // Оголошення знайдено
                {
                    // 2. Перевірити, чи існує користувач
                    var userOption = await usersRepository.GetById(userId, cancellationToken);

                    // Використовуємо Match для перевірки User
                    return await userOption.Match<Task<Result<Favorite, FavoriteException>>>(
                        some: async userToAdd => // Користувач знайдено
                        {
                             // 3. Знайти існуючий запис Favorite для цього ListingId
                            var favoriteOption = await favoritesRepository.GetByListingIdAsync(listingId, cancellationToken);

                            // Використовуємо Match для обробки favoriteOption
                            return await favoriteOption.Match<Task<Result<Favorite, FavoriteException>>>(
                                some: async favorite => // Запис Favorite для цього Listing вже існує
                                {
                                    // 4a. Перевірити, чи користувач вже є в списку Users цього Favorite
                                    // (Припускаємо, що Users завантажено через GetByListingIdAsync)
                                    if (favorite.Users != null && favorite.Users.Any(u => u.Id == userId))
                                    {
                                        return new UserAlreadyFavoritedListingException(userId, listingId);
                                    }

                                    // 4b. Додати користувача до списку і оновити запис Favorite
                                    try
                                    {
                                        favorite.AddUser(userToAdd);
                                        var updatedFavorite = await favoritesRepository.Update(favorite, cancellationToken);
                                        return updatedFavorite; // Implicit conversion
                                    }
                                    catch (Exception ex)
                                    {
                                        return new FavoriteOperationFailedException(favorite.Id, "UpdateFavoriteWithUser", ex);
                                    }
                                },
                                none: async () => // Запису Favorite для цього Listing ще немає
                                {
                                    // 4c. Створити новий запис Favorite і додати користувача
                                    var favoriteId = FavoriteId.New();
                                    try
                                    {
                                        var newFavorite = Favorite.New(favoriteId, listingId);
                                        newFavorite.AddUser(userToAdd); // Додаємо користувача
                                        var addedFavorite = await favoritesRepository.Add(newFavorite, cancellationToken);
                                        return addedFavorite; // Implicit conversion
                                    }
                                    catch (Exception ex)
                                    {
                                        return new FavoriteOperationFailedException(favoriteId, "CreateFavoriteAndAddUser", ex);
                                    }
                                }
                            );
                        },
                        none: () => // Користувача не знайдено
                        {
                             FavoriteException exception = new UserNotFoundForFavoriteException(userId);
                             return Task.FromResult<Result<Favorite, FavoriteException>>(exception);
                        }
                    );
                },
                 none: () => // Оголошення не знайдено
                 {
                     FavoriteException exception = new ListingNotFoundForFavoriteException(listingId);
                     return Task.FromResult<Result<Favorite, FavoriteException>>(exception);
                 }
            );
        }
    }
}