using CourseManagementMVC.Models;
using CourseManagementMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagementMVC.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApiService _apiService;

        public CoursesController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Courses
        public async Task<IActionResult> Index(string? search, string? mode, string? level, decimal? minPrice, decimal? maxPrice)
        {
            List<CourseViewModel> courses;

            if (!string.IsNullOrWhiteSpace(search) || !string.IsNullOrWhiteSpace(mode) || 
                !string.IsNullOrWhiteSpace(level) || minPrice.HasValue || maxPrice.HasValue)
            {
                courses = await _apiService.SearchCoursesAsync(search, mode, level, minPrice, maxPrice);
            }
            else
            {
                courses = await _apiService.GetCoursesAsync();
            }

            // Lưu filter để hiển thị lại trên view
            ViewBag.SearchKeyword = search;
            ViewBag.SelectedMode = mode;
            ViewBag.SelectedLevel = level;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(courses);
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var course = await _apiService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var contents = await _apiService.GetCourseContentsAsync(id);

            var viewModel = new CourseDetailsViewModel
            {
                Course = course,
                Contents = contents.OrderBy(c => c.OrderIndex).ToList(),
                IsEnrolled = false
            };

            if (User.Identity?.IsAuthenticated == true)
            {
                var myEnrollments = await _apiService.GetMyEnrollmentsAsync();
                var currentEnrollment = myEnrollments.FirstOrDefault(e => e.CourseId == id);
                if (currentEnrollment != null)
                {
                    viewModel.IsEnrolled = true;
                    viewModel.EnrollmentId = currentEnrollment.Id;
                    viewModel.EnrollmentStatus = currentEnrollment.Status;
                }
            }

            return View(viewModel);
        }

        // POST: Courses/Enroll
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var success = await _apiService.EnrollInCourseAsync(courseId);
            if (success)
            {
                TempData["SuccessMessage"] = "Đăng ký khóa học thành công! Bắt đầu học ngay.";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng ký khóa học.";
            }
            return RedirectToAction(nameof(Details), new { id = courseId });
        }

        // POST: Courses/Unenroll
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Unenroll(int enrollmentId, int courseId)
        {
            var success = await _apiService.UnenrollFromCourseAsync(enrollmentId);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã hủy đăng ký khóa học thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy đăng ký khóa học này.";
            }
            return RedirectToAction(nameof(Details), new { id = courseId });
        }

        // ================= ADMIN & INSTRUCTOR MANAGEMENT =================

        // GET: Courses/Create
        [Authorize(Roles = "Admin,Instructor")]
        public IActionResult Create()
        {
            return View(new CourseViewModel { Status = "Draft", LearningMode = "Online" });
        }

        // POST: Courses/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var courseDTO = CreateCourseDto.From(model);
            var success = await _apiService.CreateCourseAsync(courseDTO);
            if (success)
            {
                TempData["SuccessMessage"] = "Thêm khóa học mới thành công!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi lưu khóa học.");
            return View(model);
        }

        // GET: Courses/Edit/5
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _apiService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _apiService.UpdateCourseAsync(id, model);
            if (success)
            {
                TempData["SuccessMessage"] = "Cập nhật khóa học thành công!";
                return RedirectToAction(nameof(Details), new { id });
            }

            ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật khóa học.");
            return View(model);
        }

        // POST: Courses/Delete/5
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _apiService.DeleteCourseAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Xóa khóa học thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa khóa học.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ================= COURSE CONTENT MANAGEMENT =================

        // GET: Courses/ManageContents/5
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> ManageContents(int id)
        {
            var course = await _apiService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var contents = await _apiService.GetCourseContentsAsync(id);
            ViewBag.Course = course;

            return View(contents.OrderBy(c => c.OrderIndex).ToList());
        }

        // GET: Courses/CreateContent?courseId=5
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> CreateContent(int courseId)
        {
            var course = await _apiService.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }
            ViewBag.Course = course;

            // Get next order index
            var contents = await _apiService.GetCourseContentsAsync(courseId);
            var nextIndex = contents.Any() ? contents.Max(c => c.OrderIndex) + 1 : 1;

            return View(new CourseContentViewModel { CourseId = courseId, OrderIndex = nextIndex });
        }

        // POST: Courses/CreateContent
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateContent(CourseContentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var course = await _apiService.GetCourseByIdAsync(model.CourseId);
                ViewBag.Course = course;
                return View(model);
            }

            var success = await _apiService.CreateCourseContentAsync(model.CourseId, model);
            if (success)
            {
                TempData["SuccessMessage"] = "Thêm nội dung bài học thành công!";
                return RedirectToAction(nameof(ManageContents), new { id = model.CourseId });
            }

            ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi lưu nội dung bài học.");
            var c = await _apiService.GetCourseByIdAsync(model.CourseId);
            ViewBag.Course = c;
            return View(model);
        }

        // GET: Courses/EditContent?courseId=5&contentId=10
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> EditContent(int courseId, int contentId)
        {
            var course = await _apiService.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }
            ViewBag.Course = course;

            var contents = await _apiService.GetCourseContentsAsync(courseId);
            var content = contents.FirstOrDefault(c => c.Id == contentId);
            if (content == null)
            {
                return NotFound();
            }

            return View(content);
        }

        // POST: Courses/EditContent
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditContent(int id, CourseContentViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                var course = await _apiService.GetCourseByIdAsync(model.CourseId);
                ViewBag.Course = course;
                return View(model);
            }

            var success = await _apiService.UpdateCourseContentAsync(model.CourseId, model.Id, model);
            if (success)
            {
                TempData["SuccessMessage"] = "Cập nhật bài học thành công!";
                return RedirectToAction(nameof(ManageContents), new { id = model.CourseId });
            }

            ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật bài học.");
            var c = await _apiService.GetCourseByIdAsync(model.CourseId);
            ViewBag.Course = c;
            return View(model);
        }

        // POST: Courses/DeleteContent
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteContent(int courseId, int contentId)
        {
            var success = await _apiService.DeleteCourseContentAsync(courseId, contentId);
            if (success)
            {
                TempData["SuccessMessage"] = "Xóa bài học thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa bài học.";
            }
            return RedirectToAction(nameof(ManageContents), new { id = courseId });
        }

        // GET: Courses/CourseRoster/5
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> CourseRoster(int id)
        {
            var course = await _apiService.GetCourseByIdAsync(id);
            if (course == null) return NotFound();

            var roster = await _apiService.GetCourseRosterAsync(id);
            ViewBag.Course = course;
            return View(roster);
        }
    }
}
