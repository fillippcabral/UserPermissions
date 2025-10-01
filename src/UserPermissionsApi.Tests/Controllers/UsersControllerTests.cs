using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserPermissions.Api.Controllers;
using UserPermissions.Application.DTOs;
using UserPermissions.Application.Interfaces;
using Xunit;

namespace UserPermissions.Tests.Controllers
{
    public class UsersControllerTests
    {
        private static (UsersController ctrl, Mock<IUserService> mockSvc) Build()
        {
            var mock = new Mock<IUserService>(MockBehavior.Strict);
            var ctrl = new UsersController(mock.Object);
            return (ctrl, mock);
        }

        // ---------- POST /users ----------
        [Fact]
        public async Task Create_Returns_Created_At_GetById_On_Success()
        {
            // arrange
            var (ctrl, mock) = Build();
            var userId = Guid.NewGuid();

            var req = new CreateUserRequest("Ana", "ana@example.com", "secret1");
            var created = new UserResponse(userId, "Ana", "ana@example.com", new List<string>());
            

            mock.Setup(s => s.CreateUserAsync(req, It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

            // act
            var result = await ctrl.Create(req, CancellationToken.None);

            // assert
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            var createdAt = (CreatedAtActionResult)result.Result!;
            createdAt.ActionName.Should().Be(nameof(UsersController.GetById));
            createdAt.Value.Should().BeSameAs(created);
            createdAt.RouteValues.Should().ContainKey("id");
            createdAt.RouteValues!["id"].Should().Be(userId);

            mock.Verify(s => s.CreateUserAsync(req, It.IsAny<CancellationToken>()), Times.Once);
            mock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Create_Returns_BadRequest_On_ArgumentException()
        {
            // arrange
            var (ctrl, mock) = Build();
            var req = new CreateUserRequest("", "invalid", "123"); // invalid on purpose
            var ex = new ArgumentException("Invalid email");

            mock.Setup(s => s.CreateUserAsync(req, It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // act
            var result = await ctrl.Create(req, CancellationToken.None);

            // assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var bad = (BadRequestObjectResult)result.Result!;
            bad.Value.Should().BeEquivalentTo(new { error = ex.Message });

            mock.Verify(s => s.CreateUserAsync(req, It.IsAny<CancellationToken>()), Times.Once);
            mock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Create_Returns_Conflict_On_InvalidOperationException()
        {
            // arrange
            var (ctrl, mock) = Build();
            var req = new CreateUserRequest("Ana", "ana@example.com", "secret1");
            var ex = new InvalidOperationException("Email already exists");

            mock.Setup(s => s.CreateUserAsync(req, It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // act
            var result = await ctrl.Create(req, CancellationToken.None);

            // assert
            result.Result.Should().BeOfType<ConflictObjectResult>();
            var conflict = (ConflictObjectResult)result.Result!;
            conflict.Value.Should().BeEquivalentTo(new { error = ex.Message });

            mock.Verify(s => s.CreateUserAsync(req, It.IsAny<CancellationToken>()), Times.Once);
            mock.VerifyNoOtherCalls();
        }

        // ---------- GET /users/{id} ----------
        [Fact]
        public async Task GetById_Returns_Ok_With_User()
        {
            // arrange
            var (ctrl, mock) = Build();
            var id = Guid.NewGuid();

            var user = new UserResponse(id, "Ana", "ana@example.com", new List<string>() { "admin" });

            mock.Setup(s => s.GetUserAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // act
            var result = await ctrl.GetById(id, CancellationToken.None);

            // assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var ok = (OkObjectResult)result.Result!;
            ok.Value.Should().BeSameAs(user);

            mock.Verify(s => s.GetUserAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            mock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetById_Returns_NotFound_On_KeyNotFound()
        {
            // arrange
            var (ctrl, mock) = Build();
            var id = Guid.NewGuid();

            mock.Setup(s => s.GetUserAsync(id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException());

            // act
            var result = await ctrl.GetById(id, CancellationToken.None);

            // assert
            result.Result.Should().BeOfType<NotFoundResult>();

            mock.Verify(s => s.GetUserAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            mock.VerifyNoOtherCalls();
        }

        // ---------- POST /users/{id}/roles ----------
        [Fact]
        public async Task AssignRole_Returns_NoContent_On_Success()
        {
            // arrange
            var (ctrl, mock) = Build();
            var id = Guid.NewGuid();
            var req = new AssignRoleRequest("manager");

            mock.Setup(s => s.AssignRoleAsync(id, req, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // act
            var result = await ctrl.AssignRole(id, req, CancellationToken.None);

            // assert
            result.Should().BeOfType<NoContentResult>();

            mock.Verify(s => s.AssignRoleAsync(id, req, It.IsAny<CancellationToken>()), Times.Once);
            mock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task AssignRole_Returns_BadRequest_On_ArgumentException()
        {
            // arrange
            var (ctrl, mock) = Build();
            var id = Guid.NewGuid();
            var req = new AssignRoleRequest("");

            mock.Setup(s => s.AssignRoleAsync(id, req, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Role is required"));

            // act
            var result = await ctrl.AssignRole(id, req, CancellationToken.None);

            // assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var bad = (BadRequestObjectResult)result;
            bad.Value.Should().BeEquivalentTo(new { error = "Role is required" });

            mock.Verify(s => s.AssignRoleAsync(id, req, It.IsAny<CancellationToken>()), Times.Once);
            mock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task AssignRole_Returns_NotFound_On_KeyNotFound()
        {
            // arrange
            var (ctrl, mock) = Build();
            var id = Guid.NewGuid();
            var req = new AssignRoleRequest("manager");

            mock.Setup(s => s.AssignRoleAsync(id, req, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException());

            // act
            var result = await ctrl.AssignRole(id, req, CancellationToken.None);

            // assert
            result.Should().BeOfType<NotFoundResult>();

            mock.Verify(s => s.AssignRoleAsync(id, req, It.IsAny<CancellationToken>()), Times.Once);
            mock.VerifyNoOtherCalls();
        }
    }
}
