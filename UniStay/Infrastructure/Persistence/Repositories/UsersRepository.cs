using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Users;
using Infrastructure.Persistence;
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
    public class UsersRepository : IUsersRepository, IUsersQueries
    {
        private readonly ApplicationDbContext _context;

        public UsersRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
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

        async Task<Option<User>> IUsersRepository.GetById(UserId id, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            return user.SomeNotNull();
        }

        async Task<Option<User>> IUsersRepository.GetByEmail(string email, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            return user.SomeNotNull();
        }


        public async Task<IReadOnlyList<User>> GetAll(CancellationToken cancellationToken)
        {
            return await _context.Users.AsNoTracking().ToListAsync(cancellationToken);
        }

        async Task<Option<User>> IUsersQueries.GetById(UserId id, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                                .AsNoTracking()
                                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            return user.SomeNotNull();
        }

        async Task<Option<User>> IUsersQueries.GetByEmail(string email, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                                .AsNoTracking()
                                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            return user.SomeNotNull();
        }
    }
}