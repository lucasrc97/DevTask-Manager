using DevTaskManager.Api.Dtos;
using DevTaskManager.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevTaskManager.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthController(ITokenService tokenService, IConfiguration configuration)
    {
        _tokenService = tokenService;
        _configuration = configuration;
    }

    // Endpoint público de login (credenciais fixas para MVP: admin / 123456)
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var expectedUser = _configuration["Auth:Username"] ?? "admin";
        var expectedPass = _configuration["Auth:Password"] ?? "123456";

        if (request.Username != expectedUser || request.Password != expectedPass)
            return Unauthorized(new { message = "Usuário ou senha inválidos." });

        var token = _tokenService.GenerateToken(request.Username);
        return Ok(new { token });
    }
}
