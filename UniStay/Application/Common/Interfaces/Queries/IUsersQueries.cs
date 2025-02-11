using Domain.Users;
using Optional;

namespace Application.Common.Interfaces.Queries
{
    public interface IUsersQueries
    {
        Task<Option<User>> GetById(UserId id, CancellationToken cancellationToken);

        Task<Option<User>> GetByEmail(string email, CancellationToken cancellationToken);

        Task<IReadOnlyList<User>> GetAll(CancellationToken cancellationToken);
    }
}