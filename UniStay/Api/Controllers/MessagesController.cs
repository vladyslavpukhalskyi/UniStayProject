using Microsoft.AspNetCore.Mvc;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Api.Dtos;
using Application.Messages.Commands;
using Application.Messages.Exceptions;
using Application.Common.Interfaces.Queries;
using Domain.Messages;
using Domain.Users;
using Api.Modules.Errors;
using Optional;

namespace Api.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IMessagesQueries _messagesQueries;

        public MessagesController(ISender sender, IMessagesQueries messagesQueries)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _messagesQueries = messagesQueries ?? throw new ArgumentNullException(nameof(messagesQueries));
        }

        [HttpPost]
        [ProducesResponseType(typeof(MessageDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SendMessage([FromBody] CreateMessageDto requestDto, CancellationToken cancellationToken)
        {
            var senderIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(senderIdString) || !Guid.TryParse(senderIdString, out Guid authenticatedSenderId))
            {
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

            var command = new CreateMessageCommand
            {
                ReceiverId = requestDto.ReceiverId,
                Text = requestDto.Text,
                SenderId = authenticatedSenderId
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                createdMessage => CreatedAtRoute("GetMessageById", new { messageId = createdMessage.Id.Value }, MessageDto.FromDomainModel(createdMessage)),
                messageException => messageException.ToObjectResult()
            );
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<MessageDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMyMessages(CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid currentUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

            var messages = await _messagesQueries.GetAllMessagesForUser(new UserId(currentUserId), cancellationToken);
            var messageDtos = messages.Select(MessageDto.FromDomainModel).ToList();
            return Ok(messageDtos);
        }

        [HttpGet("conversation/{otherUserId:guid}")]
        [ProducesResponseType(typeof(IReadOnlyList<MessageDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetConversationWithUser([FromRoute] Guid otherUserId, CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
             if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid currentUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated or user ID is invalid." });
            }

            var messages = await _messagesQueries.GetConversation(new UserId(currentUserId), new UserId(otherUserId), cancellationToken);
            var messageDtos = messages.Select(MessageDto.FromDomainModel).ToList();
            return Ok(messageDtos);
        }

        [HttpGet("{messageId:guid}", Name = "GetMessageById")]
        [ProducesResponseType(typeof(MessageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MessageDto>> GetMessageById([FromRoute] Guid messageId, CancellationToken cancellationToken)
        {
            var messageOption = await _messagesQueries.GetById(new MessageId(messageId), cancellationToken);

            return messageOption.Match<ActionResult<MessageDto>>(
                message =>
                {
                    return Ok(MessageDto.FromDomainModel(message));
                },
                () => NotFound(new { Message = $"Message with id {messageId} not found." })
            );
        }

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

        [HttpDelete("{messageId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMessage(
            [FromRoute] Guid messageId,
            CancellationToken cancellationToken)
        {
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