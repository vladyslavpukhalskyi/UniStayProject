// Файл: Application/Favorites/Commands/DeleteFavoriteCommand.cs
using Application.Common; // Для Result
using Application.Common.Interfaces.Repositories; // Для IFavoritesRepository, IUsersRepository
using Application.Favorites.Exceptions; // Для FavoriteException та підтипів
using Domain.Favorites; // Для Favorite, FavoriteId
using Domain.Listings; // Для ListingId
using Domain.Users;   // Для User, UserId
using MediatR;
using Optional; // Для Option<> та Match()
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Favorites.Commands
{
    /// <summary>
    /// Команда для видалення оголошення з обраного користувачем.
    /// </summary>
    public record DeleteFavoriteCommand : IRequest<Result<Unit, FavoriteException>>
    {
        /// <summary>
        /// ID оголошення, яке видаляється з обраного.
        /// </summary>
        public required Guid ListingId { get; init; }

        /// <summary>
        /// ID користувача, який видаляє з обраного. Встановлюється з контексту аутентифікації.
        /// </summary>
        public required Guid UserId { get; init; }
    }

    /// <summary>
    /// Обробник команди DeleteFavoriteCommand.
    /// </summary>
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

            // 1. Отримати користувача, який виконує дію
            var userOption = await usersRepository.GetById(userId, cancellationToken);

            // Використовуємо Match для userOption
            return await userOption.Match<Task<Result<Unit, FavoriteException>>>(
                some: async userToRemove => // Користувач знайдено, userToRemove - це розпакований User
                {
                    // 2. Знайти запис Favorite для цього ListingId (має включати Users)
                    var favoriteOption = await favoritesRepository.GetByListingIdAsync(listingId, cancellationToken);

                    // Вкладений Match для favoriteOption
                    return await favoriteOption.Match<Task<Result<Unit, FavoriteException>>>(
                        some: async favorite => // Запис Favorite для цього Listing існує
                        {
                            // 3. Перевірити, чи користувач дійсно є в списку Users цього Favorite
                            // (Припускаємо, що Users завантажено через GetByListingIdAsync)
                            var userInList = favorite.Users?.FirstOrDefault(u => u.Id == userId);
                            if (userInList == null)
                            {
                                return new UserHasNotFavoritedListingException(userId, listingId);
                            }

                            // 4. Видалити користувача зі списку
                            try
                            {
                                // Використовуємо userToRemove, отриманий з зовнішнього Match
                                favorite.RemoveUser(userToRemove);

                                // 5. Перевірити, чи список Users тепер порожній
                                if (favorite.Users == null || !favorite.Users.Any()) // Додано перевірку на null
                                {
                                    // Якщо список порожній, видалити сам запис Favorite
                                    await favoritesRepository.Delete(favorite, cancellationToken);
                                }
                                else
                                {
                                    // Якщо в списку ще є користувачі, оновити запис Favorite
                                    await favoritesRepository.Update(favorite, cancellationToken);
                                }
                                // Повернути успіх
                                return Unit.Value;
                            }
                            catch (Exception ex)
                            {
                                return new FavoriteOperationFailedException(favorite.Id, "RemoveUserFromFavorite", ex);
                            }
                        },
                        none: () => // Запису Favorite для цього Listing не існує
                        {
                            FavoriteException exception = new UserHasNotFavoritedListingException(userId, listingId);
                            return Task.FromResult<Result<Unit, FavoriteException>>(exception);
                        }
                    );
                },
                none: () => // Користувача, що виконує дію, не знайдено
                {
                    FavoriteException exception = new UserNotFoundForFavoriteException(userId);
                    return Task.FromResult<Result<Unit, FavoriteException>>(exception);
                }
            );
        }
    }
}