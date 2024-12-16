using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models
{
    public class User
    {
        [Key]  // Это явно указывает, что user_id - это первичный ключ
        public int user_id { get; set; }
        public string users_name { get; set; }
        public string users_email { get; set; }
        public string users_password { get; set; }
    }
}
