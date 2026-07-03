using JwtAuthAPI.Data;
using JwtAuthAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthAPI.Services
{
    public interface ILessonProgressService
    {
        /// <summary>L?y ho?c t?o ti?n ð? cho m?t bài h?c c?a h?c viên</summary>
        Task<LessonProgressDto?> GetOrCreateProgressAsync(int userId, int contentId, int courseId);

        /// <summary>C?p nh?t ti?n ð? (ðánh d?u hoàn thành ho?c c?p nh?t %)</summary>
        Task<LessonProgressDto?> UpdateProgressAsync(int userId, int contentId, UpdateLessonProgressDto dto);

        /// <summary>L?y ti?n ð? toàn khóa c?a h?c viên</summary>
        Task<CourseProgressDto?> GetCourseProgressAsync(int userId, int courseId);

        /// <summary>L?y t?t c? ti?n ð? bài h?c c?a h?c viên trong m?t khóa</summary>
        Task<List<LessonProgressDto>> GetLessonProgressesByCourseAsync(int userId, int courseId);

        /// <summary>Xóa ti?n ð? khi h?y ðãng k? khóa h?c</summary>
        Task<bool> DeleteCourseProgressAsync(int userId, int courseId);
    }

    public class LessonProgressService : ILessonProgressService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LessonProgressService> _logger;

        public LessonProgressService(ApplicationDbContext context, ILogger<LessonProgressService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<LessonProgressDto?> GetOrCreateProgressAsync(int userId, int contentId, int courseId)
        {
            try
            {
                var progress = await _context.LessonProgresses
                    .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.ContentId == contentId);

                if (progress == null)
                {
                    progress = new LessonProgress
                    {
                        UserId = userId,
                        ContentId = contentId,
                        CourseId = courseId,
                        IsCompleted = false,
                        ProgressPercentage = 0,
                        StartedAt = DateTime.UtcNow
                    };
                    _context.LessonProgresses.Add(progress);
                    await _context.SaveChangesAsync();
                }

                return MapToDto(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetOrCreateProgressAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<LessonProgressDto?> UpdateProgressAsync(int userId, int contentId, UpdateLessonProgressDto dto)
        {
            try
            {
                var progress = await _context.LessonProgresses
                    .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.ContentId == contentId);

                if (progress == null)
                {
                    return null;
                }

                progress.IsCompleted = dto.IsCompleted;
                if (dto.ProgressPercentage.HasValue)
                {
                    progress.ProgressPercentage = Math.Min(100, Math.Max(0, dto.ProgressPercentage.Value));
                }

                if (dto.IsCompleted && !progress.CompletedAt.HasValue)
                {
                    progress.CompletedAt = DateTime.UtcNow;
                    progress.ProgressPercentage = 100;
                }

                _context.LessonProgresses.Update(progress);
                await _context.SaveChangesAsync();

                return MapToDto(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateProgressAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<CourseProgressDto?> GetCourseProgressAsync(int userId, int courseId)
        {
            try
            {
                var contents = await _context.CourseContents
                    .Where(cc => cc.CourseId == courseId)
                    .ToListAsync();

                if (!contents.Any())
                {
                    return new CourseProgressDto
                    {
                        CourseId = courseId,
                        TotalLessons = 0,
                        CompletedLessons = 0,
                        ProgressPercentage = 0,
                        LessonProgresses = new()
                    };
                }

                var progresses = await _context.LessonProgresses
                    .Where(lp => lp.UserId == userId && lp.CourseId == courseId)
                    .ToListAsync();

                var completedCount = progresses.Count(p => p.IsCompleted);
                var totalCount = contents.Count;
                var progressPercentage = totalCount > 0 ? (completedCount * 100) / totalCount : 0;

                return new CourseProgressDto
                {
                    CourseId = courseId,
                    TotalLessons = totalCount,
                    CompletedLessons = completedCount,
                    ProgressPercentage = progressPercentage,
                    LessonProgresses = progresses.Select(MapToDto).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetCourseProgressAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<List<LessonProgressDto>> GetLessonProgressesByCourseAsync(int userId, int courseId)
        {
            try
            {
                var progresses = await _context.LessonProgresses
                    .Where(lp => lp.UserId == userId && lp.CourseId == courseId)
                    .ToListAsync();

                return progresses.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetLessonProgressesByCourseAsync: {ex.Message}");
                return new List<LessonProgressDto>();
            }
        }

        public async Task<bool> DeleteCourseProgressAsync(int userId, int courseId)
        {
            try
            {
                var progresses = await _context.LessonProgresses
                    .Where(lp => lp.UserId == userId && lp.CourseId == courseId)
                    .ToListAsync();

                if (progresses.Any())
                {
                    _context.LessonProgresses.RemoveRange(progresses);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteCourseProgressAsync: {ex.Message}");
                return false;
            }
        }

        private static LessonProgressDto MapToDto(LessonProgress progress)
        {
            return new LessonProgressDto
            {
                Id = progress.Id,
                UserId = progress.UserId,
                ContentId = progress.ContentId,
                CourseId = progress.CourseId,
                IsCompleted = progress.IsCompleted,
                StartedAt = progress.StartedAt,
                CompletedAt = progress.CompletedAt,
                ProgressPercentage = progress.ProgressPercentage
            };
        }
    }
}
