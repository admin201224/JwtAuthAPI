using System.ComponentModel.DataAnnotations;

namespace CourseManagementMVC.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [Display(Name = "Tên đăng nhập")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập từ 3 đến 50 ký tự")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không trùng khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Vai trò")]
        public string Role { get; set; } = "Student";
    }

    public class CourseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [Display(Name = "Tiêu đề khóa học")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Học phí là bắt buộc")]
        [Range(0, 100000000, ErrorMessage = "Học phí từ 0đ trở lên")]
        [Display(Name = "Học phí (VND)")]
        public decimal Price { get; set; }

        [Display(Name = "Đường dẫn ảnh đại diện (Thumbnail)")]
        public string ThumbnailUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hình thức học là bắt buộc")]
        [Display(Name = "Hình thức học")]
        public string LearningMode { get; set; } = string.Empty; // ví dụ: Online, Offline, Hybrid

        [Required(ErrorMessage = "Thời lượng học là bắt buộc")]
        [Range(1, 1000, ErrorMessage = "Thời lượng từ 1 giờ trở lên")]
        [Display(Name = "Thời lượng (giờ)")]
        public int DurationHours { get; set; }

        [Required(ErrorMessage = "Cấp độ là bắt buộc")]
        [Display(Name = "Cấp độ")]
        public string Level { get; set; } = string.Empty; // ví dụ: Beginner, Intermediate, Advanced

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Active";

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime UpdatedAt { get; set; }

        public int CreatedByUserId { get; set; }
        public string? CreatedByUsername { get; set; }
    }

    public class CourseContentViewModel
    {
        public int Id { get; set; }
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Tiêu đề bài học là bắt buộc")]
        [Display(Name = "Tiêu đề bài học")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại nội dung là bắt buộc")]
        [Display(Name = "Loại bài học")]
        public string ContentType { get; set; } = string.Empty; // ví dụ: Lecture, Video, Quiz, Document

        [Display(Name = "Nội dung bài học")]
        public string Body { get; set; } = string.Empty;

        [Display(Name = "Đường dẫn Video")]
        public string VideoUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thứ tự bài học là bắt buộc")]
        [Display(Name = "Thứ tự sắp xếp")]
        public int OrderIndex { get; set; }

        [Display(Name = "Học thử miễn phí?")]
        public bool IsPreview { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime UpdatedAt { get; set; }
    }

    public class EnrollmentViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string CourseThumbnailUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // NotStarted, InProgress, Completed
        public DateTime EnrolledAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CourseDetailsViewModel
    {
        public CourseViewModel Course { get; set; } = null!;
        public List<CourseContentViewModel> Contents { get; set; } = new();
        public bool IsEnrolled { get; set; }
        public int? EnrollmentId { get; set; }
        public string? EnrollmentStatus { get; set; } // NotStarted, InProgress, Completed
    }
}
