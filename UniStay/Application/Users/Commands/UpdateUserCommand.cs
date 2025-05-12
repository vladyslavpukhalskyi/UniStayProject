// Файл: Application/Users/Commands/UpdateUserCommand.cs
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
    /// Команда для оновлення даних існуючого користувача.
    /// </summary>
    public record UpdateUserCommand : IRequest<Result<User, UserException>>
    {
        /// <summary>
        /// ID користувача, дані якого оновлюються. Зазвичай передається через URL.
        /// </summary>
        public required Guid UserId { get; init; }

        // Поля, які дозволено оновлювати
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public string? PhoneNumber { get; init; }
        public string? ProfileImage { get; init; }
    }

    /// <summary>
    /// Обробник команди UpdateUserCommand.
    /// </summary>
    public class UpdateUserCommandHandler(
        IUsersRepository usersRepository)
        : IRequestHandler<UpdateUserCommand, Result<User, UserException>>
    {
        public async Task<Result<User, UserException>> Handle(
            UpdateUserCommand request,
            CancellationToken cancellationToken)
        {
            var userIdToUpdate = new UserId(request.UserId);

            // 1. Отримати користувача за ID
            var existingUserOption = await usersRepository.GetById(userIdToUpdate, cancellationToken);

            // 2. Обробляємо результат за допомогою Match
            return await existingUserOption.Match<Task<Result<User, UserException>>>(
                some: async user => // Якщо користувач знайдений (some)
                {
                    // Оновити сутність користувача
                    return await UpdateUserEntity(user, request, cancellationToken);
                },
                none: () => // Якщо користувача не знайдено (none)
                {
                    // Повернути помилку UserNotFoundException
                    UserException exception = new UserNotFoundException(userIdToUpdate);
                    return Task.FromResult<Result<User, UserException>>(exception);
                }
            );
        }

        /// <summary>
        /// Приватний метод для фактичного оновлення сутності користувача.
        /// </summary>
        private async Task<Result<User, UserException>> UpdateUserEntity(
            User userToUpdate,
            UpdateUserCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // 3. Оновити деталі користувача за допомогою методу доменної моделі
                userToUpdate.UpdateDetails(
                    request.FirstName,
                    request.LastName,
                    request.PhoneNumber ?? userToUpdate.PhoneNumber, // Залишити старе значення, якщо нове не надано
                    request.ProfileImage ?? userToUpdate.ProfileImage // Залишити старе значення, якщо нове не надано
                );

                // 4. Зберегти оновленого користувача через репозиторій
                // IUsersRepository.Update повертає Task<User>
                var updatedUser = await usersRepository.Update(userToUpdate, cancellationToken);
                // Implicit conversion from User to Result<User, UserException>
                return updatedUser;
            }
            catch (Exception exception)
            {
                // 5. Якщо сталася помилка під час оновлення, повернути відповідний виняток
                UserException opFailedException = new UserOperationFailedException(userToUpdate.Id, "UpdateUser", exception);
                return opFailedException;
            }
        }
    }
}