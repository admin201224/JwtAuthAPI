using JwtAuthAPI.Data;
using JwtAuthAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthAPI.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<EnrollmentService> _logger;

        public EnrollmentService(ApplicationDbContext db, ILogger<EnrollmentService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ── Helper: Map → DTO ─────────────────────────────────────────────
        private static EnrollmentResponseDto MapToDto(Enrollment e) => new()
        {
            Id               = e.Id,
            UserId           = e.UserId,
            Username         = e.User?.Username,
            CourseId         = e.CourseId,
            CourseTitle      = e.Course?.Title,
            CourseThumbnailUrl = e.Course?.ThumbnailUrl,
            Status           = e.Status.ToString(),
            StatusDisplay    = e.Status switch
            {
                EnrollmentStatus.NotStarted => "Chưa bắt đầu",
                EnrollmentStatus.InProgress => "Đang học",
                EnrollmentStatus.Completed  => "Đã hoàn thành",
                _                           => e.Status.ToString()
            },
            EnrolledAt = e.EnrolledAt,
            UpdatedAt  = e.UpdatedAt
        };

        // ── Enroll ────────────────────────────────────────────────────────
        public async Task<EnrollmentResponseDto?> EnrollAsync(int userId, int courseId)
        {
            // Kiểm tra khóa học tồn tại
            var courseExists = await _db.Courses.AnyAsync(c => c.Id == courseId);
            if (!courseExists) return null;

            // Kiểm tra đã đăng ký chưa
            var exists = await _db.Enrollments
                                  .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
            if (exists) return null;

            var enrollment = new Enrollment
            {
                UserId     = userId,
                CourseId   = courseId,
                Status     = EnrollmentStatus.NotStarted,
                EnrolledAt = DateTime.UtcNow
            };

            _db.Enrollments.Add(enrollment);
            await _db.SaveChangesAsync();

            // Load navigation để trả về đầy đủ
            await _db.Entry(enrollment).Reference(e => e.User).LoadAsync();
            await _db.Entry(enrollment).Reference(e => e.Course).LoadAsync();

            _logger.LogInformation("User {UserId} enrolled in Course {CourseId}", userId, courseId);
            return MapToDto(enrollment);
        }

        // ── Unenroll ──────────────────────────────────────────────────────
        public async Task<bool> UnenrollAsync(int enrollmentId, int userId, string role)
        {
            var enrollment = await _db.Enrollments.FindAsync(enrollmentId);
            if (enrollment is null) return false;

            // Chỉ chính học viên hoặc Admin mới được hủy
            if (role != "Admin" && enrollment.UserId != userId)
                return false;

            _db.Enrollments.Remove(enrollment);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Enrollment {EnrollmentId} removed by User {UserId}", enrollmentId, userId);
            return true;
        }

        // ── UpdateStatus ──────────────────────────────────────────────────
        public async Task<EnrollmentResponseDto?> UpdateStatusAsync(
            int enrollmentId, EnrollmentStatus status, int userId, string role)
        {
            var enrollment = await _db.Enrollments
                                      .Include(e => e.User)
                                      .Include(e => e.Course)
                                      .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment is null) return null;

            // Chỉ chính học viên hoặc Admin mới cập nhật được
            if (role != "Admin" && enrollment.UserId != userId)
                return null;

            enrollment.Status    = status;
            enrollment.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Enrollment {EnrollmentId} status → {Status} by User {UserId}",
                enrollmentId, status, userId);
            return MapToDto(enrollment);
        }

        // ── GetMyEnrollments ──────────────────────────────────────────────
        public async Task<List<EnrollmentResponseDto>> GetMyEnrollmentsAsync(int userId)
        {
            var enrollments = await _db.Enrollments
                                       .Include(e => e.User)
                                       .Include(e => e.Course)
                                       .Where(e => e.UserId == userId)
                                       .OrderByDescending(e => e.EnrolledAt)
                                       .AsNoTracking()
                                       .ToListAsync();

            return enrollments.Select(MapToDto).ToList();
        }

        // ── GetByCourse ───────────────────────────────────────────────────
        public async Task<List<EnrollmentResponseDto>> GetByCourseAsync(int courseId)
        {
            var enrollments = await _db.Enrollments
                                       .Include(e => e.User)
                                       .Include(e => e.Course)
                                       .Where(e => e.CourseId == courseId)
                                       .OrderByDescending(e => e.EnrolledAt)
                                       .AsNoTracking()
                                       .ToListAsync();

            return enrollments.Select(MapToDto).ToList();
        }

        // ── IsEnrolled ────────────────────────────────────────────────────
        public async Task<bool> IsEnrolledAsync(int userId, int courseId)
            => await _db.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
    }
}
