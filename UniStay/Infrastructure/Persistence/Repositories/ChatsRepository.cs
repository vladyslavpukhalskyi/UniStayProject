using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Chats;
using Domain.Users;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Optional;
using Optional.Async.Extensions;

namespace Infrastructure.Persistence.Repositories
{
    public class ChatsRepository : IChatsRepository, IChatsQueries
    {
        private readonly ApplicationDbContext _context;

        public ChatsRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Chat> Add(Chat chat, CancellationToken cancellationToken)
        {
            await _context.Chats.AddAsync(chat, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return chat;
        }

        public async Task<Chat> Update(Chat chat, CancellationToken cancellationToken)
        {
            _context.Chats.Update(chat);
            await _context.SaveChangesAsync(cancellationToken);
            return chat;
        }

        public async Task<Chat> Delete(Chat chat, CancellationToken cancellationToken)
        {
            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync(cancellationToken);
            return chat;
        }

        async Task<Option<Chat>> IChatsRepository.GetById(ChatId id, CancellationToken cancellationToken)
        {
            var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            return chat.SomeNotNull();
        }

        async Task<Option<Chat>> IChatsQueries.GetById(ChatId id, CancellationToken cancellationToken)
        {
            var chat = await _context.Chats
                .AsNoTracking()
                .Include(c => c.CreatedBy)
                .Include(c => c.Members.Where(m => m.IsActive))
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            return chat.SomeNotNull();
        }

        public async Task<IReadOnlyList<Chat>> GetUserChats(UserId userId, CancellationToken cancellationToken)
        {
            return await _context.Chats
                .AsNoTracking()
                .Where(c => c.IsActive && c.Members.Any(m => m.UserId == userId && m.IsActive))
                .Include(c => c.CreatedBy)
                .Include(c => c.Members.Where(m => m.IsActive))
                    .ThenInclude(m => m.User)
                .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Chat>> GetAllChats(CancellationToken cancellationToken)
        {
            return await _context.Chats
                .AsNoTracking()
                .Where(c => c.IsActive)
                .Include(c => c.CreatedBy)
                .Include(c => c.Members.Where(m => m.IsActive))
                    .ThenInclude(m => m.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsUserMember(ChatId chatId, UserId userId, CancellationToken cancellationToken)
        {
            return await _context.ChatMembers
                .AsNoTracking()
                .AnyAsync(m => m.ChatId == chatId && m.UserId == userId && m.IsActive, cancellationToken);
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

        public async Task<IReadOnlyList<ChatMember>> GetChatMembers(ChatId chatId, CancellationToken cancellationToken)
        {
            return await _context.ChatMembers
                .AsNoTracking()
                .Where(m => m.ChatId == chatId && m.IsActive)
                .Include(m => m.User)
                .OrderBy(m => m.JoinedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
