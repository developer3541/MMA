namespace WebApplication1.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class MemberStreak
    {
        [Key]
        public int MemberId { get; set; }

        public int CurrentStreak { get; set; }
        public int BestStreak { get; set; }

        [Column(TypeName = "date")]
        public DateTime? LastSessionDate { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public MemberProfile Member { get; set; }
    }


}
