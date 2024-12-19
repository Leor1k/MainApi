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
        public bool isConfirmed { get; set; } = false;
        public string? confirmationCode { get; set; }
        public DateTime? createdAt { get; set; } = DateTime.UtcNow;

    }
}
