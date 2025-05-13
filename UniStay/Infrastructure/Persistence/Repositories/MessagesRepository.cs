// Файл: Infrastructure/Persistence/Repositories/MessagesRepository.cs
using Application.Common.Interfaces.Queries; // <--- ДОДАНО
using Application.Common.Interfaces.Repositories;
using Domain.Messages;
using Domain.Users;
using Infrastructure.Persistence; // <--- ДОДАНО
using Microsoft.EntityFrameworkCore;
using Optional;
using Optional.Async.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class MessagesRepository : IMessagesRepository, IMessagesQueries // <--- ДОДАНО IMessagesQueries
    {
        private readonly ApplicationDbContext _context;

        public MessagesRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // --- Методи IMessagesRepository ---
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
        
        // GetById для IMessagesRepository
        async Task<Option<Message>> IMessagesRepository.GetById(MessageId id, CancellationToken cancellationToken)
        {
            // Для команд може бути достатньо без Include, якщо оновлюється тільки сам Message
            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
            return message.SomeNotNull();
        }

        // --- Методи IMessagesQueries ---
        // GetById для IMessagesQueries
        async Task<Option<Message>> IMessagesQueries.GetById(MessageId id, CancellationToken cancellationToken)
        {
            var message = await _context.Messages
                .AsNoTracking()
                .Include(m => m.Sender)   // Для MessageDto
                .Include(m => m.Receiver) // Для MessageDto
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
            return message.SomeNotNull();
        }

        public async Task<IReadOnlyList<Message>> GetAllMessagesForUser(UserId userId, CancellationToken cancellationToken)
        {
            return await _context.Messages
                .AsNoTracking()
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderBy(m => m.SendDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Message>> GetConversation(UserId user1Id, UserId user2Id, CancellationToken cancellationToken)
        {
            return await _context.Messages
                .AsNoTracking()
                .Where(m => (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                            (m.SenderId == user2Id && m.ReceiverId == user1Id))
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderBy(m => m.SendDate)
                .ToListAsync(cancellationToken);
        }
    }
}