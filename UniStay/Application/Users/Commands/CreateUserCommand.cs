using Application.Common; 
using Application.Common.Interfaces.Auth; 
using Application.Common.Interfaces.Repositories; 
using Application.Users.Exceptions; 
using Domain.Users; 
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;

namespace Application.Users.Commands
{
    public record CreateUserCommand : IRequest<Result<User, UserException>>
    {
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string Email { get; init; }
        public required string Password { get; init; } 
        public string? PhoneNumber { get; init; }
        public string? ProfileImage { get; init; }
    }

    public class CreateUserCommandHandler(
        IUsersRepository usersRepository,
        IPasswordHash passwordHash) 
        : IRequestHandler<CreateUserCommand, Result<User, UserException>>
    {
        public async Task<Result<User, UserException>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var existingUserOption = await usersRepository.GetByEmail(request.Email, cancellationToken);

            return await existingUserOption.Match<Task<Result<User, UserException>>>(
                some: user => 
                {
                    UserException exception = new UserAlreadyExistsException(request.Email, user.Id);
                    return Task.FromResult<Result<User, UserException>>(exception);
                },
                none: async () => 
                {
                    return await CreateUserEntity(request, cancellationToken);
                }
            );
        }

        private async Task<Result<User, UserException>> CreateUserEntity(
            CreateUserCommand request,
            CancellationToken cancellationToken)
        {
            UserId newUserId = UserId.New(); 
            try
            {
                var hashedPassword = passwordHash.HashPassword(request.Password); 

                var user = User.New(
                    id: newUserId,
                    firstName: request.FirstName,
                    lastName: request.LastName,
                    email: request.Email,
                    password: hashedPassword, 
                    role: UserEnums.UserRole.User, 
                    phoneNumber: request.PhoneNumber ?? string.Empty,
                    profileImage: request.ProfileImage ?? string.Empty
                );

                var addedUser = await usersRepository.Add(user, cancellationToken);
                return addedUser;
            }
            catch (Exception exception)
            {
                UserException opFailedException = new UserOperationFailedException(newUserId, "CreateUser", exception);
                return opFailedException;
            }
        }
    }
}