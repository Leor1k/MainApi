namespace AuthApi.Models.Requests
{
    public class CreateChatRequest
    {
        public int UserId1 { get; set; }
        public int UserId2 { get; set; }
    }
    public class CreateGroupChat
    {
        public int CreatorID { get; set; }
        public int[] UsersId { get; set; }
    }
}
