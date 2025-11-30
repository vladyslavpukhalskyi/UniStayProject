using MediatR;
using Application.Users.Exceptions;
using Application.Auth.Dto;
using Application.Common;

namespace Application.Users.Commands
{
    public record LoginUserCommand : IRequest<Result<AuthResultDto, UserException>>
    {
        public required string Email { get; init; }
        public required string Password { get; init; }
    }
}