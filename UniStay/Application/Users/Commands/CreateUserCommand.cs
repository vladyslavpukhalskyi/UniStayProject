// Файл: Application/Users/Commands/CreateUserCommand.cs
using Application.Common; // Для Result<TSuccess, TError>
using Application.Common.Interfaces.Auth; // <<<< ЗМІНЕНО: Використовуємо IPasswordHash з цього простору імен
using Application.Common.Interfaces.Repositories; // Для IUsersRepository
using Application.Users.Exceptions; // Для UserException та підтипів
using Domain.Users; // Для User, UserId, UserEnums
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;

namespace Application.Users.Commands
{
    /// <summary>
    /// Команда для створення нового користувача.
    /// </summary>
    public record CreateUserCommand : IRequest<Result<User, UserException>>
    {
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string Email { get; init; }
        public required string Password { get; init; } // Пароль у відкритому вигляді
        public string? PhoneNumber { get; init; }
        public string? ProfileImage { get; init; }
    }

    /// <summary>
    /// Обробник команди CreateUserCommand.
    /// </summary>
    // Зверніть увагу на зміну IPasswordHasher на IPasswordHash
    public class CreateUserCommandHandler(
        IUsersRepository usersRepository,
        IPasswordHash passwordHash) // <<<< ЗМІНЕНО: Використовуємо IPasswordHash
        : IRequestHandler<CreateUserCommand, Result<User, UserException>>
    {
        public async Task<Result<User, UserException>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Перевірити, чи користувач з таким email вже існує
            var existingUserOption = await usersRepository.GetByEmail(request.Email, cancellationToken);

            // Використовуємо Match для обробки Optional<User>
            return await existingUserOption.Match<Task<Result<User, UserException>>>(
                some: user => // Якщо користувач знайдений (some)
                {
                    // Повернути помилку UserAlreadyExistsException
                    UserException exception = new UserAlreadyExistsException(request.Email, user.Id);
                    return Task.FromResult<Result<User, UserException>>(exception);
                },
                none: async () => // Якщо користувача не знайдено (none)
                {
                    // Створити нового користувача
                    return await CreateUserEntity(request, cancellationToken);
                }
            );
        }

        private async Task<Result<User, UserException>> CreateUserEntity(
            CreateUserCommand request,
            CancellationToken cancellationToken)
        {
            UserId newUserId = UserId.New(); // Генеруємо новий ID
            try
            {
                // 3. Захешувати пароль
                var hashedPassword = passwordHash.HashPassword(request.Password); // <<<< ЗМІНЕНО: Використовуємо passwordHash

                // 4. Створити сутність User
                var user = User.New(
                    id: newUserId,
                    firstName: request.FirstName,
                    lastName: request.LastName,
                    email: request.Email,
                    password: hashedPassword, // Зберігаємо хеш пароля
                    role: UserEnums.UserRole.User, // За замовчуванням - звичайний користувач
                    phoneNumber: request.PhoneNumber ?? string.Empty,
                    profileImage: request.ProfileImage ?? string.Empty
                );

                // 5. Додати користувача в репозиторій
                var addedUser = await usersRepository.Add(user, cancellationToken);
                // Implicit conversion from User to Result<User, UserException>
                return addedUser;
            }
            catch (Exception exception)
            {
                // 6. Якщо сталася помилка, повернути відповідний виняток
                UserException opFailedException = new UserOperationFailedException(newUserId, "CreateUser", exception);
                return opFailedException;
            }
        }
    }
}