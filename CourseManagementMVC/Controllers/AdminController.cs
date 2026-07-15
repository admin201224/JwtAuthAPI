using CourseManagementMVC.Models;
using CourseManagementMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagementMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApiService _apiService;

        public AdminController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _apiService.GetAllUsersAsync();
            return View(users);
        }

        // POST: Admin/DeleteUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _apiService.DeleteUserAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Xóa người dùng thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa người dùng.";
            }
            return RedirectToAction(nameof(Users));
        }

        // GET: Admin/Create
        public IActionResult Create()
        {
            return View(new CreateUserViewModel());
        }

        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _apiService.CreateUserAsync(model);
            if (success)
            {
                TempData["SuccessMessage"] = "Tạo người dùng thành công!";
                return RedirectToAction(nameof(Users));
            }

            TempData["ErrorMessage"] = "Có lỗi xảy ra, tên đăng nhập có thể đã tồn tại.";
            return View(model);
        }

        // GET: Admin/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _apiService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(Users));
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role
            };

            return View(model);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _apiService.UpdateUserAsync(id, model);
            if (success)
            {
                TempData["SuccessMessage"] = "Cập nhật người dùng thành công!";
                return RedirectToAction(nameof(Users));
            }

            TempData["ErrorMessage"] = "Có lỗi xảy ra, tên đăng nhập có thể đã tồn tại.";
            return View(model);
        }
    }
}
