using System.ComponentModel.DataAnnotations;

namespace JwtAuthAPI.Models
{
    // ── Enroll ────────────────────────────────────────────────────────────
    public class EnrollDto
    {
        [Required]
        public int CourseId { get; set; }
    }

    // ── Update Status ─────────────────────────────────────────────────────
    public class UpdateEnrollmentStatusDto
    {
        [Required]
        public EnrollmentStatus Status { get; set; }
    }

    // ── Response ──────────────────────────────────────────────────────────
    public class EnrollmentResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }
        public int CourseId { get; set; }
        public string? CourseTitle { get; set; }
        public string? CourseThumbnailUrl { get; set; }
        public string Status { get; set; } = null!;
        public string StatusDisplay { get; set; } = null!;
        public DateTime EnrolledAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
