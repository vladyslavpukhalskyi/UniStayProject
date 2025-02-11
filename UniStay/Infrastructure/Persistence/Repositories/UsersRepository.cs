using Application.Common.Interfaces.Repositories;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly ApplicationDbContext _context;

        public UsersRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> Add(User user, CancellationToken cancellationToken)
        {
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task<User> Update(User user, CancellationToken cancellationToken)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task<User> Delete(User user, CancellationToken cancellationToken)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task<Option<User>> GetById(UserId id, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
            return user.SomeNotNull();
        }

        public async Task<Option<User>> GetByEmail(string email, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            return user.SomeNotNull();
        }

        public async Task<IReadOnlyList<User>> GetAll(CancellationToken cancellationToken)
        {
            return await _context.Users.ToListAsync(cancellationToken);
        }
    }
}