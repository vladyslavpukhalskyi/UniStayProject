using MediatR;
using Application.Users.Exceptions;
using Application.Auth.Dto;
using Application.Common;

namespace Application.Users.Commands
{
    public record LoginUserCommand : IRequest<Result<AuthResultDto, UserException>>
    {
        public string Email { get; init; }
        public string Password { get; init; }
    }
}