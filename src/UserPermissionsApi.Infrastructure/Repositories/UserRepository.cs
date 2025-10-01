using Microsoft.EntityFrameworkCore;
using UserPermissions.Application.Interfaces;
using UserPermissions.Domain.Entities;
using UserPermissions.Infrastructure.Persistence;

namespace UserPermissions.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
            _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                     .FirstOrDefaultAsync(u => u.Email == email, ct);

        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                     .FirstOrDefaultAsync(u => u.Id == id, ct);

        public async Task AddAsync(User user, CancellationToken ct = default)
        {
            await _db.Users.AddAsync(user, ct);
        }

        public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}
