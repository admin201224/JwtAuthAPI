using CourseManagementMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagementMVC.Controllers
{
    [Authorize]
    public class MyCoursesController : Controller
    {
        private readonly ApiService _apiService;

        public MyCoursesController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: MyCourses
        public async Task<IActionResult> Index()
        {
            var enrollments = await _apiService.GetMyEnrollmentsAsync();
            return View(enrollments);
        }

        // POST: MyCourses/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int enrollmentId, string status, string redirectUrl)
        {
            var success = await _apiService.UpdateEnrollmentStatusAsync(enrollmentId, status);
            if (success)
            {
                TempData["SuccessMessage"] = "Cập nhật tiến trình học thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể cập nhật tiến trình học.";
            }

            if (!string.IsNullOrEmpty(redirectUrl) && Url.IsLocalUrl(redirectUrl))
            {
                return Redirect(redirectUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: MyCourses/Study/5
        public async Task<IActionResult> Study(int id)
        {
            var course = await _apiService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            // Get enrollment to see status and update if it's NotStarted
            var myEnrollments = await _apiService.GetMyEnrollmentsAsync();
            var enrollment = myEnrollments.FirstOrDefault(e => e.CourseId == id);
            if (enrollment == null)
            {
                TempData["ErrorMessage"] = "Bạn chưa đăng ký khóa học này.";
                return RedirectToAction("Details", "Courses", new { id });
            }

            // If enrollment is "Chưa bắt đầu" (NotStarted), update it to "Đang học" (InProgress)
            if (enrollment.Status == "NotStarted" || enrollment.Status == "Chưa bắt đầu")
            {
                await _apiService.UpdateEnrollmentStatusAsync(enrollment.Id, "InProgress");
                enrollment.Status = "InProgress";
            }

            var contents = await _apiService.GetCourseContentsAsync(id);
            
            ViewBag.Course = course;
            ViewBag.Enrollment = enrollment;

            return View(contents.OrderBy(c => c.OrderIndex).ToList());
        }
    }
}
