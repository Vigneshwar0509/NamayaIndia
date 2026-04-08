using System.ComponentModel.DataAnnotations;

namespace Namaya.Model
{
    public class Achievement
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Level { get; set; } // International, National, South India, State, etc.
        public string Year { get; set; }
        public string? Highlight { get; set; } // e.g., Gold Medal
        public string? ImageUrl { get; set; }  // Stores path: "/achievements/uuid.jpg"
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class AchievementDTO
    {
        public string Title { get; set; }
        public string Level { get; set; }
        public string Year { get; set; }
        public string? Highlight { get; set; }
        public IFormFile? ImageFile { get; set; } // Receives the file from React
    }
}