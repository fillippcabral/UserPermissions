
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UserPermissions.Application.DTOs;
using Xunit;

namespace UserPermissions.Tests;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task User_Lifecycle_Works()
    {
        var client = _factory.CreateClient();

        // Create user
        var create = new CreateUserRequest("Alice", "alice@example.com", "secret1");
        var createdResp = await client.PostAsJsonAsync("/users", create);
        createdResp.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var created = await createdResp.Content.ReadFromJsonAsync<UserResponse>();
        created.Should().NotBeNull();
        created!.Email.Should().Be("alice@example.com");

        // Get by id
        var get = await client.GetFromJsonAsync<UserResponse>($"/users/{created!.Id}");
        get!.Name.Should().Be("Alice");
        get!.Roles.Should().BeEmpty();

        // Assign role
        var assign = new AssignRoleRequest("Admin");
        var assignResp = await client.PostAsJsonAsync($"/users/{created!.Id}/roles", assign);
        assignResp.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        // Fetch again
        var get2 = await client.GetFromJsonAsync<UserResponse>($"/users/{created!.Id}");
        get2!.Roles.Should().ContainSingle().Which.Should().Be("Admin");

        // Login ok
        var loginOk = await client.PostAsJsonAsync("/login", new LoginRequest("alice@example.com", "secret1"));
        loginOk.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var loginData = await loginOk.Content.ReadFromJsonAsync<LoginResponse>();
        loginData!.Success.Should().BeTrue();
        loginData.Token.Should().NotBeNullOrWhiteSpace();

        // Login fail
        var loginFail = await client.PostAsJsonAsync("/login", new LoginRequest("alice@example.com", "badbad"));
        loginFail.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Validation_Errors()
    {
        var client = _factory.CreateClient();

        // Bad email
        var badEmail = await client.PostAsJsonAsync("/users", new CreateUserRequest("Bob", "not-email", "secret1"));
        badEmail.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        // Short password
        var shortPass = await client.PostAsJsonAsync("/users", new CreateUserRequest("Bob", "bob@example.com", "123"));
        shortPass.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}
