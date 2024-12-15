using AuthApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest loginRequest)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.username == loginRequest.username && u.password == loginRequest.password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }
            return Ok(new { message = "Login successful" });
        }
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync(); // Получаем всех пользователей из базы данных

            if (users == null || users.Count == 0)
            {
                return NotFound(new { message = "No users found" }); // Если пользователей нет
            }

            return Ok(users); // Возвращаем список пользователей
        }

    }
}
