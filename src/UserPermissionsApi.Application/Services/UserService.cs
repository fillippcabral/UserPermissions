
using UserPermissions.Application.Interfaces;
using UserPermissions.Application.DTOs;
using UserPermissions.Application.Services;
using UserPermissions.Domain.Entities;

namespace UserPermissions.Application;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IPasswordHasher _hasher;

    public UserService(IUserRepository users, IRoleRepository roles, IPasswordHasher hasher)
    {
        _users = users;
        _roles = roles;
        _hasher = hasher;
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        Validation.ValidateName(request.Name);
        Validation.ValidateEmail(request.Email);
        Validation.ValidatePassword(request.Password);

        var existing = await _users.GetByEmailAsync(request.Email, ct);
        if (existing is not null) 
            throw new InvalidOperationException("Email already in use.");

        _hasher.CreateHash(request.Password, out var hash, out var salt);

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = hash,
            PasswordSalt = salt
        };

        await _users.AddAsync(user, ct);
        await _users.SaveChangesAsync(ct);

        return new UserResponse(user.Id, user.Name, user.Email, new());
    }

    public async Task<UserResponse> GetUserAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("User not found.");
        var roles = user.UserRoles.Select(ur => ur.Role!.Name).OrderBy(n => n).ToList();
        
        return new UserResponse(user.Id, user.Name, user.Email, roles);
    }

    public async Task AssignRoleAsync(Guid userId, AssignRoleRequest request, CancellationToken ct = default)
    {
        Validation.Require(!string.IsNullOrWhiteSpace(request.RoleName), "Role name is required.");

        var user = await _users.GetByIdAsync(userId, ct) ?? throw new KeyNotFoundException("User not found.");
        var role = await _roles.GetOrCreateAsync(request.RoleName.Trim(), ct);

        if (user.UserRoles.Any(ur => ur.RoleId == role.Id)) 
            return;

        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, Role = role, User = user });

        await _users.SaveChangesAsync(ct);
    }
}
