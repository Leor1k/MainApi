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
        [HttpGet("{userId}/list)")]
        public async Task<ActionResult> GetFriends (int userIdIn)
        {
            var frieds = await _context.Friendships.Where(f => f.user_Id == userIdIn && f.status == "Принятый").Select(f => new
            {
                FriendId = f.friend_Id,
                FriendName = f.friend.users_name,
                FriendStatus = "Не в сети"
            }).ToListAsync();
            return Ok(frieds);
        }
        [HttpPost("request/send")]
        public async Task<IActionResult> SendFriendRequest(FriendsRequest request)
        {
            var existingRequest = await _context.Friendships
                .FirstOrDefaultAsync(f => f.user_Id == request.UserId && f.friend_Id == request.FriendId);

            if (existingRequest != null)
                return BadRequest("Вы уже друзья!");

            var friendRequest = new Friendships
            {
                user_Id = request.UserId,
                friend_Id = request.FriendId,
                status = "В процессе"
            };

            _context.Friendships.Add(friendRequest);
            await _context.SaveChangesAsync();

            return Ok("Заявка в друзья отправлена");
        }
        [HttpPost("request/accept")]
        public async Task<IActionResult> AcceptFriendRequest(FriendsRequest request)
        {
            var friendRequest = await _context.Friendships
                .FirstOrDefaultAsync(f => f.user_Id == request.FriendId && f.friend_Id == request.UserId && f.status == "В процессе");

            if (friendRequest == null)
                return NotFound("Пользователя ненайдено");

            friendRequest.status = "Принятый";
            _context.Friendships.Update(friendRequest);

            var reverseFriendship = new Friendships
            {
                user_Id = request.UserId,
                friend_Id = request.FriendId,
                status = "Accepted"
            };
            _context.Friendships.Add(reverseFriendship);

            await _context.SaveChangesAsync();

            return Ok("Вы стали друзьями!");
        }
        [HttpPost("remove")]
        public async Task<IActionResult> RemoveFriend(FriendsRequest request)
        {
            var friendships = await _context.Friendships
                .Where(f => (f.user_Id == request.UserId && f.friend_Id == request.FriendId) ||
                            (f.user_Id == request.FriendId && f.friend_Id == request.UserId))
                .ToListAsync();

            if (!friendships.Any())
                return NotFound("Пользователя не найдено");

            _context.Friendships.RemoveRange(friendships);
            await _context.SaveChangesAsync();

            return Ok("Вы больше не друзья....");
        }
    }
}
