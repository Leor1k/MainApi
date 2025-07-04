﻿using AuthApi.Data;
using AuthApi.Models;
using AuthApi.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public FriendsController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("{userIdIn}/list")]
        public async Task<ActionResult> GetFriends(int userIdIn)
        {
            Console.WriteLine($"Запрос на друзей для пользователя с ID: {userIdIn}");
            var friends = await _context.Friendships
                .Where(f => (f.user_id == userIdIn || f.friend_id == userIdIn) && f.status.Trim() == "В друзьях")
                .Select(f => new
                {
                    FriendId = f.user_id == userIdIn ? f.friend_id : f.user_id,
                    FriendName = f.user_id == userIdIn ? f.friend.username : f.user.username,
                    FriendStatus = f.user_id == userIdIn ? f.friend.status : f.user.status,
                    FriendPhoto = f.user_id == userIdIn ? f.friend.avatar_url : f.user.avatar_url
                })
                 .Distinct()
                .ToListAsync();

            if (friends.Count == 0)
            {
                return NotFound("У вас пока нет друзей....");
            }
            else
            {
                return Ok(friends);
            }
        }

        [HttpPost("request/send")]
        public async Task<IActionResult> SendFriendRequest(FriendsRequest request, [FromServices] IHubContext<ChatHub> chatHub)
        {
            var existingRequest = await _context.Friendships
                .FirstOrDefaultAsync(f => f.user_id == request.UserId && f.friend_id == request.FriendId);

            if (existingRequest != null)
                return BadRequest("Вы уже друзья!");

            var friendRequest = new Friendships
            {
                user_id = request.UserId,
                friend_id = request.FriendId,
                status = "В ожидании ответа"
            };
            var UserName = await _context.Users.FirstOrDefaultAsync(i => i.user_id == request.UserId);
            FriendSignalR ForSendFriendRequest = new FriendSignalR(request.FriendId, request.UserId, UserName.username, UserName.avatar_url);
            Console.WriteLine($"По порядку: {request.FriendId}; {request.UserId}; {UserName.username}; {UserName.avatar_url}");
            await chatHub.Clients.Group(request.FriendId.ToString())
               .SendAsync("NewRequestOnFriend", ForSendFriendRequest);

            _context.Friendships.Add(friendRequest);
            await _context.SaveChangesAsync();

            return Ok("Заявка в друзья отправлена");
        }
        [HttpPost("request/accept")]
        public async Task<IActionResult> AcceptFriendRequest(FriendsRequest request)
        {
            var friendRequest = await _context.Friendships
                .FirstOrDefaultAsync(f => f.user_id == request.FriendId && f.friend_id == request.UserId && f.status == "В ожидании ответа");

            if (friendRequest == null)
                return NotFound("Пользователя ненайдено");

            friendRequest.status = "В друзьях";
            _context.Friendships.Update(friendRequest);

            var reverseFriendship = new Friendships
            {
                user_id = request.UserId,
                friend_id = request.FriendId,
                status = "В друзьях",
                created_at = DateTime.UtcNow
            };
            _context.Friendships.Add(reverseFriendship);

            await _context.SaveChangesAsync();


            return Ok("Вы стали друзьями!");
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsersByIds([FromQuery] int currentUserId, [FromQuery] List<int> userIds)
        {
            var friendsIds = await _context.Friendships
                .Where(f => f.user_id == currentUserId || f.friend_id == currentUserId)
                .Select(f => f.user_id == currentUserId ? f.friend_id : f.user_id)
                .ToListAsync();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.user_id) &&
                            u.user_id != currentUserId &&
                            !friendsIds.Contains(u.user_id))
                .Select(u => new FriendDTO
                {
                    Id = u.user_id,
                    Name = u.username,
                    Status = "не в сети",
                    PhotoName = u.avatar_url
                })
                .ToListAsync();

            if (!users.Any())
            {
                return NotFound("Пользователи не найдены.");
            }

            return Ok(users);
        }
        [HttpGet("friend-requests")]
        public async Task<IActionResult> GetFriendRequests(int userId)
        {
            var friendRequests = await _context.Friendships
                .Where(f => f.friend_id == userId && f.status == "В ожидании ответа")
                .Select(f => new
                {
                    FriendId = f.friend_id,
                    UserId = f.user_id,
                    UserName = f.user.username,
                    PhotoName = f.user.avatar_url
                })
                .ToListAsync();

            if (!friendRequests.Any())
                return NotFound("Нет заявок в друзья.");

            return Ok(friendRequests);
        }
        [HttpPost("remove")]
        public async Task<IActionResult> RemoveFriend(FriendsRequest request)
        {
            var friendships = await _context.Friendships
                .Where(f => (f.user_id == request.UserId && f.friend_id == request.FriendId) ||
                            (f.user_id == request.FriendId && f.friend_id == request.UserId))
                .ToListAsync();

            if (!friendships.Any())
                return NotFound("Пользователь не найден");

            _context.Friendships.RemoveRange(friendships);
            await _context.SaveChangesAsync();

            var privateChat = await _context.Chats
                .Join(_context.ChatParticipants, c => c.chatid, cp => cp.chatid, (c, cp) => new { Chat = c, Participant = cp })
                .Where(x => x.Chat.chattype == "private" &&
                           (x.Participant.userid == request.UserId || x.Participant.userid == request.FriendId))
                .GroupBy(x => x.Chat.chatid)
                .Where(g => g.Count() == 2)
                .Select(g => g.First().Chat)
                .FirstOrDefaultAsync();

            if (privateChat != null)
            {
                var participants = await _context.ChatParticipants
                    .Where(cp => cp.chatid == privateChat.chatid)
                    .ToListAsync();

                _context.ChatParticipants.RemoveRange(participants);
                _context.Chats.Remove(privateChat);
                await _context.SaveChangesAsync();
            }

            return Ok("Вы больше не друзья...");
        }

        [HttpGet("getPrivateChat/{UserId}/{FriendId}")]
        public async Task<ActionResult> getPrivateChat(int UserId, int FriendId)
        {
            var chat = await _context.Chats
                .Where(c => c.chattype == "private" &&
                            _context.ChatParticipants.Any(cp => cp.chatid == c.chatid && cp.userid == UserId) &&
                            _context.ChatParticipants.Any(cp => cp.chatid == c.chatid && cp.userid == FriendId))
                .FirstOrDefaultAsync();

            if (chat != null)
            {
                return Ok(chat.chatid);
            }

            return NotFound("Приватный чат не найден между этими пользователями.");
        }
        [HttpGet("getUsersParts")]
        public async Task<ActionResult> GetUsersNames([FromQuery] List<int> userIds)
        {
            if (userIds == null || userIds.Count == 0)
                return BadRequest("Список пользователей пуст.");

            var users = await _context.Users
                .Where(u => userIds.Contains(u.user_id))
                .Select(u => new
                {
                    u.user_id,
                    u.username
                })
                .ToListAsync();

            if (users == null || users.Count == 0)
                return NotFound("Пользователи не найдены.");

            return Ok(users);
        }





    }
}
