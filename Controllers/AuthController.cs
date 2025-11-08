using Microsoft.AspNetCore.Mvc;
using AnalyticsPipeline.Models;
using AnalyticsPipeline.Services;
using Microsoft.EntityFrameworkCore;
using AnalyticsPipeline.Data;

namespace AnalyticsPipeline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AnalyticsDbContext _context;
        private readonly AuthService _authService;

        public AuthController(AnalyticsDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // ---------------------
        // Register new user
        // ---------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return BadRequest("User already exists.");

            user.PasswordHash = _authService.HashPassword(user.PasswordHash);
            user.CreatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

        // ---------------------
        // Login
        // ---------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginUser)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginUser.Email);
            if (user == null)
                return Unauthorized("Invalid credentials.");

            if (!_authService.VerifyPassword(loginUser.PasswordHash, user.PasswordHash))
                return Unauthorized("Invalid credentials.");

            var token = _authService.GenerateJwtToken(user);
            return Ok(new { token });
        }
    }
}
