using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CourseManagementMVC.Pages
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AdminDashboardModel> _logger;

        public AdminDashboardModel(HttpClient httpClient, ILogger<AdminDashboardModel> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public AdminDashboardData Dashboard { get; set; } = new();
        public bool IsLoading { get; set; } = true;
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var token = User.FindFirst("access_token")?.Value ?? "";

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("https://localhost:7001/api/dashboard/admin/stats");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Dashboard = System.Text.Json.JsonSerializer.Deserialize<AdminDashboardData>(content,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
                else
                {
                    ErrorMessage = "Không thể tải dữ liệu bảng điều khiển";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading admin dashboard: {ex.Message}");
                ErrorMessage = "Đã xảy ra lỗi khi tải bảng điều khiển";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public class AdminDashboardData
    {
        public string Message { get; set; } = "";
        public AdminStats Statistics { get; set; } = new();
    }

    public class AdminStats
    {
        public int TotalTeachers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public List<ContentStatistic> MostDownloadedContents { get; set; } = new();
        public List<CourseStatistic> CourseStatistics { get; set; } = new();
    }

    public class ContentStatistic
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int DownloadCount { get; set; }
    }

    public class CourseStatistic
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int EnrollmentCount { get; set; }
        public int ContentCount { get; set; }
    }
}