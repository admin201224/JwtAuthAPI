using JwtAuthAPI.Models;
using JwtAuthAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtAuthAPI.Controllers
{
    /// <summary>
    /// Quản lý nội dung (bài học) của một khóa học.
    /// Route: /api/course/{courseId}/contents
    /// </summary>
    [Route("api/course/{courseId:int}/contents")]
    [ApiController]
    public class CourseContentController : ControllerBase
    {
        private readonly ICourseContentService _contentService;
        private readonly ICourseService _courseService;
        private readonly ILogger<CourseContentController> _logger;

        public CourseContentController(
            ICourseContentService contentService,
            ICourseService courseService,
            ILogger<CourseContentController> logger)
        {
            _contentService = contentService;
            _courseService  = courseService;
            _logger         = logger;
        }

        private int GetUserId() =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id) ? id : 0;

        private string GetUserRole() =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        // ── GET /api/course/{courseId}/contents ───────────────────────────
        /// <summary>Lấy danh sách bài học của khóa, sắp xếp theo thứ tự (Public)</summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(int courseId)
        {
            var courseExists = await _courseService.ExistsAsync(courseId);
            if (!courseExists)
                return NotFound(new { message = $"Không tìm thấy khóa học ID = {courseId}" });

            var contents = await _contentService.GetByCourseIdAsync(courseId);
            return Ok(new
            {
                message  = "Lấy danh sách bài học thành công",
                courseId,
                count    = contents.Count,
                contents
            });
        }

        // ── GET /api/course/{courseId}/contents/{id} ──────────────────────
        /// <summary>Chi tiết một bài học (Public)</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int courseId, int id)
        {
            var content = await _contentService.GetByIdAsync(courseId, id);
            if (content is null)
                return NotFound(new { message = $"Không tìm thấy bài học ID = {id} trong khóa {courseId}" });

            return Ok(new { message = "Lấy thông tin bài học thành công", content });
        }

        // ── POST /api/course/{courseId}/contents ──────────────────────────
        /// <summary>Thêm bài học vào khóa (Admin / Instructor)</summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Create(int courseId, [FromBody] CreateContentDto dto)
        {
            var courseExists = await _courseService.ExistsAsync(courseId);
            if (!courseExists)
                return NotFound(new { message = $"Không tìm thấy khóa học ID = {courseId}" });

            var content = await _contentService.CreateAsync(courseId, dto, GetUserId());
            return CreatedAtAction(nameof(GetById), new { courseId, id = content.Id }, new
            {
                message = "Thêm bài học thành công",
                content
            });
        }

        // ── PUT /api/course/{courseId}/contents/{id} ──────────────────────
        /// <summary>Sửa bài học (Admin sửa tất cả; Instructor chỉ sửa của mình)</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Update(int courseId, int id, [FromBody] UpdateContentDto dto)
        {
            var content = await _contentService.UpdateAsync(courseId, id, dto, GetUserId(), GetUserRole());
            if (content is null)
                return StatusCode(403, new { message = "Không tìm thấy hoặc bạn không có quyền sửa bài học này" });

            return Ok(new { message = "Cập nhật bài học thành công", content });
        }

        // ── DELETE /api/course/{courseId}/contents/{id} ───────────────────
        /// <summary>Xóa bài học (Admin / Instructor — chỉ bài của mình)</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Delete(int courseId, int id)
        {
            var result = await _contentService.DeleteAsync(courseId, id, GetUserId(), GetUserRole());
            if (!result)
                return StatusCode(403, new { message = "Không tìm thấy hoặc bạn không có quyền xóa bài học này" });

            return Ok(new { message = "Xóa bài học thành công" });
        }
    }
}
