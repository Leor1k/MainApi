namespace AuthApi.Models.Requests
{
    public class ChangeNameRequest
    {
        public int UserUd {  get; set; }
        public string NewUserName { get; set; }
    }
}
