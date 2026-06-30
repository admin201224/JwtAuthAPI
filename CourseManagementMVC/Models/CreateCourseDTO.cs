using System.ComponentModel.DataAnnotations;

namespace CourseManagementMVC.Models
{
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

        // --- Static Factory Mapping Method ---
        public static CreateCourseDto From(CourseViewModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            return new CreateCourseDto
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                ThumbnailUrl = string.IsNullOrWhiteSpace(model.ThumbnailUrl) ? null : model.ThumbnailUrl,
                DurationHours = model.DurationHours,

                // Inline independent enum parsing with fallback defaults
                LearningMode = Enum.TryParse(model.LearningMode, true, out LearningMode lm) ? lm : LearningMode.Online,
                Level = Enum.TryParse(model.Level, true, out CourseLevel cl) ? cl : CourseLevel.Beginner
            };
        }
    }

    public class UpdateCourseDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? ThumbnailUrl { get; set; }
        public LearningMode? LearningMode { get; set; }
        public int? DurationHours { get; set; }
        public CourseLevel? Level { get; set; }

        public static UpdateCourseDto From(CourseViewModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            return new UpdateCourseDto
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                ThumbnailUrl = string.IsNullOrWhiteSpace(model.ThumbnailUrl) ? null : model.ThumbnailUrl,
                DurationHours = model.DurationHours,
                LearningMode = Enum.TryParse(model.LearningMode, true, out LearningMode lm) ? lm : (LearningMode?)null,
                Level = Enum.TryParse(model.Level, true, out CourseLevel cl) ? cl : (CourseLevel?)null
            };
        }
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
    /// Hình thức học của khóa học
    /// </summary>
    public enum LearningMode
    {
        Online = 0,     // Học trực tuyến hoàn toàn
        Offline = 1,    // Học trực tiếp tại cơ sở
        Hybrid = 2,     // Kết hợp online + offline
        SelfPaced = 3   // Tự học theo tiến độ cá nhân
    }
}
