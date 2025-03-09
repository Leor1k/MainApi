using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models
{
    public class User
    {
        [Key]  
        public int user_id { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string password_hash { get; set; }
        public string avatar_url { get; set; }
        public string status { get; set; }
        public DateTime? createdat { get; set; } = DateTime.UtcNow;
        public bool isconfirmed { get; set; } = false;
        public string? confirmationcode { get; set; }

    }
}
