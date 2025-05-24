using Domain.Users;
using Optional;
using System.Collections.Generic; // Added for IReadOnlyList
using System.Threading; // Added for CancellationToken
using System.Threading.Tasks; // Added for Task

namespace Application.Common.Interfaces.Queries
{
    public interface IUsersQueries
    {
        Task<Option<User>> GetById(UserId id, CancellationToken cancellationToken);

        Task<Option<User>> GetByEmail(string email, CancellationToken cancellationToken); 

        Task<IReadOnlyList<User>> GetAll(CancellationToken cancellationToken);
    }
}