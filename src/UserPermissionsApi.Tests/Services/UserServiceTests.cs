using FluentAssertions;
using Moq;
using UserPermissions.Application;
using UserPermissions.Application.DTOs;
using UserPermissions.Application.Interfaces;
using UserPermissions.Domain.Entities;
using Xunit;

namespace UserPermissions.Tests.Application
{
    public class UserServiceTests
    {
        private static (UserService svc,
                        Mock<IUserRepository> users,
                        Mock<IRoleRepository> roles,
                        Mock<IPasswordHasher> hasher)
            Build()
        {
            var users = new Mock<IUserRepository>(MockBehavior.Strict);
            var roles = new Mock<IRoleRepository>(MockBehavior.Strict);
            var hasher = new Mock<IPasswordHasher>(MockBehavior.Strict);
            var svc = new UserService(users.Object, roles.Object, hasher.Object);
            return (svc, users, roles, hasher);
        }

        // -------- CreateUserAsync --------

        [Fact]
        public async Task CreateUser_Throws_On_Invalid_Name()
        {
            var (svc, _, _, _) = Build();
            var req = new CreateUserRequest("   ", "ana@example.com", "secret1");

            Func<Task> act = () => svc.CreateUserAsync(req, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Name*");
        }

        [Fact]
        public async Task CreateUser_Throws_On_Invalid_Email()
        {
            var (svc, _, _, _) = Build();
            var req = new CreateUserRequest("Ana", "not-an-email", "secret1");

            Func<Task> act = () => svc.CreateUserAsync(req, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*email*");
        }

        [Fact]
        public async Task CreateUser_Throws_On_Invalid_Password()
        {
            var (svc, _, _, _) = Build();
            var req = new CreateUserRequest("Ana", "ana@example.com", "123");

            Func<Task> act = () => svc.CreateUserAsync(req, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*password*");
        }

        [Fact]
        public async Task CreateUser_Throws_InvalidOperation_When_Email_Already_In_Use()
        {
            var (svc, users, _, hasher) = Build();
            var req = new CreateUserRequest("Ana", "ana@example.com", "secret1");

            users.Setup(r => r.GetByEmailAsync("ana@example.com", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new User { Email = "ana@example.com" });

            // No hashing/persistence should occur
            var act = () => svc.CreateUserAsync(req, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Email already in use.");

            users.Verify(r => r.GetByEmailAsync("ana@example.com", It.IsAny<CancellationToken>()), Times.Once);
            users.VerifyNoOtherCalls();
            hasher.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateUser_Succeeds_Normalizes_And_Persists()
        {
            var (svc, users, _, hasher) = Build();
            var req = new CreateUserRequest("Ana", "Ana@Example.com", "secret1");

            // Duplicate check
            users.Setup(r => r.GetByEmailAsync("Ana@Example.com", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((User?)null);

            // Hashing
            string outHash = "HASH";
            string outSalt = "SALT";
            hasher.Setup(h => h.CreateHash(It.IsAny<string>(), out outHash, out outSalt));

            // Persistence: set Id when adding
            var newId = Guid.NewGuid();
            users.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                 .Callback<User, CancellationToken>((u, _) => u.Id = newId)
                 .Returns(Task.CompletedTask);

            users.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

            var res = await svc.CreateUserAsync(req, CancellationToken.None);

            res.Id.Should().Be(newId);
            res.Name.Should().Be("Ana"); // trimmed
            res.Email.Should().Be("ana@example.com"); // trimmed + lower
            res.Roles.Should().BeEmpty();

            users.Verify(r => r.GetByEmailAsync("Ana@Example.com", It.IsAny<CancellationToken>()), Times.Once);
            hasher.Verify(h => h.CreateHash("secret1", out outHash, out outSalt), Times.Once);

            users.Verify(r => r.AddAsync(It.Is<User>(u =>
                    u.Id == newId &&
                    u.Name == "Ana" &&
                    u.Email == "ana@example.com" &&
                    u.PasswordHash == "HASH" &&
                    u.PasswordSalt == "SALT"),
                It.IsAny<CancellationToken>()), Times.Once);

            users.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            users.VerifyNoOtherCalls();
            hasher.VerifyNoOtherCalls();
        }

        // -------- GetUserAsync --------

        [Fact]
        public async Task GetUser_Returns_User_With_Sorted_Roles()
        {
            var (svc, users, _, _) = Build();
            var id = Guid.NewGuid();

            var admin = new Role { Id = Guid.NewGuid(), Name = "admin" };
            var editor = new Role { Id = Guid.NewGuid(), Name = "editor" };

            var user = new User
            {
                Id = id,
                Name = "Ana",
                Email = "ana@example.com",
                UserRoles = new List<UserRole>
                {
                    new UserRole { RoleId = editor.Id, Role = editor },
                    new UserRole { RoleId = admin.Id, Role = admin }
                }
            };

            users.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(user);

            var res = await svc.GetUserAsync(id, CancellationToken.None);

            res.Id.Should().Be(id);
            res.Name.Should().Be("Ana");
            res.Email.Should().Be("ana@example.com");
            res.Roles.Should().Equal("admin", "editor"); // sorted

            users.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            users.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetUser_Throws_When_Not_Found()
        {
            var (svc, users, _, _) = Build();
            var id = Guid.NewGuid();

            users.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((User?)null);

            var act = () => svc.GetUserAsync(id, CancellationToken.None);
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("User not found.");

            users.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            users.VerifyNoOtherCalls();
        }

        // -------- AssignRoleAsync --------

        [Fact]
        public async Task AssignRole_Throws_On_Empty_RoleName()
        {
            var (svc, _, _, _) = Build();
            var req = new AssignRoleRequest("   ");

            var act = () => svc.AssignRoleAsync(Guid.NewGuid(), req, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Role name is required.*");
        }

        [Fact]
        public async Task AssignRole_Throws_When_User_Not_Found()
        {
            var (svc, users, roles, _) = Build();
            var userId = Guid.NewGuid();
            var req = new AssignRoleRequest("manager");

            users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((User?)null);

            var act = () => svc.AssignRoleAsync(userId, req, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("User not found.");

            users.Verify(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
            users.VerifyNoOtherCalls();
            roles.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task AssignRole_Is_Idempotent_When_Already_Assigned()
        {
            var (svc, users, roles, _) = Build();
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var req = new AssignRoleRequest("manager");

            var role = new Role { Id = roleId, Name = "manager" };
            var user = new User
            {
                Id = userId,
                UserRoles = new List<UserRole>
                {
                    new UserRole { UserId = userId, RoleId = roleId, Role = role }
                }
            };

            users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(user);

            roles.Setup(r => r.GetOrCreateAsync("manager", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(role);

            // No SaveChanges should happen because role already present
            await svc.AssignRoleAsync(userId, req, CancellationToken.None);

            users.Verify(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
            roles.Verify(r => r.GetOrCreateAsync("manager", It.IsAny<CancellationToken>()), Times.Once);
            users.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            users.VerifyNoOtherCalls();
            roles.VerifyNoOtherCalls();

            user.UserRoles.Should().HaveCount(1);
        }

        [Fact]
        public async Task AssignRole_Adds_Role_And_Saves_When_Not_Assigned()
        {
            var (svc, users, roles, _) = Build();
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var req = new AssignRoleRequest("  manager  "); // will be trimmed

            var role = new Role { Id = roleId, Name = "manager" };
            var user = new User { Id = userId, UserRoles = new List<UserRole>() };

            users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(user);

            roles.Setup(r => r.GetOrCreateAsync("manager", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(role);

            users.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

            await svc.AssignRoleAsync(userId, req, CancellationToken.None);

            user.UserRoles.Should().ContainSingle(ur => ur.RoleId == roleId && ur.UserId == userId);

            users.Verify(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
            roles.Verify(r => r.GetOrCreateAsync("manager", It.IsAny<CancellationToken>()), Times.Once);
            users.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            users.VerifyNoOtherCalls();
            roles.VerifyNoOtherCalls();
        }
    }
}
