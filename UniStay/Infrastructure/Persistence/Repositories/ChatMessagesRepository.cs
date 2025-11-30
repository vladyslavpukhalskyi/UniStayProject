using Application.Common.Interfaces.Repositories;
using Domain.Chats;
using Domain.Users;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Optional;
using Optional.Async.Extensions;

namespace Infrastructure.Persistence.Repositories
{
    public class ChatMessagesRepository : IChatMessagesRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatMessagesRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ChatMessage> Add(ChatMessage message, CancellationToken cancellationToken)
        {
            await _context.ChatMessages.AddAsync(message, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return message;
        }

        public async Task<ChatMessage> Update(ChatMessage message, CancellationToken cancellationToken)
        {
            _context.ChatMessages.Update(message);
            await _context.SaveChangesAsync(cancellationToken);
            return message;
        }

        public async Task<ChatMessage> Delete(ChatMessage message, CancellationToken cancellationToken)
        {
            _context.ChatMessages.Remove(message);
            await _context.SaveChangesAsync(cancellationToken);
            return message;
        }

        public async Task<Option<ChatMessage>> GetById(ChatMessageId id, CancellationToken cancellationToken)
        {
            var message = await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Chat)
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
            return message.SomeNotNull();
        }

        public async Task<IReadOnlyList<ChatMessage>> GetChatMessages(ChatId chatId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _context.ChatMessages
                .AsNoTracking()
                .Where(m => m.ChatId == chatId && !m.IsDeleted)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.SentAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ChatMessage>> GetUserMessages(UserId userId, CancellationToken cancellationToken)
        {
            return await _context.ChatMessages
                .AsNoTracking()
                .Where(m => m.SenderId == userId && !m.IsDeleted)
                .Include(m => m.Chat)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetChatMessageCount(ChatId chatId, CancellationToken cancellationToken)
        {
            return await _context.ChatMessages
                .AsNoTracking()
                .CountAsync(m => m.ChatId == chatId && !m.IsDeleted, cancellationToken);
        }
    }
}
