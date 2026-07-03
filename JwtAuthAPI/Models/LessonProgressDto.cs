namespace JwtAuthAPI.Models
{
    public class LessonProgressDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ContentId { get; set; }
        public int CourseId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int ProgressPercentage { get; set; }
    }

    public class UpdateLessonProgressDto
    {
        public int ContentId { get; set; }
        public bool IsCompleted { get; set; }
        public int? ProgressPercentage { get; set; }
    }

    public class CourseProgressDto
    {
        public int CourseId { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int ProgressPercentage { get; set; }
        public List<LessonProgressDto> LessonProgresses { get; set; } = new();
    }
}
