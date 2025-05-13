using Microsoft.AspNetCore.Mvc;
using MediatR; // Для ISender
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // Для отримання UserId з Claims
using System.Threading;
using System.Threading.Tasks;
using Api.Dtos; // Розташування ваших MessageDto, CreateMessageDto, UpdateMessageDto
using Application.Messages.Commands; // Розташування ваших команд для Message
using Application.Messages.Exceptions; // Для MessageException
using Application.Common.Interfaces.Queries; // Припускаємо, що IMessagesQueries тут
using Domain.Messages; // Для MessageId
using Domain.Users;   // Для UserId
using Api.Modules.Errors; // Для MessageErrorHandler.ToObjectResult()
using Optional;

namespace Api.Controllers
{
    [Route("api/messages")]
    [ApiController]
    // [Authorize] // TODO: Загальна авторизація для всього контролера, якщо потрібно
    public class MessagesController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IMessagesQueries _messagesQueries;

        public MessagesController(ISender sender, IMessagesQueries messagesQueries)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _messagesQueries = messagesQueries ?? throw new ArgumentNullException(nameof(messagesQueries));
        }

        // POST: api/messages (Відправити нове повідомлення)
        // [Authorize] // TODO: Додайте авторизацію
        [HttpPost]
        [ProducesResponseType(typeof(MessageDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Якщо отримувач не знайдений
        public async Task<IActionResult> SendMessage([FromBody] CreateMessageDto requestDto, CancellationToken cancellationToken)
        {
            // TODO: Отримати SenderId з контексту аутентифікованого користувача
            var senderIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(senderIdString) || !Guid.TryParse(senderIdString, out Guid authenticatedSenderId))
            {
                // Зазвичай, якщо [Authorize] увімкнено, цей випадок не має статися,
                // але додаткова перевірка не завадить, або повертати 401/403 через політику.
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

            var command = new CreateMessageCommand
            {
                ReceiverId = requestDto.ReceiverId,
                Text = requestDto.Text,
                SenderId = authenticatedSenderId // ID відправника береться з аутентифікації
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                createdMessage => CreatedAtRoute("GetMessageById", new { messageId = createdMessage.Id.Value }, MessageDto.FromDomainModel(createdMessage)),
                messageException => messageException.ToObjectResult()
            );
        }

        // GET: api/messages (Отримати всі повідомлення для поточного користувача)
        // [Authorize] // TODO: Додайте авторизацію
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<MessageDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMyMessages(CancellationToken cancellationToken)
        {
            // TODO: Отримати currentUserId з контексту аутентифікованого користувача
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid currentUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

            var messages = await _messagesQueries.GetAllMessagesForUser(new UserId(currentUserId), cancellationToken);
            var messageDtos = messages.Select(MessageDto.FromDomainModel).ToList();
            return Ok(messageDtos);
        }

        // GET: api/messages/conversation/{otherUserId} (Отримати розмову поточного користувача з іншим користувачем)
        // [Authorize] // TODO: Додайте авторизацію
        [HttpGet("conversation/{otherUserId:guid}")]
        [ProducesResponseType(typeof(IReadOnlyList<MessageDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetConversationWithUser([FromRoute] Guid otherUserId, CancellationToken cancellationToken)
        {
            // TODO: Отримати currentUserId з контексту аутентифікованого користувача
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
             if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid currentUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

            var messages = await _messagesQueries.GetConversation(new UserId(currentUserId), new UserId(otherUserId), cancellationToken);
            var messageDtos = messages.Select(MessageDto.FromDomainModel).ToList();
            return Ok(messageDtos);
        }

        // GET: api/messages/{messageId}
        // [Authorize] // TODO: Додайте авторизацію (перевірка, чи користувач є учасником повідомлення)
        [HttpGet("{messageId:guid}", Name = "GetMessageById")]
        [ProducesResponseType(typeof(MessageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MessageDto>> GetMessageById([FromRoute] Guid messageId, CancellationToken cancellationToken)
        {
            // TODO: Отримати currentUserId з контексту аутентифікованого користувача
            // var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid currentUserId))
            // {
            // return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            // }

            var messageOption = await _messagesQueries.GetById(new MessageId(messageId), cancellationToken);

            return messageOption.Match<ActionResult<MessageDto>>(
                message =>
                {
                    // TODO: Додаткова перевірка: чи є поточний користувач відправником або отримувачем
                    // if (message.SenderId.Value != currentUserId && message.ReceiverId.Value != currentUserId)
                    // {
                    //     return Forbid(); // Або NotFound(), щоб не розкривати існування повідомлення
                    // }
                    return Ok(MessageDto.FromDomainModel(message));
                },
                () => NotFound(new { Message = $"Message with id {messageId} not found." })
            );
        }

        // PUT: api/messages/{messageId} (Оновити текст повідомлення)
        // [Authorize] // TODO: Додайте авторизацію
        [HttpPut("{messageId:guid}")]
        [ProducesResponseType(typeof(MessageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMessage(
            [FromRoute] Guid messageId,
            [FromBody] UpdateMessageDto requestDto,
            CancellationToken cancellationToken)
        {
            // TODO: Отримати RequestingUserId з контексту аутентифікованого користувача
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid authenticatedUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

            var command = new UpdateMessageCommand
            {
                MessageId = messageId,
                Text = requestDto.Text,
                RequestingUserId = authenticatedUserId
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                updatedMessage => Ok(MessageDto.FromDomainModel(updatedMessage)),
                messageException => messageException.ToObjectResult()
            );
        }

        // DELETE: api/messages/{messageId} (Видалити повідомлення)
        // [Authorize] // TODO: Додайте авторизацію
        [HttpDelete("{messageId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMessage(
            [FromRoute] Guid messageId,
            CancellationToken cancellationToken)
        {
            // TODO: Отримати RequestingUserId з контексту аутентифікованого користувача
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid authenticatedUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

            var command = new DeleteMessageCommand
            {
                MessageId = messageId,
                RequestingUserId = authenticatedUserId
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                deletedMessage => NoContent(),
                messageException => messageException.ToObjectResult()
            );
        }
    }
}