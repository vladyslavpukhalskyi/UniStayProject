using Microsoft.AspNetCore.Mvc;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Dtos;
using Application.Users.Commands;
using Application.Users.Exceptions;
using Application.Common.Interfaces.Queries;
using Domain.Users;
using Api.Modules.Errors;
using Optional;
using Application.Auth.Dto; // <<<< ДОДАНО: Для AuthResultDto
using Microsoft.AspNetCore.Authorization; // <<<< ДОДАНО: Для атрибута [AllowAnonymous]

namespace Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IUsersQueries _usersQueries;

        public UsersController(ISender sender, IUsersQueries usersQueries)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _usersQueries = usersQueries ?? throw new ArgumentNullException(nameof(usersQueries));
        }

        // GET: api/users
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
        // [Authorize] // Можливо, ви захочете захистити цей ендпоінт
        public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAllUsers(CancellationToken cancellationToken)
        {
            var users = await _usersQueries.GetAll(cancellationToken);
            var userDtos = users.Select(UserDto.FromDomainModel).ToList();
            return Ok(userDtos);
        }

        // GET: api/users/{userId}
        [HttpGet("{userId:guid}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [Authorize] // Можливо, ви захочете захистити цей ендпоінт
        public async Task<ActionResult<UserDto>> GetUserById([FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            var userOption = await _usersQueries.GetById(new UserId(userId), cancellationToken);

            return userOption.Match<ActionResult<UserDto>>(
                user => Ok(UserDto.FromDomainModel(user)),
                () => NotFound(new { Message = $"User with id {userId} not found." })
            );
        }

        // POST: api/users (Реєстрація нового користувача)
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [AllowAnonymous] // <<<< ДОДАНО: Щоб дозволити неавторизованим користувачам реєструватися
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto requestDto, CancellationToken cancellationToken)
        {
            var command = new CreateUserCommand
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
                createdUser => CreatedAtAction(nameof(GetUserById), new { userId = createdUser.Id.Value }, UserDto.FromDomainModel(createdUser)),
                userException => userException.ToObjectResult()
            );
        }

        // ДОДАНО: Ендпоінт для входу (логіну)
        [HttpPost("login")] // POST api/users/login
        [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Зазвичай 401 для невірних облікових даних
        [AllowAnonymous] // <<<< ДОДАНО: Щоб дозволити неавторизованим користувачам входити
        public async Task<IActionResult> LoginUser([FromBody] LoginUserDtos requestDto, CancellationToken cancellationToken)
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

        // PUT: api/users/{userId}
        [HttpPut("{userId:guid}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize] // <<<< ДОДАНО: Захищаємо цей ендпоінт, бо він змінює дані користувача
        public async Task<IActionResult> UpdateUser([FromRoute] Guid userId, [FromBody] UpdateUserDto requestDto, CancellationToken cancellationToken)
        {
            var command = new UpdateUserCommand
            {
                UserId = userId,
                FirstName = requestDto.FirstName,
                LastName = requestDto.LastName,
                PhoneNumber = requestDto.PhoneNumber,
                ProfileImage = requestDto.ProfileImage
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                updatedUser => Ok(UserDto.FromDomainModel(updatedUser)),
                userException => userException.ToObjectResult()
            );
        }

        // DELETE: api/users/{userId}
        [HttpDelete("{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize] // <<<< ДОДАНО: Захищаємо цей ендпоінт
        public async Task<IActionResult> DeleteUser([FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            var command = new DeleteUserCommand
            {
                UserId = userId
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                deletedUser => NoContent(),
                userException => userException.ToObjectResult()
            );
        }
    }
}