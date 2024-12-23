using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApi.Models
{
    public class Friendships
    {
        [Key]
        public int friendship_id { get; set; }

        public int user_id { get; set; }

        [ForeignKey(nameof(friend_id))]
        public int friend_id { get; set; }

        public string status { get; set; }

        public DateTime? created_at { get; set; } = DateTime.UtcNow;

        [ForeignKey("user_id")]
        public virtual User user { get; set; }

        [ForeignKey("friend_id")]
        public virtual User friend { get; set; }
    }
}
