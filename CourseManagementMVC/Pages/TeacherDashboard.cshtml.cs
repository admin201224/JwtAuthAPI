using CourseManagementMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace CourseManagementMVC.Pages
{
    [Authorize(Roles = "Instructor")]
    public class TeacherDashboardModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TeacherDashboardModel> _logger;

        public TeacherDashboardModel(HttpClient httpClient, ILogger<TeacherDashboardModel> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public DashboardContent Dashboard { get; set; } = new();
        public bool IsLoading { get; set; } = true;
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var token = User.FindFirst("access_token")?.Value ?? "";

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("https://localhost:7001/api/dashboard/teacher");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    // Parse JSON and populate Dashboard
                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<DashboardData>(content,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    Dashboard = apiResponse?.Dashboard ?? new();
                }
                else
                {
                    ErrorMessage = "Failed to load dashboard data";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading teacher dashboard: {ex.Message}");
                ErrorMessage = "An error occurred while loading dashboard";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public class DashboardData
    {
        public string Message { get; set; } = "";
        public DashboardContent Dashboard { get; set; } = new();
    }

    public class DashboardContent
    {
        public List<CourseInfo> Courses { get; set; } = new();
        public int TotalContentsUploaded { get; set; }
        public int TotalDownloads { get; set; }
    }

    public class CourseInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int StudentCount { get; set; }
        public List<ContentInfo> Contents { get; set; } = new();
    }

    public class ContentInfo
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string ContentType { get; set; } = "";
        public long FileSize { get; set; }
        public int DownloadCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; } = "";
    }
}