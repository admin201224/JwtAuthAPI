namespace JwtAuthAPI.Models
{
    public class ContentUploadDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
    }

    public class ContentUploadResponseDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public string ContentType { get; set; } = null!;
        public long FileSize { get; set; }
        public int DownloadCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; } = null!;
    }

    public class DashboardStatisticsDto
    {
        public int TotalTeachers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public List<ContentStatisticDto> MostDownloadedContents { get; set; } = new();
        public List<CourseStatisticDto> CourseStatistics { get; set; } = new();
    }

    public class ContentStatisticDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public int DownloadCount { get; set; }
    }

    public class CourseStatisticDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int EnrollmentCount { get; set; }
        public int ContentCount { get; set; }
    }

    public class TeacherDashboardDto
    {
        public List<CourseWithContentDto> Courses { get; set; } = new();
        public int TotalContentsUploaded { get; set; }
        public int TotalDownloads { get; set; }
    }

    public class CourseWithContentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public List<ContentUploadResponseDto> Contents { get; set; } = new();
    }
}
