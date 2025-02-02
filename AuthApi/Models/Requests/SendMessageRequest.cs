namespace AuthApi.Models.Requests
{
    public class SendMessageRequest
    {
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Content { get; set; } 
    }
}
