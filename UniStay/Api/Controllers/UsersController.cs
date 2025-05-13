using Microsoft.AspNetCore.Mvc;
using MediatR; // Для ISender
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Dtos; // Розташування ваших UserDto, CreateUserDto, UpdateUserDto
using Application.Users.Commands; // Розташування ваших команд для User
using Application.Users.Exceptions; // Для UserException
using Application.Common.Interfaces.Queries; // Припускаємо, що IUsersQueries тут
using Domain.Users; // Для UserId
using Api.Modules.Errors; // Для UserErrorHandler.ToObjectResult()
using Optional; // Для Option<>

namespace Api.Controllers
{
    [Route("api/users")] // Стандартний префікс api/
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IUsersQueries _usersQueries; // Припускаємо наявність цього інтерфейсу

        public UsersController(ISender sender, IUsersQueries usersQueries)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _usersQueries = usersQueries ?? throw new ArgumentNullException(nameof(usersQueries));
        }

        // GET: api/users
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
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
        public async Task<ActionResult<UserDto>> GetUserById([FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            var userOption = await _usersQueries.GetById(new UserId(userId), cancellationToken);

            return userOption.Match<ActionResult<UserDto>>(
                user => Ok(UserDto.FromDomainModel(user)),
                () => NotFound(new { Message = $"User with id {userId} not found." }) // Повертаємо NotFound з повідомленням
            );
        }

        // POST: api/users
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
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
                // Роль зазвичай встановлюється за замовчуванням в обробнику команди
            };

            var result = await _sender.Send(command, cancellationToken);

            // Використовуємо UserErrorHandler для обробки помилок
            // Для успішного результату повертаємо 201 Created з посиланням на створений ресурс
            return result.Match<IActionResult>(
                createdUser => CreatedAtAction(nameof(GetUserById), new { userId = createdUser.Id.Value }, UserDto.FromDomainModel(createdUser)),
                userException => userException.ToObjectResult() // Використовуємо ваш UserErrorHandler
            );
        }

        // PUT: api/users/{userId}
        [HttpPut("{userId:guid}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser([FromRoute] Guid userId, [FromBody] UpdateUserDto requestDto, CancellationToken cancellationToken)
        {
            var command = new UpdateUserCommand
            {
                UserId = userId, // ID беремо з маршруту
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
        public async Task<IActionResult> DeleteUser([FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            var command = new DeleteUserCommand
            {
                UserId = userId
            };

            var result = await _sender.Send(command, cancellationToken);

            // Для успішного видалення повертаємо 204 NoContent
            // У вашому прикладі Delete повертав ActorDto, але для DELETE зазвичай краще 204 або 200 з об'єктом.
            // Якщо IUsersRepository.Delete повертає User, то ваш DeleteUserCommand поверне Result<User, UserException>
            return result.Match<IActionResult>(
                deletedUser => NoContent(), // Або Ok(UserDto.FromDomainModel(deletedUser)) якщо хочете повернути дані
                userException => userException.ToObjectResult()
            );
        }
    }
}