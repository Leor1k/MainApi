using AuthApi.Data;
using AuthApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {   
        private readonly AppDbContext _context;
        public ChatsController(AppDbContext context, EmailService emailService)
        {
            _context = context;
        }
        [HttpPost("create-private-chat")]
        public async Task<IActionResult> CreatePrivateChat([FromBody] CreateChatRequest request)
        {
            if (request.UserId1 == request.UserId2)
                return BadRequest("Нельзя создать личный чат с самим собой.");
            var existingChat = await _context.Chats
                .FirstOrDefaultAsync(c =>
                    c.chattype == "private" &&
                    _context.ChatParticipants.Any(p => p.chatid == c.chatid && p.userid == request.UserId1) &&
                    _context.ChatParticipants.Any(p => p.chatid == c.chatid && p.userid == request.UserId2));

            if (existingChat != null)
                return Conflict("Личный чат между этими пользователями уже существует.");

            var chat = new Chat
            {
                chatname = "Личный чат",
                chattype = "private",
                createdat = DateTime.UtcNow
            };

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            var participants = new List<ChatParticipant>
            {
                new ChatParticipant { chatid = chat.chatid, userid = request.UserId1 },
                new ChatParticipant { chatid = chat.chatid, userid = request.UserId2 }
            };
            _context.ChatParticipants.AddRange(participants);
            await _context.SaveChangesAsync();
            return Ok(chat);
        }


    }
}
