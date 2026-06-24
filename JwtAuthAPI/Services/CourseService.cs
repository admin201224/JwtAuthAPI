using JwtAuthAPI.Data;
using JwtAuthAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthAPI.Services
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CourseService> _logger;

        public CourseService(ApplicationDbContext db, ILogger<CourseService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ── Helper: Map entity → ResponseDto ─────────────────────────────
        private static CourseResponseDto MapToDto(Course c) => new()
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            Price = c.Price,
            ThumbnailUrl = c.ThumbnailUrl,
            LearningMode = c.LearningMode.ToString(),
            LearningModeDisplay = c.LearningMode switch
            {
                LearningMode.Online    => "Trực tuyến",
                LearningMode.Offline   => "Trực tiếp",
                LearningMode.Hybrid    => "Kết hợp",
                LearningMode.SelfPaced => "Tự học",
                _                     => c.LearningMode.ToString()
            },
            DurationHours = c.DurationHours,
            Level = c.Level.ToString(),
            LevelDisplay = c.Level switch
            {
                CourseLevel.Beginner     => "Cơ bản",
                CourseLevel.Intermediate => "Trung cấp",
                CourseLevel.Advanced     => "Nâng cao",
                _                        => c.Level.ToString()
            },
            Status = c.Status.ToString(),
            StatusDisplay = c.Status switch
            {
                CourseStatus.Draft     => "Bản nháp",
                CourseStatus.Published => "Đã công khai",
                CourseStatus.Archived  => "Lưu trữ",
                _                      => c.Status.ToString()
            },
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            CreatedByUserId = c.CreatedByUserId,
            CreatedByUsername = c.CreatedBy?.Username
        };

        // ── GetAll ────────────────────────────────────────────────────────
        public async Task<List<CourseResponseDto>> GetAllAsync(
            LearningMode? mode = null,
            CourseStatus? status = null,
            CourseLevel? level = null)
        {
            var query = _db.Courses
                           .Include(c => c.CreatedBy)
                           .AsNoTracking()
                           .AsQueryable();

            if (mode.HasValue)
                query = query.Where(c => c.LearningMode == mode.Value);

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            if (level.HasValue)
                query = query.Where(c => c.Level == level.Value);

            var courses = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return courses.Select(MapToDto).ToList();
        }

        // ── GetById ───────────────────────────────────────────────────────
        public async Task<CourseResponseDto?> GetByIdAsync(int id)
        {
            var course = await _db.Courses
                                  .Include(c => c.CreatedBy)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync(c => c.Id == id);

            return course is null ? null : MapToDto(course);
        }

        // ── Create ────────────────────────────────────────────────────────
        public async Task<CourseResponseDto> CreateAsync(CreateCourseDto dto, int createdByUserId)
        {
            var course = new Course
            {
                Title          = dto.Title,
                Description    = dto.Description,
                Price          = dto.Price,
                ThumbnailUrl   = dto.ThumbnailUrl,
                LearningMode   = dto.LearningMode,
                DurationHours  = dto.DurationHours,
                Level          = dto.Level,
                Status         = CourseStatus.Draft,
                CreatedAt      = DateTime.UtcNow,
                CreatedByUserId = createdByUserId
            };

            _db.Courses.Add(course);
            await _db.SaveChangesAsync();

            // Load navigation để trả về username
            await _db.Entry(course)
                     .Reference(c => c.CreatedBy)
                     .LoadAsync();

            _logger.LogInformation("Course {CourseId} created by User {UserId}", course.Id, createdByUserId);
            return MapToDto(course);
        }

        // ── Update ────────────────────────────────────────────────────────
        public async Task<CourseResponseDto?> UpdateAsync(int id, UpdateCourseDto dto, int userId, string role)
        {
            var course = await _db.Courses
                                  .Include(c => c.CreatedBy)
                                  .FirstOrDefaultAsync(c => c.Id == id);

            if (course is null) return null;

            // Instructor chỉ được sửa khóa học của mình
            if (role == "Instructor" && course.CreatedByUserId != userId)
                return null;

            if (dto.Title is not null)         course.Title         = dto.Title;
            if (dto.Description is not null)   course.Description   = dto.Description;
            if (dto.Price.HasValue)            course.Price         = dto.Price.Value;
            if (dto.ThumbnailUrl is not null)  course.ThumbnailUrl  = dto.ThumbnailUrl;
            if (dto.LearningMode.HasValue)     course.LearningMode  = dto.LearningMode.Value;
            if (dto.DurationHours.HasValue)    course.DurationHours = dto.DurationHours.Value;
            if (dto.Level.HasValue)            course.Level         = dto.Level.Value;

            course.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Course {CourseId} updated by User {UserId}", id, userId);
            return MapToDto(course);
        }

        // ── UpdateStatus ──────────────────────────────────────────────────
        public async Task<CourseResponseDto?> UpdateStatusAsync(int id, CourseStatus newStatus, int userId, string role)
        {
            var course = await _db.Courses
                                  .Include(c => c.CreatedBy)
                                  .FirstOrDefaultAsync(c => c.Id == id);

            if (course is null) return null;

            // Instructor chỉ được đổi trạng thái khóa học của mình
            if (role == "Instructor" && course.CreatedByUserId != userId)
                return null;

            course.Status    = newStatus;
            course.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Course {CourseId} status changed to {Status} by User {UserId}", id, newStatus, userId);
            return MapToDto(course);
        }

        // ── Delete ────────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(int id)
        {
            var course = await _db.Courses.FindAsync(id);
            if (course is null) return false;

            _db.Courses.Remove(course);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Course {CourseId} deleted", id);
            return true;
        }

        // ── Exists ────────────────────────────────────────────────────────
        public async Task<bool> ExistsAsync(int id)
            => await _db.Courses.AnyAsync(c => c.Id == id);
    }
}
