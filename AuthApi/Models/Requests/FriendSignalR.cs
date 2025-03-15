namespace AuthApi.Models.Requests
{
    public class FriendSignalR
    {
        public int FriendId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string PhotoName { get; set; }

        public FriendSignalR(int friendId, int userId, string userName, string photoName)
        {
            FriendId = friendId;
            UserId = userId;
            UserName = userName;
            PhotoName = photoName;
        }
    }
}