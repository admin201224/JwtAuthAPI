using JwtAuthAPI.Models;
using JwtAuthAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly ILogger<CourseController> _logger;

        public CourseController(ICourseService courseService, ILogger<CourseController> logger)
        {
            _courseService = courseService;
            _logger = logger;
        }

        // ── Helper lấy thông tin user từ JWT ─────────────────────────────
        private int GetUserId() =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id) ? id : 0;

        private string GetUserRole() =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        // ── GET /api/course ───────────────────────────────────────────────
        /// <summary>
        /// Lấy danh sách khóa học (Public).
        /// Filter: mode (Online/Offline/Hybrid/SelfPaced), status (Draft/Published/Archived), level (Beginner/Intermediate/Advanced)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(
            [FromQuery] LearningMode? mode = null,
            [FromQuery] CourseStatus? status = null,
            [FromQuery] CourseLevel? level = null)
        {
            var courses = await _courseService.GetAllAsync(mode, status, level);
            return Ok(new
            {
                message = "Lấy danh sách khóa học thành công",
                count   = courses.Count,
                filters = new { mode = mode?.ToString(), status = status?.ToString(), level = level?.ToString() },
                courses
            });
        }

        // ── GET /api/course/{id} ──────────────────────────────────────────
        /// <summary>
        /// Lấy chi tiết một khóa học theo ID (Public)
        /// </summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var course = await _courseService.GetByIdAsync(id);
            if (course is null)
                return NotFound(new { message = $"Không tìm thấy khóa học với ID = {id}" });

            return Ok(new { message = "Lấy thông tin khóa học thành công", course });
        }

        // ── POST /api/course ──────────────────────────────────────────────
        /// <summary>
        /// Tạo khóa học mới (Admin hoặc Instructor)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Create([FromBody] CreateCourseDto dto)
        {
            var userId = GetUserId();
            _logger.LogInformation("User {UserId} [{Role}] creating course: {Title}", userId, GetUserRole(), dto.Title);

            var course = await _courseService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = course.Id }, new
            {
                message = "Tạo khóa học thành công",
                course
            });
        }

        // ── PUT /api/course/{id} ──────────────────────────────────────────
        /// <summary>
        /// Cập nhật thông tin khóa học.
        /// Admin cập nhật được tất cả; Instructor chỉ cập nhật khóa học của mình.
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseDto dto)
        {
            var userId = GetUserId();
            var role   = GetUserRole();

            var exists = await _courseService.ExistsAsync(id);
            if (!exists)
                return NotFound(new { message = $"Không tìm thấy khóa học với ID = {id}" });

            var course = await _courseService.UpdateAsync(id, dto, userId, role);
            if (course is null)
                return StatusCode(403, new { message = "Bạn chỉ có thể chỉnh sửa khóa học của mình" });

            _logger.LogInformation("Course {CourseId} updated by User {UserId}", id, userId);
            return Ok(new { message = "Cập nhật khóa học thành công", course });
        }

        // ── DELETE /api/course/{id} ───────────────────────────────────────
        /// <summary>
        /// Xóa khóa học (Admin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var adminId = GetUserId();
            _logger.LogInformation("Admin {UserId} deleting course {CourseId}", adminId, id);

            var result = await _courseService.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = $"Không tìm thấy khóa học với ID = {id}" });

            return Ok(new { message = "Xóa khóa học thành công" });
        }

        // ── PATCH /api/course/{id}/status ─────────────────────────────────
        /// <summary>
        /// Đổi trạng thái khóa học: Draft → Published → Archived.
        /// Admin thay đổi được tất cả; Instructor chỉ thay đổi khóa học của mình.
        /// </summary>
        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateCourseStatusDto dto)
        {
            var userId = GetUserId();
            var role   = GetUserRole();

            var exists = await _courseService.ExistsAsync(id);
            if (!exists)
                return NotFound(new { message = $"Không tìm thấy khóa học với ID = {id}" });

            var course = await _courseService.UpdateStatusAsync(id, dto.Status, userId, role);
            if (course is null)
                return StatusCode(403, new { message = "Bạn chỉ có thể thay đổi trạng thái khóa học của mình" });

            _logger.LogInformation("Course {CourseId} status → {Status} by User {UserId}", id, dto.Status, userId);
            return Ok(new { message = $"Đổi trạng thái thành công → {course.StatusDisplay}", course });
        }
    }
}
