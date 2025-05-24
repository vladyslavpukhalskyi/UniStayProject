using MediatR;
using Application.Users.Exceptions; // Для UserException
using Application.Auth.Dto;
using Application.Common;
// using Application.Common.Models; // <<<< ОНОВЛЕНО: Для Result

namespace Application.Users.Commands
{
    public record LoginUserCommand : IRequest<Result<AuthResultDto, UserException>>
    {
        public string Email { get; init; }
        public string Password { get; init; }
    }
}