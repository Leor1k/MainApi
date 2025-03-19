using AuthApi.Data;
using AuthApi.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersSettingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UsersSettingsController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost("ChangeUserName")]
        public async Task<ActionResult> ChangeUserName([FromBody] ChangeNameRequest request)
        {
            var user = await _context.Users
               .FirstOrDefaultAsync(u => u.user_id == request.UserUd);
            user.username = request.NewUserName;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Имя успешно изменено" });
        }
    }
}
