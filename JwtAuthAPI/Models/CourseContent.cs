namespace JwtAuthAPI.Models
{
    /// <summary>Loại nội dung bài học</summary>
    public enum ContentType
    {
        Lecture  = 0,   // Bài giảng text
        Video    = 1,   // Video bài học
        Quiz     = 2,   // Bài kiểm tra
        Document = 3    // Tài liệu đính kèm
    }

    public class CourseContent
    {
        public int Id { get; set; }

        /// <summary>FK — Thuộc khóa học nào</summary>
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        /// <summary>Tiêu đề bài học</summary>
        public string Title { get; set; } = null!;

        /// <summary>Loại nội dung</summary>
        public ContentType ContentType { get; set; } = ContentType.Lecture;

        /// <summary>Nội dung text (cho Lecture / Document)</summary>
        public string? Body { get; set; }

        /// <summary>URL video (cho loại Video)</summary>
        public string? VideoUrl { get; set; }

        /// <summary>Thứ tự hiển thị trong khóa học (tăng dần)</summary>
        public int OrderIndex { get; set; } = 0;

        /// <summary>Bài học này có được xem thử miễn phí không?</summary>
        public bool IsPreview { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        /// <summary>FK — Người tạo bài học</summary>
        public int CreatedByUserId { get; set; }
        public User CreatedBy { get; set; } = null!;
    }
}
