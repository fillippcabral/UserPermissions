
using UserPermissions.Application.Interfaces;
using UserPermissions.Application.DTOs;
using UserPermissions.Application.Services;

namespace UserPermissions.Application;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;

    public AuthService(IUserRepository users, IPasswordHasher hasher)
    {
        _users = users;
        _hasher = hasher;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        Validation.ValidateEmail(request.Email);
        Validation.ValidatePassword(request.Password);

        var user = await _users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);

        if (user is null) 
            return new(false, "Invalid credentials.", null);

        var ok = _hasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt);
        
        if (!ok) 
            return new(false, "Invalid credentials.", null);

        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        
        return new(true, "Login successful.", token);
    }
}
