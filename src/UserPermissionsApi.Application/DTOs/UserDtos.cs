
namespace UserPermissions.Application.DTOs;

public record CreateUserRequest(string Name, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AssignRoleRequest(string RoleName);
public record UserResponse(Guid Id, string Name, string Email, List<string> Roles);
public record LoginResponse(bool Success, string Message, string? Token);
