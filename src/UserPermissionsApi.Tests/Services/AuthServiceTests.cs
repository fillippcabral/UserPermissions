using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using UserPermissions.Application;
using UserPermissions.Application.DTOs;
using UserPermissions.Application.Interfaces;
using UserPermissions.Domain.Entities;
using Xunit;

namespace UserPermissions.Tests.Application
{
    public class AuthServiceTests
    {
        private static (AuthService svc, Mock<IUserRepository> users, Mock<IPasswordHasher> hasher)
            Build()
        {
            var users = new Mock<IUserRepository>(MockBehavior.Strict);
            var hasher = new Mock<IPasswordHasher>(MockBehavior.Strict);
            var svc = new AuthService(users.Object, hasher.Object);
            return (svc, users, hasher);
        }

        [Fact]
        public async Task LoginAsync_Throws_On_Invalid_Email()
        {
            var (svc, _, _) = Build();
            var req = new LoginRequest("not-an-email", "secret1");

            Func<Task> act = () => svc.LoginAsync(req, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*email*");
        }

        [Fact]
        public async Task LoginAsync_Throws_On_Invalid_Password()
        {
            var (svc, _, _) = Build();
            var req = new LoginRequest("ana@example.com", "123"); // too short per typical rule

            Func<Task> act = () => svc.LoginAsync(req, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*password*");
        }

        [Fact]
        public async Task LoginAsync_Returns_Invalid_When_User_Not_Found()
        {
            var (svc, users, _) = Build();
            var req = new LoginRequest("Ana@Example.com", "secret1");
            var normalized = "ana@example.com";

            users.Setup(r => r.GetByEmailAsync(
                        It.Is<string>(e => e == normalized),
                        It.IsAny<CancellationToken>()))
                 .ReturnsAsync((User?)null);

            var res = await svc.LoginAsync(req, CancellationToken.None);

            res.Success.Should().BeFalse();
            res.Message.Should().Be("Invalid credentials.");
            res.Token.Should().BeNull();

            users.Verify(r => r.GetByEmailAsync(normalized, It.IsAny<CancellationToken>()), Times.Once);
            users.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task LoginAsync_Returns_Invalid_When_Password_Incorrect()
        {
            var (svc, users, hasher) = Build();
            var req = new LoginRequest("ana@example.com", "wrongpass");

            var user = new User
            {
                Email = "ana@example.com",
                PasswordHash = "HASH",
                PasswordSalt = "SALT"
            };

            users.Setup(r => r.GetByEmailAsync("ana@example.com", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(user);

            hasher.Setup(h => h.Verify("wrongpass", "HASH", "SALT"))
                  .Returns(false);

            var res = await svc.LoginAsync(req, CancellationToken.None);

            res.Success.Should().BeFalse();
            res.Message.Should().Be("Invalid credentials.");
            res.Token.Should().BeNull();

            users.Verify(r => r.GetByEmailAsync("ana@example.com", It.IsAny<CancellationToken>()), Times.Once);
            hasher.Verify(h => h.Verify("wrongpass", "HASH", "SALT"), Times.Once);
            users.VerifyNoOtherCalls();
            hasher.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task LoginAsync_Returns_Success_With_Token_When_Credentials_Are_Correct()
        {
            var (svc, users, hasher) = Build();
            var req = new LoginRequest("Ana@Example.com", "secret1");
            var normalized = "ana@example.com";

            var user = new User
            {
                Email = normalized,
                PasswordHash = "HASH",
                PasswordSalt = "SALT"
            };

            users.Setup(r => r.GetByEmailAsync(normalized, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(user);

            hasher.Setup(h => h.Verify("secret1", "HASH", "SALT"))
                  .Returns(true);

            var res = await svc.LoginAsync(req, CancellationToken.None);

            res.Success.Should().BeTrue();
            res.Message.Should().Be("Login successful.");
            res.Token.Should().NotBeNullOrWhiteSpace();
            // The service creates a Base64 Guid token; just ensure it's non-empty
            res.Token!.Length.Should().BeGreaterThan(10);

            users.Verify(r => r.GetByEmailAsync(normalized, It.IsAny<CancellationToken>()), Times.Once);
            hasher.Verify(h => h.Verify("secret1", "HASH", "SALT"), Times.Once);
            users.VerifyNoOtherCalls();
            hasher.VerifyNoOtherCalls();
        }
    }
}
