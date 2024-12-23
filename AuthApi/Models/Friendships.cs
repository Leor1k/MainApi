using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApi.Models
{
    public class Friendships
    {
        [Key]
        public int friendship_id { get; set; }

        public int user_Id { get; set; }

        [ForeignKey(nameof(friend_Id))]
        public int friend_Id { get; set; }

        public string status { get; set; }

        public DateTime? createdat { get; set; } = DateTime.UtcNow;

        [ForeignKey("user_Id")]
        public virtual User user { get; set; }

        [ForeignKey("friend_Id")]
        public virtual User friend { get; set; }
    }
}
