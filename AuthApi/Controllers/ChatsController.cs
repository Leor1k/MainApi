using AuthApi.Data;
using AuthApi.Models;
using AuthApi.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request, [FromServices] IHubContext<ChatHub> chatHub)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("Сообщение не может быть пустым.");

            var chat = await _context.Chats.FindAsync(request.ChatId);
            if (chat == null)
                return NotFound("Чат не найден.");

            var isParticipant = await _context.ChatParticipants
                .AnyAsync(p => p.chatid == request.ChatId && p.userid == request.SenderId);

            if (!isParticipant)
                return Forbid("Вы не являетесь участником данного чата.");

            var message = new Messages
            {
                chatid = request.ChatId,
                senderid = request.SenderId,
                content = request.Content,
                createdat = DateTime.UtcNow
            };

            _context.Messagess.Add(message);
            await _context.SaveChangesAsync();

            await chatHub.Clients.Group(request.ChatId.ToString())
                .SendAsync("ReceiveMessage", message);

            return Ok(message);
        }

        //[HttpGet("{userIdIn}/list")]
        [HttpGet("get-messages/{ChatId}/{UserId}")]
        public async Task<ActionResult> GetUsersList (int ChatId, int UserId)
        {
            var isParticipant = await _context.ChatParticipants
     .AnyAsync(p => p.chatid == ChatId && p.userid == UserId);
            Console.WriteLine($"ID user: {UserId}\nId chat {ChatId}");
            if (!isParticipant)
                return StatusCode(StatusCodes.Status403Forbidden, "Вы не являетесь участником данного чата.");

            var messages = await _context.Messagess
                .Where(m => m.chatid == ChatId)
                .OrderBy(m => m.createdat)
                .ToListAsync();

            return Ok(messages);
        }
    }
}
