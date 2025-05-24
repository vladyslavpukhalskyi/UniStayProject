// Файл: Infrastructure/Persistence/Repositories/UsersRepository.cs
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Users;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Optional;
using Optional.Async.Extensions; // Переконайтесь, що цей пакет встановлено, якщо використовуєте SomeNotNull()
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

        // --- Методи IUsersRepository (для операцій запису/зміни) ---
        // Ці методи повертають відстежувані сутності, якщо вони використовуються для модифікації.
        public async Task<User> Add(User user, CancellationToken cancellationToken)
        {
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task<User> Update(User user, CancellationToken cancellationToken)
        {
            _context.Users.Update(user); // EF Core почне відстежувати сутність, якщо вона ще не відстежується
            await _context.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task<User> Delete(User user, CancellationToken cancellationToken)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
            return user;
        }

        // Явна реалізація GetById для IUsersRepository (зазвичай повертає відстежуваний об'єкт для подальших змін)
        async Task<Option<User>> IUsersRepository.GetById(UserId id, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            return user.SomeNotNull();
        }

        // Явна реалізація GetByEmail для IUsersRepository (також може повертати відстежуваний об'єкт)
        async Task<Option<User>> IUsersRepository.GetByEmail(string email, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            return user.SomeNotNull();
        }


        // --- Методи IUsersQueries (для операцій читання) ---
        // Ці методи використовують AsNoTracking(), оскільки вони призначені лише для читання даних.
        public async Task<IReadOnlyList<User>> GetAll(CancellationToken cancellationToken)
        {
            return await _context.Users.AsNoTracking().ToListAsync(cancellationToken);
        }

        // Явна реалізація GetById для IUsersQueries (не відстежується)
        async Task<Option<User>> IUsersQueries.GetById(UserId id, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                                .AsNoTracking()
                                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            return user.SomeNotNull();
        }

        // Явна реалізація GetByEmail для IUsersQueries (не відстежується)
        async Task<Option<User>> IUsersQueries.GetByEmail(string email, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                                .AsNoTracking()
                                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            return user.SomeNotNull();
        }
    }
}