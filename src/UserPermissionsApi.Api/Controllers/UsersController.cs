
using Microsoft.AspNetCore.Mvc;
using UserPermissions.Application.DTOs;
using UserPermissions.Application.Interfaces;

namespace UserPermissions.Api.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users)
    {
        _users = users;
    }

    /// <summary>
    /// Cria um novo usuário.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest req, CancellationToken ct)
    {
        try
        {
            var created = await _users.CreateUserAsync(req, ct);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex) 
        { 
            return BadRequest(new { error = ex.Message }); 
        }
        catch (InvalidOperationException ex) 
        { 
            return Conflict(new { error = ex.Message }); 
        }
    }

    /// <summary>
    /// Obtém o Usuário por Id.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        try
        { 
            return Ok(await _users.GetUserAsync(id, ct)); 
        }
        catch (KeyNotFoundException) 
        { 
            return NotFound(); 
        }
    }

    /// <summary>
    /// Vincula uma função ao Usuário.
    /// </summary>
    [HttpPost("{id:guid}/roles")]
    public async Task<ActionResult> AssignRole([FromRoute] Guid id, [FromBody] AssignRoleRequest req, CancellationToken ct)
    {
        try
        {
            await _users.AssignRoleAsync(id, req, ct);

            return NoContent();
        }
        catch (ArgumentException ex) 
        { 
            return BadRequest(new { error = ex.Message }); 
        }
        catch (KeyNotFoundException) 
        { 
            return NotFound(); 
        }
    }
}
