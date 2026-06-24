namespace JwtAuthAPI.Models
{
    /// <summary>Trạng thái học tập của học viên trong khóa học</summary>
    public enum EnrollmentStatus
    {
        NotStarted = 0,  // Chưa bắt đầu
        InProgress = 1,  // Đang học
        Completed  = 2   // Đã hoàn thành
    }

    public class Enrollment
    {
        public int Id { get; set; }

        /// <summary>FK — Học viên</summary>
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        /// <summary>FK — Khóa học được đăng ký</summary>
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        /// <summary>Trạng thái học: NotStarted / InProgress / Completed</summary>
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.NotStarted;

        /// <summary>Ngày đăng ký</summary>
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        /// <summary>Ngày cập nhật trạng thái gần nhất</summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
