using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models
{
    public class ChatParticipant
    {
        [Key]
        public int participantid { get; set; }
        public int chatid { get; set; }
        public int userid { get; set; }
        public string role { get; set; }
        public DateTime? joinedat { get; set; } = DateTime.UtcNow;
    }
}
