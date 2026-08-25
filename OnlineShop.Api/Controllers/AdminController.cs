using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AdminController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public class LoginRequest
    {
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var expected = _configuration["Admin:Password"];
        if (string.IsNullOrWhiteSpace(expected)
            || !string.Equals(request.Password, expected, StringComparison.Ordinal))
        {
            return Unauthorized(new { message = "Invalid password." });
        }

        return Ok(new { message = "Authenticated." });
    }
}
