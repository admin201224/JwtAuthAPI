using JwtAuthAPI.Data;
using JwtAuthAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthAPI.Services
{
    public class CourseContentService : ICourseContentService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CourseContentService> _logger;

        public CourseContentService(ApplicationDbContext db, ILogger<CourseContentService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ── Helper: Map → DTO ─────────────────────────────────────────────
        private static ContentResponseDto MapToDto(CourseContent c) => new()
        {
            Id          = c.Id,
            CourseId    = c.CourseId,
            Title       = c.Title,
            ContentType = c.ContentType,
            ContentTypeDisplay = c.ContentType switch
            {
                "PDF"   => "PDF",
                "DOCX"  => "Word",
                "PPTX"  => "PowerPoint",
                "VIDEO" => "Video",
                _       => c.ContentType
            },
            Body              = c.Description,
            VideoUrl          = null,
            OrderIndex        = 0,
            IsPreview         = false,
            CreatedAt         = c.CreatedAt,
            UpdatedAt         = c.UpdatedAt,
            CreatedByUserId   = c.CreatedByUserId,
            CreatedByUsername = c.CreatedByUser?.Username
        };

        // ── GetByCourseId ─────────────────────────────────────────────────
        public async Task<List<ContentResponseDto>> GetByCourseIdAsync(int courseId)
        {
            var contents = await _db.CourseContents
                                    .Include(c => c.CreatedByUser)
                                    .Where(c => c.CourseId == courseId)
                                    .AsNoTracking()
                                    .ToListAsync();

            return contents.Select(MapToDto).ToList();
        }

        // ── GetById ───────────────────────────────────────────────────────
        public async Task<ContentResponseDto?> GetByIdAsync(int courseId, int contentId)
        {
            var content = await _db.CourseContents
                                   .Include(c => c.CreatedByUser)
                                   .AsNoTracking()
                                   .FirstOrDefaultAsync(c => c.Id == contentId && c.CourseId == courseId);

            return content is null ? null : MapToDto(content);
        }

        // ── Create ────────────────────────────────────────────────────────
        public async Task<ContentResponseDto> CreateAsync(int courseId, CreateContentDto dto, int userId)
        {
            var content = new CourseContent
            {
                CourseId        = courseId,
                Title           = dto.Title,
                ContentType     = dto.ContentType,
                Description     = dto.Description ?? "",
                FilePath        = "default.pdf",
                FileSize        = 0,
                CreatedAt       = DateTime.UtcNow,
                CreatedByUserId = userId
            };

            _db.CourseContents.Add(content);
            await _db.SaveChangesAsync();

            await _db.Entry(content).Reference(c => c.CreatedByUser).LoadAsync();

            _logger.LogInformation("Content {ContentId} added to Course {CourseId} by User {UserId}",
                content.Id, courseId, userId);

            return MapToDto(content);
        }

        // ── Update ────────────────────────────────────────────────────────
        public async Task<ContentResponseDto?> UpdateAsync(
            int courseId, int contentId, UpdateContentDto dto, int userId, string role)
        {
            var content = await _db.CourseContents
                                   .Include(c => c.CreatedByUser)
                                   .FirstOrDefaultAsync(c => c.Id == contentId && c.CourseId == courseId);

            if (content is null) return null;

            // Instructor chỉ sửa nội dung của mình
            if (role == "Instructor" && content.CreatedByUserId != userId)
                return null;

            if (dto.Title is not null)       content.Title = dto.Title;
            if (dto.ContentType is not null) content.ContentType = dto.ContentType;
            if (dto.Description is not null) content.Description = dto.Description;
            content.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            _logger.LogInformation("Content {ContentId} updated by User {UserId}", contentId, userId);
            return MapToDto(content);
        }

        // ── Delete ────────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(int courseId, int contentId, int userId, string role)
        {
            var content = await _db.CourseContents
                                   .FirstOrDefaultAsync(c => c.Id == contentId && c.CourseId == courseId);

            if (content is null) return false;

            // Instructor chỉ xóa nội dung mình tạo
            if (role == "Instructor" && content.CreatedByUserId != userId)
                return false;

            _db.CourseContents.Remove(content);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Content {ContentId} deleted by User {UserId}", contentId, userId);
            return true;
        }
    }
}
