using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models
{
    public class Messages
    {
        [Key]
        public int messageid { get; set; }
        public int chatid { get; set; }
        public int senderid { get; set; }
        public string? content { get; set; }
        public string? messagetype { get; set; }
        public DateTime? createdat { get; set; } = DateTime.UtcNow;

        public virtual Chat? Chat { get; set; }   // Навигационное свойство для чата
        public virtual User? Sender { get; set; } // Навигационное свойство для пользователя
    }
}
