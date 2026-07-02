using JwtAuthAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IContentUploadService _contentUploadService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IContentUploadService contentUploadService, ILogger<DashboardController> logger)
        {
            _contentUploadService = contentUploadService;
            _logger = logger;
        }

        private int GetUserId() =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id) ? id : 0;

        /// <summary>
        /// Get admin dashboard statistics
        /// </summary>
        [HttpGet("admin/stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminStats()
        {
            try
            {
                var stats = await _contentUploadService.GetAdminDashboardStatsAsync();
                return Ok(new
                {
                    message = "Admin statistics retrieved successfully",
                    statistics = stats
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get teacher dashboard
        /// </summary>
        [HttpGet("teacher")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetTeacherDashboard()
        {
            try
            {
                var userId = GetUserId();
                var dashboard = await _contentUploadService.GetTeacherDashboardAsync(userId);
                return Ok(new
                {
                    message = "Teacher dashboard retrieved successfully",
                    dashboard
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}