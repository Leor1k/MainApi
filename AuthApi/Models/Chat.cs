using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models
{
    public class Chat
    {
        [Key]
        public int chatid { get; set; }
        public string chatname { get; set; }
        public string chattype { get; set; }
        public DateTime? createdat { get; set; } = DateTime.UtcNow;
    }
}
