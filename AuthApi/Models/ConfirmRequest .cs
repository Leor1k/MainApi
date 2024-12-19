namespace AuthApi.Models
{
    public class ConfirmRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}
