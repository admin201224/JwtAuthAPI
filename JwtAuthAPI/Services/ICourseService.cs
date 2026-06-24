using JwtAuthAPI.Models;

namespace JwtAuthAPI.Services
{
    public interface ICourseService
    {
        /// <summary>Lấy danh sách khóa học với filter tuỳ chọn</summary>
        Task<List<CourseResponseDto>> GetAllAsync(
            LearningMode? mode = null,
            CourseStatus? status = null,
            CourseLevel? level = null);

        /// <summary>Lấy chi tiết một khóa học theo ID</summary>
        Task<CourseResponseDto?> GetByIdAsync(int id);

        /// <summary>Tạo khóa học mới</summary>
        Task<CourseResponseDto> CreateAsync(CreateCourseDto dto, int createdByUserId);

        /// <summary>
        /// Cập nhật khóa học.
        /// Admin có thể sửa tất cả; Instructor chỉ sửa khóa của mình.
        /// Trả về null nếu không tìm thấy hoặc không có quyền.
        /// </summary>
        Task<CourseResponseDto?> UpdateAsync(int id, UpdateCourseDto dto, int userId, string role);

        /// <summary>
        /// Đổi trạng thái khóa học (Publish / Archive / Draft).
        /// Trả về null nếu không tìm thấy hoặc không có quyền.
        /// </summary>
        Task<CourseResponseDto?> UpdateStatusAsync(int id, CourseStatus newStatus, int userId, string role);

        /// <summary>Xóa khóa học (Admin only). Trả về false nếu không tìm thấy.</summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>Kiểm tra khóa học có tồn tại không</summary>
        Task<bool> ExistsAsync(int id);
    }
}
