using AuthApi.Data;
using AuthApi.Models;
using Microsoft.AspNetCore.Mvc;
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
        [HttpGet("{userId}/list")]
        public async Task<ActionResult> GetFriends(int userIdIn)
        {
            Console.WriteLine($"Запрос на друзей для пользователя с ID: {userIdIn}");
            var friends = await _context.Friendships
                .Where(f => (f.user_id == userIdIn || f.friend_id == userIdIn) && f.status.Trim() == "В друзьях")
                .Select(f => new
                {
                    FriendId = f.user_id == userIdIn ? f.friend_id : f.user_id,
                    FriendName = f.user_id == userIdIn ? f.friend.users_name : f.user.users_name,
                    FriendStatus = "Не в сети"
                })
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
        public async Task<IActionResult> SendFriendRequest(FriendsRequest request)
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
                status = "В друзьях"
            };
            _context.Friendships.Add(reverseFriendship);

            await _context.SaveChangesAsync();

            return Ok("Вы стали друзьями!");
        }
        [HttpPost("remove")]
        public async Task<IActionResult> RemoveFriend(FriendsRequest request)
        {
            var friendships = await _context.Friendships
                .Where(f => (f.user_id == request.UserId && f.friend_id == request.FriendId) ||
                            (f.user_id == request.FriendId && f.friend_id == request.UserId))
                .ToListAsync();

            if (!friendships.Any())
                return NotFound("Пользователя не найдено");

            _context.Friendships.RemoveRange(friendships);
            await _context.SaveChangesAsync();

            return Ok("Вы больше не друзья....");
        }
    }
}
