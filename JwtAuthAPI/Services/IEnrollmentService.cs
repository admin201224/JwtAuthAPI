using JwtAuthAPI.Models;

namespace JwtAuthAPI.Services
{
    public interface IEnrollmentService
    {
        /// <summary>Đăng ký học viên vào khóa học. Trả về null nếu đã đăng ký rồi hoặc khóa không tồn tại.</summary>
        Task<EnrollmentResponseDto?> EnrollAsync(int userId, int courseId);

        /// <summary>Hủy đăng ký. Trả về false nếu không tìm thấy hoặc không phải của mình.</summary>
        Task<bool> UnenrollAsync(int enrollmentId, int userId, string role);

        /// <summary>Cập nhật trạng thái học (NotStarted / InProgress / Completed)</summary>
        Task<EnrollmentResponseDto?> UpdateStatusAsync(int enrollmentId, EnrollmentStatus status, int userId, string role);

        /// <summary>Danh sách khóa học đã đăng ký của user hiện tại</summary>
        Task<List<EnrollmentResponseDto>> GetMyEnrollmentsAsync(int userId);

        /// <summary>Danh sách học viên trong một khóa học (Admin / Instructor)</summary>
        Task<List<EnrollmentResponseDto>> GetByCourseAsync(int courseId);

        /// <summary>Kiểm tra user đã đăng ký khóa học chưa</summary>
        Task<bool> IsEnrolledAsync(int userId, int courseId);
    }
}
