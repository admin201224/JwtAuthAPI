using JwtAuthAPI.Data;
using JwtAuthAPI.Data;
using JwtAuthAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthAPI.Services
{
    public interface IContentUploadService
    {
        Task<ContentUploadResponseDto> UploadContentAsync(int courseId, int userId, IFormFile file, ContentUploadDto dto);
        Task<List<ContentUploadResponseDto>> GetCourseContentsAsync(int courseId);
        Task<bool> DeleteContentAsync(int contentId, int userId);
        Task<Stream> DownloadContentAsync(int contentId);
        Task<DashboardStatisticsDto> GetAdminDashboardStatsAsync();
        Task<TeacherDashboardDto> GetTeacherDashboardAsync(int userId);
    }

    public class ContentUploadService : IContentUploadService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContentUploadService> _logger;
        private readonly string _uploadsDirectory;

        public ContentUploadService(ApplicationDbContext context, ILogger<ContentUploadService> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _uploadsDirectory = Path.Combine(env.WebRootPath, "uploads", "course-contents");

            // Create directory if it doesn't exist
            if (!Directory.Exists(_uploadsDirectory))
            {
                Directory.CreateDirectory(_uploadsDirectory);
            }
        }

        public async Task<ContentUploadResponseDto> UploadContentAsync(int courseId, int userId, IFormFile file, ContentUploadDto dto)
        {
            try
            {
                // Verify course exists and user is instructor
                var course = await _context.Courses.FindAsync(courseId);
                if (course == null)
                {
                    throw new InvalidOperationException("Course not found");
                }

                var user = await _context.Users.FindAsync(userId);
                if (user?.Role != "Instructor" && user?.Role != "Admin")
                {
                    throw new UnauthorizedAccessException("Only instructors can upload content");
                }

                // Validate file
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".txt", ".xlsx" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    throw new InvalidOperationException("File type not allowed");
                }

                if (file.Length > 50 * 1024 * 1024) // 50MB limit
                {
                    throw new InvalidOperationException("File size exceeds 50MB limit");
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(_uploadsDirectory, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create database record
                var content = new CourseContent
                {
                    CourseId = courseId,
                    Title = dto.Title,
                    Description = dto.Description,
                    ContentType = fileExtension.TrimStart('.').ToUpper(),
                    FilePath = fileName,
                    FileSize = file.Length,
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CourseContents.Add(content);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Content uploaded: {content.Id} by user {userId}");

                return MapToContentResponseDto(content, user.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading content: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ContentUploadResponseDto>> GetCourseContentsAsync(int courseId)
        {
            var contents = await _context.CourseContents
                .Where(c => c.CourseId == courseId)
                .Include(c => c.CreatedByUser)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return contents.Select(c => MapToContentResponseDto(c, c.CreatedByUser.Username)).ToList();
        }

        public async Task<bool> DeleteContentAsync(int contentId, int userId)
        {
            var content = await _context.CourseContents.FindAsync(contentId);
            if (content == null) return false;

            // Verify user is instructor/admin
            var user = await _context.Users.FindAsync(userId);
            if (user?.Role != "Instructor" && user?.Role != "Admin")
            {
                throw new UnauthorizedAccessException("Only instructors can delete content");
            }

            try
            {
                // Delete file
                var filePath = Path.Combine(_uploadsDirectory, content.FilePath);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Delete database record
                _context.CourseContents.Remove(content);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting content: {ex.Message}");
                return false;
            }
        }

        public async Task<Stream> DownloadContentAsync(int contentId)
        {
            var content = await _context.CourseContents.FindAsync(contentId);
            if (content == null)
            {
                throw new FileNotFoundException("Content not found");
            }

            // Increment download count
            content.DownloadCount++;
            await _context.SaveChangesAsync();

            var filePath = Path.Combine(_uploadsDirectory, content.FilePath);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found on server");
            }

            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        public async Task<DashboardStatisticsDto> GetAdminDashboardStatsAsync()
        {
            var stats = new DashboardStatisticsDto
            {
                TotalTeachers = await _context.Users.CountAsync(u => u.Role == "Instructor"),
                TotalStudents = await _context.Users.CountAsync(u => u.Role == "Student"),
                TotalCourses = await _context.Courses.CountAsync(),
                TotalEnrollments = await _context.CourseEnrollments.CountAsync(),
                MostDownloadedContents = await _context.CourseContents
                    .Include(c => c.Course)
                    .OrderByDescending(c => c.DownloadCount)
                    .Take(10)
                    .Select(c => new ContentStatisticDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        CourseName = c.Course.Title,
                        DownloadCount = c.DownloadCount
                    })
                    .ToListAsync(),
                CourseStatistics = await _context.Courses
                    .Include(c => c.Contents)
                    .Select(c => new CourseStatisticDto
                    {
                        Id = c.Id,
                        Name = c.Title,
                        EnrollmentCount = c.Enrollments.Count,
                        ContentCount = c.Contents.Count
                    })
                    .ToListAsync()
            };

            return stats;
        }

        public async Task<TeacherDashboardDto> GetTeacherDashboardAsync(int userId)
        {
            var dashboard = new TeacherDashboardDto();

            // Get all courses taught by this instructor
            var courses = await _context.Courses
                .Where(c => c.CreatedByUserId == userId)
                .Include(c => c.Contents)
                .Include(c => c.Enrollments)
                .ToListAsync();

            dashboard.TotalContentsUploaded = courses.Sum(c => c.Contents.Count);
            dashboard.TotalDownloads = courses.SelectMany(c => c.Contents).Sum(c => c.DownloadCount);

            dashboard.Courses = courses.Select(c => new CourseWithContentDto
            {
                Id = c.Id,
                Name = c.Title,
                Description = c.Description,
                StudentCount = c.Enrollments.Count,
                Contents = c.Contents.Select(ct => MapToContentResponseDto(ct, ct.CreatedByUser?.Username ?? "Unknown")).ToList()
            }).ToList();

            return dashboard;
        }

        private ContentUploadResponseDto MapToContentResponseDto(CourseContent content, string username)
        {
            return new ContentUploadResponseDto
            {
                Id = content.Id,
                CourseId = content.CourseId,
                Title = content.Title,
                Description = content.Description,
                ContentType = content.ContentType,
                FileSize = content.FileSize,
                DownloadCount = content.DownloadCount,
                CreatedAt = content.CreatedAt,
                CreatedByUserName = username
            };
        }
    }
}