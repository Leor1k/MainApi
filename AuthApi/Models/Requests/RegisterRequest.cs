namespace AuthApi.Models.Requests
{
    public class RegisterRequest
    {
        public string users_name { get; set; }
        public string users_email { get; set; }
        public string users_password { get; set; }
    }
}
