using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using Api.Dtos;
using Application.Chats.Commands;
using Application.Common.Interfaces.Queries;
using Domain.Chats;
using Domain.Users;
using Api.Modules.Errors;

namespace Api.Controllers
{
    [Route("api/chats")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IChatsQueries _chatsQueries;

        public ChatsController(ISender sender, IChatsQueries chatsQueries)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _chatsQueries = chatsQueries ?? throw new ArgumentNullException(nameof(chatsQueries));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ChatDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ChatDto>> CreateChat([FromBody] CreateChatRequest request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var command = new CreateChatCommand
            {
                Name = request.Name,
                Description = request.Description,
                Type = request.Type,
                CreatedById = userId.Value
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<ActionResult<ChatDto>>(
                chat => CreatedAtAction(nameof(GetChatById), new { id = chat.Id.Value }, ChatDto.FromDomainModel(chat)),
                error => error.ToActionResult()
            );
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ChatDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ChatDto>> GetChatById(Guid id, CancellationToken cancellationToken)
        {
            var chatId = new ChatId(id);
            var chatOption = await _chatsQueries.GetById(chatId, cancellationToken);

            return chatOption.Match<ActionResult<ChatDto>>(
                chat => Ok(ChatDto.FromDomainModel(chat)),
                () => NotFound()
            );
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<ChatDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ChatDto>>> GetUserChats(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var userIdDomain = new UserId(userId.Value);
            var chats = await _chatsQueries.GetUserChats(userIdDomain, cancellationToken);
            var chatDtos = chats.Select(ChatDto.FromDomainModel).ToList();
            return Ok(chatDtos);
        }

        

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ChatDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ChatDto>> UpdateChat(Guid id, [FromBody] UpdateChatRequest request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var command = new UpdateChatCommand
            {
                ChatId = id,
                UserId = userId.Value,
                Name = request.Name,
                Description = request.Description
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<ActionResult<ChatDto>>(
                chat => Ok(ChatDto.FromDomainModel(chat)),
                error => error.ToActionResult()
            );
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteChat(Guid id, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var command = new DeleteChatCommand
            {
                ChatId = id,
                UserId = userId.Value
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<ActionResult>(
                _ => NoContent(),
                error => error.ToActionResult()
            );
        }

        [HttpPost("{id}/leave")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> LeaveChat(Guid id, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var command = new LeaveChatCommand
            {
                ChatId = id,
                UserId = userId.Value
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<ActionResult>(
                _ => NoContent(),
                error => error.ToActionResult()
            );
        }

        [HttpGet("{id}/messages")]
        [ProducesResponseType(typeof(IReadOnlyList<ChatMessageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<ChatMessageDto>>> GetChatMessages(
            Guid id, 
            [FromQuery] int skip = 0, 
            [FromQuery] int take = 50, 
            CancellationToken cancellationToken = default)
        {
            var chatId = new ChatId(id);
            var messages = await _chatsQueries.GetChatMessages(chatId, skip, take, cancellationToken);
            var messageDtos = messages.Select(ChatMessageDto.FromDomainModel).ToList();
            return Ok(messageDtos);
        }

        [HttpPost("{id}/messages")]
        [ProducesResponseType(typeof(ChatMessageDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ChatMessageDto>> SendMessage(Guid id, [FromBody] SendMessageRequest request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var command = new SendChatMessageCommand
            {
                ChatId = id,
                SenderId = userId.Value,
                Content = request.Content
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<ActionResult<ChatMessageDto>>(
                message => CreatedAtAction(nameof(GetChatMessages), new { id }, ChatMessageDto.FromDomainModel(message)),
                error => error.ToActionResult()
            );
        }

        [HttpPut("{id}/messages/{messageId}")]
        [ProducesResponseType(typeof(ChatMessageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ChatMessageDto>> UpdateMessage(Guid id, Guid messageId, [FromBody] SendMessageRequest request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var command = new UpdateChatMessageCommand
            {
                ChatId = id,
                MessageId = messageId,
                RequestingUserId = userId.Value,
                Content = request.Content
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<ActionResult<ChatMessageDto>>(
                message => Ok(ChatMessageDto.FromDomainModel(message)),
                error => error.ToActionResult()
            );
        }

        [HttpDelete("{id}/messages/{messageId}")]
        [ProducesResponseType(typeof(ChatMessageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ChatMessageDto>> DeleteMessage(Guid id, Guid messageId, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var command = new DeleteChatMessageCommand
            {
                ChatId = id,
                MessageId = messageId,
                RequestingUserId = userId.Value
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<ActionResult<ChatMessageDto>>(
                message => Ok(ChatMessageDto.FromDomainModel(message)),
                error => error.ToActionResult()
            );
        }

        [HttpGet("{id}/members")]
        [ProducesResponseType(typeof(IReadOnlyList<ChatMemberDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<ChatMemberDto>>> GetChatMembers(Guid id, CancellationToken cancellationToken)
        {
            var chatId = new ChatId(id);
            var members = await _chatsQueries.GetChatMembers(chatId, cancellationToken);
            var memberDtos = members.Select(ChatMemberDto.FromDomainModel).ToList();
            return Ok(memberDtos);
        }

        [HttpPost("{id}/members")]
        [ProducesResponseType(typeof(ChatMemberDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ChatMemberDto>> AddMember(Guid id, [FromBody] AddMemberRequest request, CancellationToken cancellationToken)
        {
            var requestorId = GetCurrentUserId();
            if (requestorId == null)
                return Unauthorized();

            var command = new AddUserToChatCommand
            {
                ChatId = id,
                RequestorUserId = requestorId.Value,
                TargetUserId = request.TargetUserId,
                Role = request.Role ?? ChatEnums.ChatMemberRole.Member
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.Match<ActionResult<ChatMemberDto>>(
                member => CreatedAtAction(nameof(GetChatMembers), new { id }, ChatMemberDto.FromDomainModel(member)),
                error => error.ToActionResult()
            );
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
