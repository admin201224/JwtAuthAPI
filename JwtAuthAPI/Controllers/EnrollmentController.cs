using JwtAuthAPI.Models;
using JwtAuthAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ILogger<EnrollmentController> _logger;

        public EnrollmentController(IEnrollmentService enrollmentService, ILogger<EnrollmentController> logger)
        {
            _enrollmentService = enrollmentService;
            _logger = logger;
        }

        private int GetUserId() =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id) ? id : 0;

        private string GetUserRole() =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        // ── POST /api/enrollment ──────────────────────────────────────────
        /// <summary>Đăng ký học viên vào khóa học (bất kỳ user đã đăng nhập)</summary>
        [HttpPost]
        public async Task<IActionResult> Enroll([FromBody] EnrollDto dto)
        {
            var userId = GetUserId();
            var enrollment = await _enrollmentService.EnrollAsync(userId, dto.CourseId);

            if (enrollment is null)
                return Conflict(new { message = "Bạn đã đăng ký khóa học này rồi hoặc khóa học không tồn tại" });

            _logger.LogInformation("User {UserId} enrolled in Course {CourseId}", userId, dto.CourseId);
            return CreatedAtAction(nameof(GetMyEnrollments), null, new
            {
                message    = "Đăng ký khóa học thành công",
                enrollment
            });
        }

        // ── DELETE /api/enrollment/{id} ───────────────────────────────────
        /// <summary>Hủy đăng ký. Học viên hủy của mình; Admin hủy bất kỳ.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Unenroll(int id)
        {
            var result = await _enrollmentService.UnenrollAsync(id, GetUserId(), GetUserRole());
            if (!result)
                return StatusCode(403, new { message = "Không tìm thấy hoặc bạn không có quyền hủy đăng ký này" });

            return Ok(new { message = "Hủy đăng ký thành công" });
        }

        // ── PATCH /api/enrollment/{id}/status ─────────────────────────────
        /// <summary>Cập nhật trạng thái học: NotStarted / InProgress / Completed</summary>
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateEnrollmentStatusDto dto)
        {
            var enrollment = await _enrollmentService.UpdateStatusAsync(id, dto.Status, GetUserId(), GetUserRole());
            if (enrollment is null)
                return StatusCode(403, new { message = "Không tìm thấy hoặc bạn không có quyền cập nhật" });

            return Ok(new
            {
                message    = $"Cập nhật trạng thái → {enrollment.StatusDisplay}",
                enrollment
            });
        }

        // ── GET /api/enrollment/my ────────────────────────────────────────
        /// <summary>Danh sách khóa học đã đăng ký của user hiện tại</summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyEnrollments()
        {
            var userId = GetUserId();
            var enrollments = await _enrollmentService.GetMyEnrollmentsAsync(userId);
            return Ok(new
            {
                message     = "Danh sách khóa học đã đăng ký",
                count       = enrollments.Count,
                enrollments
            });
        }

        // ── GET /api/enrollment/course/{courseId} ─────────────────────────
        /// <summary>Danh sách học viên trong một khóa học (Admin / Instructor)</summary>
        [HttpGet("course/{courseId:int}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            var enrollments = await _enrollmentService.GetByCourseAsync(courseId);
            return Ok(new
            {
                message     = $"Danh sách học viên của khóa {courseId}",
                count       = enrollments.Count,
                enrollments
            });
        }
    }
}
