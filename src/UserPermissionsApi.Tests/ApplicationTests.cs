
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UserPermissions.Application;
using UserPermissions.Application.DTOs;
using UserPermissions.Infrastructure.Persistence;
using UserPermissions.Infrastructure.Repositories;
using UserPermissions.Infrastructure.Security;
using Xunit;

namespace UserPermissions.Tests;

public class ApplicationTests
{
    private (UserService, AuthService) Build()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var db = new AppDbContext(opts);
        var users = new UserRepository(db);
        var roles = new RoleRepository(db);
        var hasher = new PasswordHasher();
        return (new UserService(users, roles, hasher), new AuthService(users, hasher));
    }

    [Fact]
    public async Task Cannot_Create_Duplicate_Email()
    {
        var (svc, _) = Build();
        await svc.CreateUserAsync(new CreateUserRequest("Ana", "ana@example.com", "secret1"));
        Func<Task> act = () => svc.CreateUserAsync(new CreateUserRequest("Ana2", "ana@example.com", "secret1"));
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AssignRole_Is_Idempotent()
    {
        var (svc, _) = Build();
        var u = await svc.CreateUserAsync(new CreateUserRequest("Jo", "jo@example.com", "secret1"));
        await svc.AssignRoleAsync(u.Id, new AssignRoleRequest("Viewer"));
        await svc.AssignRoleAsync(u.Id, new AssignRoleRequest("Viewer"));
        var loaded = await svc.GetUserAsync(u.Id);
        loaded.Roles.Should().ContainSingle().Which.Should().Be("Viewer");
    }

    [Fact]
    public async Task Login_Fails_For_Wrong_Password()
    {
        var (svc, auth) = Build();
        await svc.CreateUserAsync(new CreateUserRequest("Lu", "lu@example.com", "secret1"));
        var res = await auth.LoginAsync(new LoginRequest("lu@example.com", "wrongpassword"));
        res.Success.Should().BeFalse();
    }
}
