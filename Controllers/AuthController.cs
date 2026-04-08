using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Namaya.Model;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context; // Your EF Core context

    public AuthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // This is where it crashes if the connection string is wrong
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || user.PasswordHash != request.Password)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            return Ok(new { id = user.Id, email = user.Email, role = user.Role, token = "dummy-token" });
        }
        catch (Exception ex)
        {
            // This will send the real error to your React console
            return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
        }
    }
}