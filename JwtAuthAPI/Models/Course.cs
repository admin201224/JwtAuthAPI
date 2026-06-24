namespace JwtAuthAPI.Models
{
    /// <summary>
    /// Hình thức học của khóa học
    /// </summary>
    public enum LearningMode
    {
        Online = 0,     // Học trực tuyến hoàn toàn
        Offline = 1,    // Học trực tiếp tại cơ sở
        Hybrid = 2,     // Kết hợp online + offline
        SelfPaced = 3   // Tự học theo tiến độ cá nhân
    }

    /// <summary>
    /// Trình độ yêu cầu của khóa học
    /// </summary>
    public enum CourseLevel
    {
        Beginner = 0,
        Intermediate = 1,
        Advanced = 2
    }

    /// <summary>
    /// Trạng thái của khóa học
    /// </summary>
    public enum CourseStatus
    {
        Draft = 0,      // Bản nháp, chưa công khai
        Published = 1,  // Đã công khai
        Archived = 2    // Đã lưu trữ / ngừng hoạt động
    }

    public class Course
    {
        public int Id { get; set; }

        /// <summary>Tên khóa học</summary>
        public string Title { get; set; } = null!;

        /// <summary>Mô tả ngắn về khóa học</summary>
        public string? Description { get; set; }

        /// <summary>Học phí (0 = miễn phí)</summary>
        public decimal Price { get; set; } = 0;

        /// <summary>URL ảnh bìa khóa học</summary>
        public string? ThumbnailUrl { get; set; }

        /// <summary>Hình thức học: Online / Offline / Hybrid / SelfPaced</summary>
        public LearningMode LearningMode { get; set; } = LearningMode.Online;

        /// <summary>Tổng số giờ học (ước tính)</summary>
        public int? DurationHours { get; set; }

        /// <summary>Trình độ yêu cầu: Beginner / Intermediate / Advanced</summary>
        public CourseLevel Level { get; set; } = CourseLevel.Beginner;

        /// <summary>Trạng thái: Draft / Published / Archived</summary>
        public CourseStatus Status { get; set; } = CourseStatus.Draft;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        /// <summary>FK — Người tạo khóa học</summary>
        public int CreatedByUserId { get; set; }
        public User CreatedBy { get; set; } = null!;

        // Navigation properties
        public ICollection<CourseContent> Contents { get; set; } = new List<CourseContent>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
