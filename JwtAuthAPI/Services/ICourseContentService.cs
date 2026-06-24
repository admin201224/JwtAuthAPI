using JwtAuthAPI.Models;

namespace JwtAuthAPI.Services
{
    public interface ICourseContentService
    {
        /// <summary>Lấy tất cả bài học của một khóa, sắp xếp theo OrderIndex</summary>
        Task<List<ContentResponseDto>> GetByCourseIdAsync(int courseId);

        /// <summary>Lấy chi tiết một bài học</summary>
        Task<ContentResponseDto?> GetByIdAsync(int courseId, int contentId);

        /// <summary>Thêm bài học vào khóa học</summary>
        Task<ContentResponseDto> CreateAsync(int courseId, CreateContentDto dto, int userId);

        /// <summary>
        /// Sửa bài học.
        /// Admin sửa tất cả; Instructor chỉ sửa bài học trong khóa của mình.
        /// Trả về null nếu không tìm thấy hoặc không có quyền.
        /// </summary>
        Task<ContentResponseDto?> UpdateAsync(int courseId, int contentId, UpdateContentDto dto, int userId, string role);

        /// <summary>Xóa bài học</summary>
        Task<bool> DeleteAsync(int courseId, int contentId, int userId, string role);
    }
}
