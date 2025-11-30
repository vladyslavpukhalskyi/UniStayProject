using MediatR;
using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Queries;
using Application.Users.Exceptions;
using Application.Auth.Dto;
using Application.Common;
using Optional.Unsafe;

namespace Application.Auth.Commands
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

            var user = userOption.ValueOrFailure();

            if (!_passwordHash.VerifyPassword(user.Password, request.Password))
            {
                return Result<AuthResultDto, UserException>.Failure(UserException.InvalidCredentials("Invalid email or password."));
            }

            var token = _jwtGenerator.GenerateToken(user.Id, user.Email, user.Role.ToString());
            return Result<AuthResultDto, UserException>.Success(new AuthResultDto { Token = token });
        }
    }
}
