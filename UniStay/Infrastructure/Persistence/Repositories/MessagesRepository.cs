using Application.Common.Interfaces.Repositories;
using Domain.Messages;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories
{
    public class MessagesRepository : IMessagesRepository
    {
        private readonly ApplicationDbContext _context;

        public MessagesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Message> Add(Message message, CancellationToken cancellationToken)
        {
            await _context.Messages.AddAsync(message, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return message;
        }

        public async Task<Message> Update(Message message, CancellationToken cancellationToken)
        {
            _context.Messages.Update(message);
            await _context.SaveChangesAsync(cancellationToken);
            return message;
        }

        public async Task<Message> Delete(Message message, CancellationToken cancellationToken)
        {
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync(cancellationToken);
            return message;
        }

        public async Task<Option<Message>> GetById(MessageId id, CancellationToken cancellationToken)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
            return message.SomeNotNull();
        }

        public async Task<IReadOnlyList<Message>> GetAllMessagesForUser(UserId userId, CancellationToken cancellationToken)
        {
            return await _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderBy(m => m.SendDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Message>> GetConversation(UserId senderId, UserId receiverId, CancellationToken cancellationToken)
        {
            return await _context.Messages
                .Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                            (m.SenderId == receiverId && m.ReceiverId == senderId))
                .OrderBy(m => m.SendDate)
                .ToListAsync(cancellationToken);
        }
    }
}