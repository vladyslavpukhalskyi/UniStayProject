using Microsoft.AspNetCore.Mvc;
using MediatR;
using Api.Dtos;
using Application.Auth.Dto;
using Application.Auth.Commands;
using Api.Modules.Errors;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISender _sender;

        public AuthController(ISender sender)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] CreateUserDto requestDto, CancellationToken cancellationToken)
        {
            var command = new RegisterUserCommand
            {
                FirstName = requestDto.FirstName,
                LastName = requestDto.LastName,
                Email = requestDto.Email,
                Password = requestDto.Password,
                PhoneNumber = requestDto.PhoneNumber,
                ProfileImage = requestDto.ProfileImage
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                createdUser => Created(string.Empty, new { createdUser.Id }),
                userException => userException.ToObjectResult()
            );
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginUserDtos requestDto, CancellationToken cancellationToken)
        {
            var command = new LoginUserCommand
            {
                Email = requestDto.Email,
                Password = requestDto.Password
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                authResult => Ok(authResult),
                userException => userException.ToObjectResult()
            );
        }
    }
}
