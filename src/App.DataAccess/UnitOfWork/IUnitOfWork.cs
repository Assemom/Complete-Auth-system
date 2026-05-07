using App.DataAccess.Repositories;

namespace App.DataAccess.UnitOfWork;

public interface IUnitOfWork
{
    IAuthRepository Auth { get; }
    IUserRepository Users { get; }
    Task<int> SaveChangesAsync();
}
