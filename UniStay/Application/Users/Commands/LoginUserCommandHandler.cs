using MediatR;
using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Queries;
using Application.Users.Exceptions;
using Application.Auth.Dto;
using Optional;
using Domain.Users;
using System;
using Application.Common;

namespace Application.Users.Commands
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<AuthResultDto, UserException>>
    {
        private readonly IUsersQueries _usersQueries;
        private readonly IPasswordHash _passwordHash;
        private readonly IJwtGenerator _jwtGenerator;

        public LoginUserCommandHandler(IUsersQueries usersQueries, IPasswordHash passwordHash, IJwtGenerator jwtGenerator)
        {
            _usersQueries = usersQueries;
            _passwordHash = passwordHash;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<Result<AuthResultDto, UserException>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var userOption = await _usersQueries.GetByEmail(request.Email, cancellationToken);

            if (!userOption.HasValue)
            {
                return Result<AuthResultDto, UserException>.Failure(UserException.InvalidCredentials("Invalid email or password."));
            }

            var user = userOption.ValueOr(() => throw new InvalidOperationException("User was expected but not found."));

            if (!_passwordHash.VerifyPassword(user.Password, request.Password))
            {
                return Result<AuthResultDto, UserException>.Failure(UserException.InvalidCredentials("Invalid email or password."));
            }

            var token = _jwtGenerator.GenerateToken(user.Id, user.Email);

            return Result<AuthResultDto, UserException>.Success(new AuthResultDto { Token = token });
        }
    }
}