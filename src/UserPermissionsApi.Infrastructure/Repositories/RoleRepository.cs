using Microsoft.EntityFrameworkCore;
using UserPermissions.Application.Interfaces;
using UserPermissions.Domain.Entities;
using UserPermissions.Infrastructure.Persistence;

namespace UserPermissions.Infrastructure.Repositories
{

    public class RoleRepository : IRoleRepository
    {

        private readonly AppDbContext _db;
        public RoleRepository(AppDbContext db) => _db = db;

        public Task<Role?> GetByNameAsync(string name, CancellationToken ct = default) => _db.Roles.FirstOrDefaultAsync(r => r.Name == name, ct);

        public async Task<Role> GetOrCreateAsync(string name, CancellationToken ct = default)
        {
            var normalized = name.Trim();
            var r = await _db.Roles.FirstOrDefaultAsync(x => x.Name == normalized, ct);
            
            if (r is not null) 
                return r;
            
            r = new Role { Name = normalized };

            await _db.Roles.AddAsync(r, ct);
            await _db.SaveChangesAsync(ct);
            
            return r;
        }
    }
}
