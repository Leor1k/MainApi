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
                new ChatParticipant { chatid = chat.chatid, userid = request.UserId1, role = "Создатель" },
                new ChatParticipant { chatid = chat.chatid, userid = request.UserId2, role = "Участник"}
            };
            _context.ChatParticipants.AddRange(participants);
            await _context.SaveChangesAsync();
            return Ok(chat);
        }
        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request, [FromServices] IHubContext<ChatHub> chatHub)
        {
            Console.WriteLine("////////////////////\nПрилетело в API\n////////////////////");

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

            // Отправляем сообщение в группу получателя
            await chatHub.Clients.Group(request.ReceiverId.ToString())
                .SendAsync("ReceiveMessage", message);

            return Ok(message);
        }

        [HttpGet("get-messages/{UserId}/{FriendId}")]
        public async Task<ActionResult> GetUsersList(int UserId, int FriendId)
        {
            
            var chat = await _context.Chats
                .Join(_context.ChatParticipants, c => c.chatid, cp => cp.chatid, (c, cp) => new { Chat = c, Participant = cp })
                .Where(c => c.Participant.userid == UserId || c.Participant.userid == FriendId)
                .GroupBy(c => c.Chat.chatid) 
                .Where(g => g.Count() == 2 && g.All(p => p.Participant.userid == UserId || p.Participant.userid == FriendId))
                .Select(g => g.First().Chat) 
                .FirstOrDefaultAsync();

            if (chat == null)
            {
                return NotFound("Не найден приватный чат между указанными пользователями.");
            }

            var messages = await _context.Messagess
                .Where(m => m.chatid == chat.chatid)
                .OrderBy(m => m.createdat)
                .ToListAsync();

            return Ok(messages);
        }
        [HttpGet("get-chats/{UserId}")]
        public async Task<ActionResult> GetUsersChats(int UserId)
        {
            var chats = await _context.ChatParticipants
                .Where(cp => cp.userid == UserId)
                .Join(
                    _context.Messagess.GroupBy(m => m.chatid)
                        .Select(g => new { ChatId = g.Key, LastMessageTime = g.Max(m => m.createdat) }),
                    cp => cp.chatid,
                    m => m.ChatId,
                    (cp, m) => new { cp.chatid, m.LastMessageTime }
                )
                .OrderByDescending(c => c.LastMessageTime)
                .Select(c => new
                {
                    ChatId = c.chatid,
                    Participants = _context.ChatParticipants
                        .Where(cp => cp.chatid == c.chatid)
                        .Join(
                            _context.Users,
                            cp => cp.userid,
                            u => u.user_id,
                            (cp, u) => new { u.user_id, u.username }
                        )
                        .ToList()
                })
                .ToListAsync();

            if (chats == null || chats.Count == 0)
            {
                return NotFound("Чаты не найдены");
            }

            return Ok(chats);
        }


    }
}
