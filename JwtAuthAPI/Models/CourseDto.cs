using System.ComponentModel.DataAnnotations;

namespace JwtAuthAPI.Models
{
    /// <summary>
    /// DTO để tạo khóa học mới
    /// </summary>
    public class CreateCourseDto
    {
        [Required(ErrorMessage = "Tên khóa học là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tên khóa học không vượt quá 200 ký tự")]
        public string Title { get; set; } = null!;

        [MaxLength(2000, ErrorMessage = "Mô tả không vượt quá 2000 ký tự")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Học phí phải >= 0")]
        public decimal Price { get; set; } = 0;

        [Url(ErrorMessage = "ThumbnailUrl phải là URL hợp lệ")]
        public string? ThumbnailUrl { get; set; }

        public LearningMode LearningMode { get; set; } = LearningMode.Online;

        [Range(1, 9999, ErrorMessage = "Số giờ học phải từ 1 đến 9999")]
        public int? DurationHours { get; set; }

        public CourseLevel Level { get; set; } = CourseLevel.Beginner;
    }

    /// <summary>
    /// DTO để cập nhật khóa học (tất cả trường đều optional)
    /// </summary>
    public class UpdateCourseDto
    {
        [MaxLength(200, ErrorMessage = "Tên khóa học không vượt quá 200 ký tự")]
        public string? Title { get; set; }

        [MaxLength(2000, ErrorMessage = "Mô tả không vượt quá 2000 ký tự")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Học phí phải >= 0")]
        public decimal? Price { get; set; }

        [Url(ErrorMessage = "ThumbnailUrl phải là URL hợp lệ")]
        public string? ThumbnailUrl { get; set; }

        public LearningMode? LearningMode { get; set; }

        [Range(1, 9999, ErrorMessage = "Số giờ học phải từ 1 đến 9999")]
        public int? DurationHours { get; set; }

        public CourseLevel? Level { get; set; }
    }

    /// <summary>
    /// DTO để đổi trạng thái khóa học (Publish / Archive)
    /// </summary>
    public class UpdateCourseStatusDto
    {
        [Required]
        public CourseStatus Status { get; set; }
    }

    /// <summary>
    /// DTO trả về client — không lộ internal fields
    /// </summary>
    public class CourseResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string LearningMode { get; set; } = null!;   // Tên enum dạng string
        public string LearningModeDisplay { get; set; } = null!; // Tên hiển thị tiếng Việt
        public int? DurationHours { get; set; }
        public string Level { get; set; } = null!;
        public string LevelDisplay { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string StatusDisplay { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int CreatedByUserId { get; set; }
        public string? CreatedByUsername { get; set; }
    }
}
