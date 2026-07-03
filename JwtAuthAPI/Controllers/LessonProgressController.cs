using JwtAuthAPI.Models;
using JwtAuthAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtAuthAPI.Controllers
{
    [ApiController]
    [Route("api/lesson-progress")]
    [Authorize]
    public class LessonProgressController : ControllerBase
    {
        private readonly ILessonProgressService _progressService;
        private readonly ILogger<LessonProgressController> _logger;

        public LessonProgressController(ILessonProgressService progressService, ILogger<LessonProgressController> logger)
        {
            _progressService = progressService;
            _logger = logger;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }

        /// <summary>L?y ho?c t?o ti?n ð? cho m?t bài h?c</summary>
        [HttpGet("get-or-create/{contentId}/{courseId}")]
        public async Task<IActionResult> GetOrCreateProgress(int contentId, int courseId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized();

            var progress = await _progressService.GetOrCreateProgressAsync(userId, contentId, courseId);
            if (progress == null)
                return NotFound();

            return Ok(new { success = true, data = progress });
        }

        /// <summary>C?p nh?t ti?n ð? bài h?c (ðánh d?u hoàn thành)</summary>
        [HttpPost("update/{contentId}")]
        public async Task<IActionResult> UpdateProgress(int contentId, [FromBody] UpdateLessonProgressDto dto)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized();

            var progress = await _progressService.UpdateProgressAsync(userId, contentId, dto);
            if (progress == null)
                return NotFound();

            return Ok(new { success = true, data = progress });
        }

        /// <summary>L?y ti?n ð? toàn khóa h?c (progress bar)</summary>
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetCourseProgress(int courseId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized();

            var progress = await _progressService.GetCourseProgressAsync(userId, courseId);
            if (progress == null)
                return NotFound();

            return Ok(new { success = true, data = progress });
        }

        /// <summary>L?y ti?n ð? t?t c? bài h?c trong khóa</summary>
        [HttpGet("course/{courseId}/lessons")]
        public async Task<IActionResult> GetLessonProgresses(int courseId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized();

            var progresses = await _progressService.GetLessonProgressesByCourseAsync(userId, courseId);
            return Ok(new { success = true, data = progresses });
        }
    }
}
