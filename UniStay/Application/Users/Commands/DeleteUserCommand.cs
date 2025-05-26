using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.Users.Exceptions;
using Domain.Users;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Users.Commands
{
    public record DeleteUserCommand : IRequest<Result<User, UserException>>
    {
        public required Guid UserId { get; init; }
    }

    public class DeleteUserCommandHandler(
        IUsersRepository usersRepository)
        : IRequestHandler<DeleteUserCommand, Result<User, UserException>>
    {
        public async Task<Result<User, UserException>> Handle(
            DeleteUserCommand request,
            CancellationToken cancellationToken)
        {
            var userIdToDelete = new UserId(request.UserId);

            var existingUserOption = await usersRepository.GetById(userIdToDelete, cancellationToken);

            return await existingUserOption.Match<Task<Result<User, UserException>>>(
                some: async user =>
                {
                    return await DeleteUserEntity(user, cancellationToken);
                },
                none: () =>
                {
                    UserException exception = new UserNotFoundException(userIdToDelete);
                    return Task.FromResult<Result<User, UserException>>(exception);
                }
            );
        }

        private async Task<Result<User, UserException>> DeleteUserEntity(User user, CancellationToken cancellationToken)
        {
            try
            {
                var deletedUser = await usersRepository.Delete(user, cancellationToken);
                return deletedUser;
            }
            catch (Exception exception)
            {
                UserException opFailedException = new UserOperationFailedException(user.Id, "DeleteUser", exception);
                return opFailedException;
            }
        }
    }
}