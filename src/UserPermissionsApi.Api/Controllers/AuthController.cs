
using Microsoft.AspNetCore.Mvc;
using UserPermissions.Application.Interfaces;
using UserPermissions.Application.DTOs;
using Asp.Versioning;

namespace UserPermissions.Api.Controllers;

[ApiController]
[Route("login")]
[ApiVersion(1)]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }
    /// <summary>
    /// Endpoint para fazer a autenticação.
    /// </summary>
    /// <returns>Usuário com o Token gerado</returns>
    /// <response code="200">Retorna que a operação foi bem sucedida juntamente com dados do token gerado</response>
    ///
    [HttpPost]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        try
        {
            var res = await _auth.LoginAsync(req, ct);
            
            if (!res.Success) 
                return Unauthorized(res);

            return Ok(res);
        }
        catch (ArgumentException ex) 
        { 
            return BadRequest(new { error = ex.Message }); 
        }
    }
}
