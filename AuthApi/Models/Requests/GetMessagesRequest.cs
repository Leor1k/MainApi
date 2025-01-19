namespace AuthApi.Models.Requests
{
    public class GetMessagesRequest
    {
        public int ChatId { get; set; }
        public int UserId { get; set; }
    }
}
