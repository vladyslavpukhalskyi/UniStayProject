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
    public record UpdateUserCommand : IRequest<Result<User, UserException>>
    {
        public required Guid UserId { get; init; }

        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public string? PhoneNumber { get; init; }
        public string? ProfileImage { get; init; }
    }

    public class UpdateUserCommandHandler(
        IUsersRepository usersRepository)
        : IRequestHandler<UpdateUserCommand, Result<User, UserException>>
    {
        public async Task<Result<User, UserException>> Handle(
            UpdateUserCommand request,
            CancellationToken cancellationToken)
        {
            var userIdToUpdate = new UserId(request.UserId);

            var existingUserOption = await usersRepository.GetById(userIdToUpdate, cancellationToken);

            return await existingUserOption.Match<Task<Result<User, UserException>>>(
                some: async user =>
                {
                    return await UpdateUserEntity(user, request, cancellationToken);
                },
                none: () =>
                {
                    UserException exception = new UserNotFoundException(userIdToUpdate);
                    return Task.FromResult<Result<User, UserException>>(exception);
                }
            );
        }

        private async Task<Result<User, UserException>> UpdateUserEntity(
            User userToUpdate,
            UpdateUserCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                userToUpdate.UpdateDetails(
                    request.FirstName,
                    request.LastName,
                    request.PhoneNumber ?? userToUpdate.PhoneNumber,
                    request.ProfileImage ?? userToUpdate.ProfileImage
                );

                var updatedUser = await usersRepository.Update(userToUpdate, cancellationToken);
                return updatedUser;
            }
            catch (Exception exception)
            {
                UserException opFailedException = new UserOperationFailedException(userToUpdate.Id, "UpdateUser", exception);
                return opFailedException;
            }
        }
    }
}