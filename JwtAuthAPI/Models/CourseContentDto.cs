using System.ComponentModel.DataAnnotations;

namespace JwtAuthAPI.Models
{
    // ── Create ────────────────────────────────────────────────────────────
    public class CreateContentDto
    {
        [Required(ErrorMessage = "Tiêu đề bài học là bắt buộc")]
        [MaxLength(300)]
        public string Title { get; set; } = null!;

        public ContentType ContentType { get; set; } = ContentType.Lecture;

        public string? Body { get; set; }

        [Url(ErrorMessage = "VideoUrl phải là URL hợp lệ")]
        public string? VideoUrl { get; set; }

        [Range(0, 9999)]
        public int OrderIndex { get; set; } = 0;

        public bool IsPreview { get; set; } = false;
    }

    // ── Update ────────────────────────────────────────────────────────────
    public class UpdateContentDto
    {
        [MaxLength(300)]
        public string? Title { get; set; }

        public ContentType? ContentType { get; set; }

        public string? Body { get; set; }

        [Url(ErrorMessage = "VideoUrl phải là URL hợp lệ")]
        public string? VideoUrl { get; set; }

        [Range(0, 9999)]
        public int? OrderIndex { get; set; }

        public bool? IsPreview { get; set; }
    }

    // ── Response ──────────────────────────────────────────────────────────
    public class ContentResponseDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public string ContentTypeDisplay { get; set; } = null!;
        public string? Body { get; set; }
        public string? VideoUrl { get; set; }
        public int OrderIndex { get; set; }
        public bool IsPreview { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int CreatedByUserId { get; set; }
        public string? CreatedByUsername { get; set; }
    }
}
