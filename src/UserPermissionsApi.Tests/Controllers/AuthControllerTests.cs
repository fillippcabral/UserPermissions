using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserPermissions.Api.Controllers;
using UserPermissions.Application.DTOs;
using UserPermissions.Application.Interfaces;
using Xunit;

namespace UserPermissions.Tests.Controllers
{
    public class AuthControllerTests
    {
        private static (AuthController ctrl, Mock<IAuthService> mockAuth) Build()
        {
            var mock = new Mock<IAuthService>(MockBehavior.Strict);
            var ctrl = new AuthController(mock.Object);
            return (ctrl, mock);
        }

        [Fact]
        public async Task Login_Returns_Ok_When_Success()
        {
            // arrange
            var (ctrl, mockAuth) = Build();
            var req = new LoginRequest("ana@example.com", "secret1");
            var expected = new LoginResponse(
                Success: true,
                Message: "Successful",
                Token: "jwt-token");

            mockAuth
                .Setup(s => s.LoginAsync(req, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            // act
            var result = await ctrl.Login(req, CancellationToken.None);

            // assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var ok = result.Result as OkObjectResult;
            ok!.StatusCode.Should().Be(200);
            ok.Value.Should().BeEquivalentTo(expected);

            mockAuth.Verify(s => s.LoginAsync(req, It.IsAny<CancellationToken>()), Times.Once);
            mockAuth.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Login_Returns_Unauthorized_When_Failure()
        {
            // arrange
            var (ctrl, mockAuth) = Build();
            var req = new LoginRequest("ana@example.com", "wrongpass");
            var expected = new LoginResponse(
                Success: false,
                Token: null,
                Message: "Invalid credentials");

            mockAuth
                .Setup(s => s.LoginAsync(req, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            // act
            var result = await ctrl.Login(req, CancellationToken.None);

            // assert
            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorized = result.Result as UnauthorizedObjectResult;
            unauthorized!.StatusCode.Should().Be(401);
            unauthorized.Value.Should().BeEquivalentTo(expected);

            mockAuth.Verify(s => s.LoginAsync(req, It.IsAny<CancellationToken>()), Times.Once);
            mockAuth.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Login_Returns_BadRequest_When_ArgumentException()
        {
            // arrange
            var (ctrl, mockAuth) = Build();
            var req = new LoginRequest("", "");
            var ex = new ArgumentException("Email is required");

            mockAuth
                .Setup(s => s.LoginAsync(req, It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // act
            var result = await ctrl.Login(req, CancellationToken.None);

            // assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var bad = result.Result as BadRequestObjectResult;
            bad!.StatusCode.Should().Be(400);

            // The controller returns an anonymous object: new { error = ex.Message }
            bad.Value.Should().BeEquivalentTo(new { error = ex.Message });

            mockAuth.Verify(s => s.LoginAsync(req, It.IsAny<CancellationToken>()), Times.Once);
            mockAuth.VerifyNoOtherCalls();
        }
    }

}
