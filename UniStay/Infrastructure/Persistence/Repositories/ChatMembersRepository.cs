using Application.Common.Interfaces.Repositories;
using Domain.Chats;
using Domain.Users;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Optional;
using Optional.Async.Extensions;

namespace Infrastructure.Persistence.Repositories
{
    public class ChatMembersRepository : IChatMembersRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatMembersRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ChatMember> Add(ChatMember member, CancellationToken cancellationToken)
        {
            await _context.ChatMembers.AddAsync(member, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return member;
        }

        public async Task<ChatMember> Update(ChatMember member, CancellationToken cancellationToken)
        {
            _context.ChatMembers.Update(member);
            await _context.SaveChangesAsync(cancellationToken);
            return member;
        }

        public async Task<ChatMember> Delete(ChatMember member, CancellationToken cancellationToken)
        {
            _context.ChatMembers.Remove(member);
            await _context.SaveChangesAsync(cancellationToken);
            return member;
        }

        public async Task<Option<ChatMember>> GetById(ChatMemberId id, CancellationToken cancellationToken)
        {
            var member = await _context.ChatMembers
                .Include(m => m.User)
                .Include(m => m.Chat)
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
            return member.SomeNotNull();
        }

        public async Task<Option<ChatMember>> GetByChatAndUser(ChatId chatId, UserId userId, CancellationToken cancellationToken)
        {
            var member = await _context.ChatMembers
                .Include(m => m.User)
                .Include(m => m.Chat)
                .FirstOrDefaultAsync(m => m.ChatId == chatId && m.UserId == userId && m.IsActive, cancellationToken);
            return member.SomeNotNull();
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

        public async Task<IReadOnlyList<ChatMember>> GetUserMemberships(UserId userId, CancellationToken cancellationToken)
        {
            return await _context.ChatMembers
                .AsNoTracking()
                .Where(m => m.UserId == userId && m.IsActive)
                .Include(m => m.Chat)
                .OrderByDescending(m => m.JoinedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
