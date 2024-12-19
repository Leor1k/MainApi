using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models
{
    public class User
    {
        [Key]  
        public int user_id { get; set; }
        public string users_name { get; set; }
        public string users_email { get; set; }
        public string users_password { get; set; }
        public bool isconfirmed { get; set; } = false;
        public string? confirmationcode { get; set; }
        public DateTime? createdat { get; set; } = DateTime.UtcNow;

    }
}
