using App.DataAccess.Context;
using App.DataAccess.Repositories;

namespace App.DataAccess.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IAuthRepository? _authRepository;
    private IUserRepository? _userRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IAuthRepository Auth => _authRepository ??= new AuthRepository(_context);

    public IUserRepository Users => _userRepository ??= new UserRepository(_context);

    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
