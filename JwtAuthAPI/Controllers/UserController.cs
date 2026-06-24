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
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation($"Admin {userId} requested all users");

            var users = await _userService.GetAllUsersAsync();
            return Ok(new
            {
                message = "Users retrieved successfully",
                count = users.Count,
                users
            });
        }

        /// <summary>
        /// Get user by ID (Own profile or Admin)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // Allow if own profile or admin
            if (currentUserId != id.ToString() && role != "Admin")
                return Forbid("You can only view your own profile");

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new
            {
                message = "User retrieved successfully",
                user
            });
        }

        /// <summary>
        /// Create new user (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation($"Admin {adminId} creating new user: {dto.Username}");

            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Username and password are required");

            var user = await _userService.CreateUserAsync(dto);
            if (user == null)
                return BadRequest("Username already exists");

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, new
            {
                message = "User created successfully",
                user
            });
        }

        /// <summary>
        /// Update user (Own profile or Admin)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // Allow if own profile or admin
            if (currentUserId != id.ToString() && role != "Admin")
                return Forbid("You can only update your own profile");

            var userExists = await _userService.UserExistsAsync(id);
            if (!userExists)
                return NotFound(new { message = "User not found" });

            var user = await _userService.UpdateUserAsync(id, dto);
            if (user == null)
                return BadRequest("Username already exists or invalid data");

            _logger.LogInformation($"User {id} updated");

            return Ok(new
            {
                message = "User updated successfully",
                user
            });
        }

        /// <summary>
        /// Delete user (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation($"Admin {adminId} deleting user {id}");

            var result = await _userService.DeleteUserAsync(id);
            if (!result)
                return NotFound(new { message = "User not found" });

            return Ok(new { message = "User deleted successfully" });
        }

        /// <summary>
        /// Get current user info
        /// </summary>
        [HttpGet("me/profile")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userId, out int id))
                return BadRequest("Invalid user id");

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(new
            {
                message = "Current user profile",
                user
            });
        }
    }
}
