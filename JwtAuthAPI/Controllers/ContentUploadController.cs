using JwtAuthAPI.Models;
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
    public class ContentUploadController : ControllerBase
    {
        private readonly IContentUploadService _contentUploadService;
        private readonly ILogger<ContentUploadController> _logger;

        public ContentUploadController(IContentUploadService contentUploadService, ILogger<ContentUploadController> logger)
        {
            _contentUploadService = contentUploadService;
            _logger = logger;
        }

        private int GetUserId() =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id) ? id : 0;

        /// <summary>
        /// Upload course content (PDF, Word, etc.)
        /// </summary>
        [HttpPost("upload/{courseId:int}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> UploadContent(
            int courseId,
            [FromForm] IFormFile file,
            [FromForm] ContentUploadDto dto)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "File is required" });
                }

                var userId = GetUserId();
                var result = await _contentUploadService.UploadContentAsync(courseId, userId, file, dto);

                return Ok(new
                {
                    message = "Content uploaded successfully",
                    content = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Upload error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all contents for a course
        /// </summary>
        [HttpGet("course/{courseId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourseContents(int courseId)
        {
            try
            {
                var contents = await _contentUploadService.GetCourseContentsAsync(courseId);
                return Ok(new
                {
                    message = "Contents retrieved successfully",
                    count = contents.Count,
                    contents
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete content
        /// </summary>
        [HttpDelete("{contentId:int}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> DeleteContent(int contentId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _contentUploadService.DeleteContentAsync(contentId, userId);

                if (!result)
                {
                    return NotFound(new { message = "Content not found" });
                }

                return Ok(new { message = "Content deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Download content file
        /// </summary>
        [HttpGet("download/{contentId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadContent(int contentId)
        {
            try
            {
                var stream = await _contentUploadService.DownloadContentAsync(contentId);
                return File(stream, "application/octet-stream", $"content_{contentId}");
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}