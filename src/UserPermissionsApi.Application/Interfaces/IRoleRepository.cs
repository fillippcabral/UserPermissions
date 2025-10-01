using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserPermissions.Domain.Entities;

namespace UserPermissions.Application.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> GetByNameAsync(string name, CancellationToken ct = default);
        Task<Role> GetOrCreateAsync(string name, CancellationToken ct = default);
    }
}
