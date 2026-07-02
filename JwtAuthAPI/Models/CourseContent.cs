namespace JwtAuthAPI.Models
{
    public class CourseContent
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public string ContentType { get; set; } = null!; // PDF, Word, Video, etc.
        public string FilePath { get; set; } = null!;
        public long FileSize { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int DownloadCount { get; set; } = 0;

        // Navigation properties
        public Course Course { get; set; } = null!;
        public User CreatedByUser { get; set; } = null!;
    }

    public class CourseEnrollment
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int UserId { get; set; }
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Active"; // Active, Completed, Dropped

        // Navigation properties
        public Course Course { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
