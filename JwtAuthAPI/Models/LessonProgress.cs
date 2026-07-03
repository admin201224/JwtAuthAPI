namespace JwtAuthAPI.Models
{
    /// <summary>
    /// Theo d?i ti?n ð? h?c c?a t?ng h?c viên v?i t?ng bài h?c
    /// </summary>
    public class LessonProgress
    {
        public int Id { get; set; }

        /// <summary>FK — H?c viên</summary>
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        /// <summary>FK — Bài h?c trong khóa</summary>
        public int ContentId { get; set; }
        public CourseContent Content { get; set; } = null!;

        /// <summary>FK — Khóa h?c ch?a bài này</summary>
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        /// <summary>Bài h?c này ð? hoàn thành chýa</summary>
        public bool IsCompleted { get; set; } = false;

        /// <summary>Ngày b?t ð?u xem bài</summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>Ngày ðánh d?u hoàn thành</summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>Ti?n ð? xem (0-100%)</summary>
        public int ProgressPercentage { get; set; } = 0;
    }
}
