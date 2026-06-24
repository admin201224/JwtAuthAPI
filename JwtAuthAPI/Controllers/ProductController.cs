using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;

        public ProductController(ILogger<ProductController> logger)
        {
            _logger = logger;
        }

        [HttpGet("list")]
        public IActionResult GetProducts()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            _logger.LogInformation($"User {username} (ID: {userId}) requested products");

            var products = new[]
            {
                new { id = 1, name = "Product 1", price = 100 },
                new { id = 2, name = "Product 2", price = 200 },
                new { id = 3, name = "Product 3", price = 300 }
            };

            return Ok(new
            {
                message = $"Hello {username}! Here are your products.",
                products
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var product = new { id, name = $"Product {id}", price = id * 100 };
            return Ok(new
            {
                message = $"Product retrieved for user {userId}",
                product
            });
        }

        [HttpGet("admin-only")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnly()
        {
            return Ok(new { message = "This is admin-only content" });
        }
    }
}
