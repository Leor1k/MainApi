namespace AuthApi.Models.Requests
{
    public class FriendSignalR
    {
        int FriendId { get; set; }
        int UserId { get; set; }
        string? UserName { get; set; }
        string? PhotoName { get; set; }
        public FriendSignalR(int friendId, int userId, string? userName, string? photoName)
        {
            FriendId = friendId;
            UserId = userId;
            UserName = userName;
            PhotoName = photoName;
        }
    }
}
