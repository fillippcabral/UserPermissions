
using UserPermissions.Application.DTOs;

namespace UserPermissions.Application.Interfaces;

public interface IUserService
{
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserResponse> GetUserAsync(Guid id, CancellationToken ct = default);
    Task AssignRoleAsync(Guid userId, AssignRoleRequest request, CancellationToken ct = default);
}
