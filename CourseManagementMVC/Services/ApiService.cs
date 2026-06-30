using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection.Emit;
using System.Text.Json;
using CourseManagementMVC.Models;

namespace CourseManagementMVC.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient client, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var baseUrl = configuration["BackendUrl"] ?? "https://localhost:7195/";
            _client.BaseAddress = new Uri(baseUrl);
        }

        private void AddAuthHeader()
        {
            var context = _httpContextAccessor.HttpContext;
            var token = context?.User.FindFirst("AccessToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _client.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<string?> LoginAsync(string username, string password)
        {
            var response = await _client.PostAsJsonAsync("api/authorize/login", new { username, password });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (result.TryGetProperty("accessToken", out var tokenProp))
                {
                    return tokenProp.GetString();
                }
            }
            return null;
        }

        public async Task<bool> RegisterAsync(string username, string email, string password, string role)
        {
            var response = await _client.PostAsJsonAsync("api/authorize/register", new { username, email, password, role });
            return response.IsSuccessStatusCode;
        }

        // COURSES
        public async Task<List<CourseViewModel>> GetCoursesAsync()
        {
            AddAuthHeader();
            var response = await _client.GetAsync("api/course");
            if (response.IsSuccessStatusCode)
            {
                var wrapper = await response.Content.ReadFromJsonAsync<CoursesWrapper>(_jsonOptions);
                return wrapper?.Courses ?? new List<CourseViewModel>();
            }
            return new List<CourseViewModel>();
        }

        public async Task<CourseViewModel?> GetCourseByIdAsync(int id)
        {
            AddAuthHeader();
            var response = await _client.GetAsync($"api/course/{id}");
            if (response.IsSuccessStatusCode)
            {
                var wrapper = await response.Content.ReadFromJsonAsync<CourseWrapper>(_jsonOptions);
                return wrapper?.Course;
            }
            return null;
        }

        public async Task<bool> CreateCourseAsync(CreateCourseDto course)
        {
            AddAuthHeader();

            var response = await _client.PostAsJsonAsync("api/course", course);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateCourseAsync(int id, CourseViewModel course)
        {
            AddAuthHeader();
            var dto = UpdateCourseDto.From(course);
            var response = await _client.PutAsJsonAsync($"api/course/{id}", dto, _jsonOptions);
            if (!response.IsSuccessStatusCode) return false;

            // Cập nhật Status riêng qua PATCH endpoint nếu có
            if (!string.IsNullOrEmpty(course.Status))
            {
                await _client.PatchAsJsonAsync($"api/course/{id}/status",
                    new { status = course.Status }, _jsonOptions);
            }

            return true;
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            AddAuthHeader();
            var response = await _client.DeleteAsync($"api/course/{id}");
            return response.IsSuccessStatusCode;
        }

        // COURSE CONTENTS
        public async Task<List<CourseContentViewModel>> GetCourseContentsAsync(int courseId)
        {
            AddAuthHeader();
            var response = await _client.GetAsync($"api/course/{courseId}/contents");
            if (response.IsSuccessStatusCode)
            {
                var wrapper = await response.Content.ReadFromJsonAsync<ContentsWrapper>(_jsonOptions);
                return wrapper?.Contents ?? new List<CourseContentViewModel>();
            }
            return new List<CourseContentViewModel>();
        }

        public async Task<bool> CreateCourseContentAsync(int courseId, CourseContentViewModel content)
        {
            AddAuthHeader();
            var response = await _client.PostAsJsonAsync($"api/course/{courseId}/contents", content, _jsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateCourseContentAsync(int courseId, int contentId, CourseContentViewModel content)
        {
            AddAuthHeader();
            var response = await _client.PutAsJsonAsync($"api/course/{courseId}/contents/{contentId}", content, _jsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCourseContentAsync(int courseId, int contentId)
        {
            AddAuthHeader();
            var response = await _client.DeleteAsync($"api/course/{courseId}/contents/{contentId}");
            return response.IsSuccessStatusCode;
        }

        // ENROLLMENTS
        public async Task<List<EnrollmentViewModel>> GetMyEnrollmentsAsync()
        {
            AddAuthHeader();
            var response = await _client.GetAsync("api/enrollment/my");
            if (response.IsSuccessStatusCode)
            {
                var wrapper = await response.Content.ReadFromJsonAsync<EnrollmentsWrapper>(_jsonOptions);
                return wrapper?.Enrollments ?? new List<EnrollmentViewModel>();
            }
            return new List<EnrollmentViewModel>();
        }

        public async Task<bool> EnrollInCourseAsync(int courseId)
        {
            AddAuthHeader();
            var response = await _client.PostAsJsonAsync("api/enrollment", new { courseId });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UnenrollFromCourseAsync(int enrollmentId)
        {
            AddAuthHeader();
            var response = await _client.DeleteAsync($"api/enrollment/{enrollmentId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateEnrollmentStatusAsync(int enrollmentId, string status)
        {
            AddAuthHeader();
            var response = await _client.PatchAsJsonAsync($"api/enrollment/{enrollmentId}/status", new { status });
            return response.IsSuccessStatusCode;
        }

        public async Task<List<EnrollmentViewModel>> GetCourseRosterAsync(int courseId)
        {
            AddAuthHeader();
            var response = await _client.GetAsync($"api/enrollment/course/{courseId}");
            if (response.IsSuccessStatusCode)
            {
                var wrapper = await response.Content.ReadFromJsonAsync<EnrollmentsWrapper>(_jsonOptions);
                return wrapper?.Enrollments ?? new List<EnrollmentViewModel>();
            }
            return new List<EnrollmentViewModel>();
        }

        // USERS
        public async Task<List<UserViewModel>> GetAllUsersAsync()
        {
            AddAuthHeader();
            var response = await _client.GetAsync("api/user");
            if (response.IsSuccessStatusCode)
            {
                var wrapper = await response.Content.ReadFromJsonAsync<UsersWrapper>(_jsonOptions);
                return wrapper?.Users ?? new List<UserViewModel>();
            }
            return new List<UserViewModel>();
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            AddAuthHeader();
            var response = await _client.DeleteAsync($"api/user/{id}");
            return response.IsSuccessStatusCode;
        }

        // Wrapper helper classes
        private class CoursesWrapper
        {
            public List<CourseViewModel> Courses { get; set; } = new();
        }

        private class CourseWrapper
        {
            public CourseViewModel Course { get; set; } = null!;
        }

        private class ContentsWrapper
        {
            public List<CourseContentViewModel> Contents { get; set; } = new();
        }

        private class EnrollmentsWrapper
        {
            public List<EnrollmentViewModel> Enrollments { get; set; } = new();
        }

        private class UsersWrapper
        {
            public List<UserViewModel> Users { get; set; } = new();
        }
    }
}
