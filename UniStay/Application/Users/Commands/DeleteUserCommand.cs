// Файл: Application/Users/Commands/DeleteUserCommand.cs
using Application.Common; // Для Result<TSuccess, TError>
using Application.Common.Interfaces.Repositories; // Для IUsersRepository
using Application.Users.Exceptions; // Для UserException, UserNotFoundException, UserOperationFailedException
using Domain.Users; // Для User, UserId
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Users.Commands
{
    /// <summary>
    /// Команда для видалення користувача.
    /// </summary>
    public record DeleteUserCommand : IRequest<Result<User, UserException>>
    {
        /// <summary>
        /// ID користувача, якого потрібно видалити.
        /// </summary>
        public required Guid UserId { get; init; }
    }

    /// <summary>
    /// Обробник команди DeleteUserCommand.
    /// </summary>
    public class DeleteUserCommandHandler(
        IUsersRepository usersRepository)
        : IRequestHandler<DeleteUserCommand, Result<User, UserException>>
    {
        public async Task<Result<User, UserException>> Handle(
            DeleteUserCommand request,
            CancellationToken cancellationToken)
        {
            // Створюємо доменний UserId з Guid, отриманого в команді
            var userIdToDelete = new UserId(request.UserId);

            // 1. Отримати користувача за ID
            var existingUserOption = await usersRepository.GetById(userIdToDelete, cancellationToken);

            // 2. Обробляємо результат за допомогою Match
            return await existingUserOption.Match<Task<Result<User, UserException>>>(
                some: async user => // Якщо користувач знайдений (some)
                {
                    // Видалити сутність користувача
                    return await DeleteUserEntity(user, cancellationToken);
                },
                none: () => // Якщо користувача не знайдено (none)
                {
                    // Повернути помилку UserNotFoundException
                    UserException exception = new UserNotFoundException(userIdToDelete);
                    return Task.FromResult<Result<User, UserException>>(exception);
                }
            );
        }

        /// <summary>
        /// Приватний метод для фактичного видалення сутності користувача.
        /// </summary>
        private async Task<Result<User, UserException>> DeleteUserEntity(User user, CancellationToken cancellationToken)
        {
            try
            {
                // 3. Видалити користувача через репозиторій
                // IUsersRepository.Delete повертає Task<User>
                var deletedUser = await usersRepository.Delete(user, cancellationToken);
                // Implicit conversion from User to Result<User, UserException>
                return deletedUser;
            }
            catch (Exception exception)
            {
                // 4. Якщо сталася помилка під час видалення, повернути відповідний виняток
                UserException opFailedException = new UserOperationFailedException(user.Id, "DeleteUser", exception);
                return opFailedException;
            }
        }
    }
}