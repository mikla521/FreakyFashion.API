using FreakyFashion.API.DTOs;
using FreakyFashion.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace FreakyFashion.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly IConfiguration _config;

    public AuthController(TokenService tokenService, IConfiguration config)
    {
        _tokenService = tokenService;
        _config = config;
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        // In a real application, validate against a user store (ASP.NET Identity etc.)
        // For this demo, credentials come from configuration.
        var validUsername = _config["Auth:Username"];
        var validPassword = _config["Auth:Password"];

        if (dto.Username != validUsername || dto.Password != validPassword)
            return Unauthorized(new { error = "Invalid username or password." });

        var token = _tokenService.GenerateToken(dto.Username);
        return Ok(new TokenDto(token, "Bearer", 3600));
    }
}
