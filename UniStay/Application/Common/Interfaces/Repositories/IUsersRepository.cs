using Domain.Users;
using Optional;

namespace Application.Common.Interfaces.Repositories
{
    public interface IUsersRepository
    {
        Task<User> Add(User user, CancellationToken cancellationToken);
        Task<User> Update(User user, CancellationToken cancellationToken);
        Task<User> Delete(User user, CancellationToken cancellationToken);
        Task<Option<User>> GetById(UserId id, CancellationToken cancellationToken);
        Task<Option<User>> GetByEmail(string email, CancellationToken cancellationToken);
        Task<IReadOnlyList<User>> GetAll(CancellationToken cancellationToken);
    }
}