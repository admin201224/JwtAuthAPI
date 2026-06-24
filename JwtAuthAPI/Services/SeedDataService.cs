using JwtAuthAPI.Data;
using JwtAuthAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace JwtAuthAPI.Services
{
    public class SeedDataService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<SeedDataService> _logger;

        public SeedDataService(ApplicationDbContext db, ILogger<SeedDataService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Check if users already exist
                if (!await _db.Users.AnyAsync())
                {
                    _logger.LogInformation("Starting database user seed");

                    // Create default users
                    var admin = CreateUser("admin", "Admin@123456", "Admin");
                    var user = CreateUser("user", "User@123456", "User");
                    var moderator = CreateUser("moderator", "Moderator@123456", "Moderator");

                    _db.Users.AddRange(admin, user, moderator);
                    await _db.SaveChangesAsync();

                    _logger.LogInformation("Database user seed completed successfully");
                    _logger.LogInformation("Default users created:");
                    _logger.LogInformation("- admin / Admin@123456 (Admin)");
                    _logger.LogInformation("- user / User@123456 (User)");
                    _logger.LogInformation("- moderator / Moderator@123456 (Moderator)");
                }

                // Check if courses already exist
                if (!await _db.Courses.AnyAsync())
                {
                    var adminUser = await _db.Users.FirstOrDefaultAsync(u => u.Role == "Admin");
                    if (adminUser != null)
                    {
                        _logger.LogInformation("Seeding default courses");
                        var course1 = new Course
                        {
                            Title = "Lập trình C# ASP.NET Core MVC cho người bắt đầu",
                            Description = "Khóa học này sẽ hướng dẫn bạn cách xây dựng ứng dụng web hiện đại và hiệu năng cao sử dụng ASP.NET Core MVC trên nền tảng .NET 9. Bạn sẽ được tìm hiểu từ cơ bản về lập trình C#, mô hình MVC, kết nối cơ sở dữ liệu Entity Framework Core, đến các cơ chế Authentication/Authorization, Dependency Injection và triển khai ứng dụng thực tế.",
                            Price = 499000,
                            ThumbnailUrl = "https://images.unsplash.com/photo-1517694712202-14dd9538aa97?w=800&auto=format&fit=crop&q=60",
                            LearningMode = LearningMode.Online,
                            DurationHours = 40,
                            Level = CourseLevel.Beginner,
                            Status = CourseStatus.Published,
                            CreatedByUserId = adminUser.Id,
                            CreatedAt = DateTime.UtcNow
                        };

                        var course2 = new Course
                        {
                            Title = "Thiết kế cơ sở dữ liệu & truy vấn SQL nâng cao",
                            Description = "Khóa học chuyên sâu về thiết kế cơ sở dữ liệu quan hệ, chuẩn hóa dữ liệu (1NF, 2NF, 3NF), xây dựng các truy vấn phức tạp bằng T-SQL. Bạn sẽ làm quen với Store Procedures, Triggers, Views, Indexes để tối ưu hóa hiệu năng cơ sở dữ liệu và xử lý hàng triệu bản ghi hiệu quả.",
                            Price = 0, // Miễn phí
                            ThumbnailUrl = "https://images.unsplash.com/photo-1544383835-bda2bc66a55d?w=800&auto=format&fit=crop&q=60",
                            LearningMode = LearningMode.Hybrid,
                            DurationHours = 30,
                            Level = CourseLevel.Intermediate,
                            Status = CourseStatus.Published,
                            CreatedByUserId = adminUser.Id,
                            CreatedAt = DateTime.UtcNow
                        };

                        _db.Courses.AddRange(course1, course2);
                        await _db.SaveChangesAsync();

                        // Seed Course Contents for course 1
                        var contents1 = new List<CourseContent>
                        {
                            new CourseContent
                            {
                                CourseId = course1.Id,
                                Title = "Giới thiệu tổng quan về .NET 9 và ASP.NET Core",
                                ContentType = ContentType.Lecture,
                                Body = "Chào mừng bạn đến với khóa học ASP.NET Core MVC. Trong bài học này, chúng ta sẽ tìm hiểu lịch sử phát triển của .NET, các điểm mới trong phiên bản .NET 9 và cấu trúc cơ bản của một ứng dụng ASP.NET Core. Bạn sẽ hiểu được tại sao ASP.NET Core là framework hàng đầu cho ứng dụng doanh nghiệp lớn hiện nay.",
                                OrderIndex = 1,
                                IsPreview = true,
                                CreatedByUserId = adminUser.Id,
                                CreatedAt = DateTime.UtcNow
                            },
                            new CourseContent
                            {
                                CourseId = course1.Id,
                                Title = "Hướng dẫn cài đặt Visual Studio và SDK .NET 9",
                                ContentType = ContentType.Video,
                                VideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                                Body = "Bài học này hướng dẫn các bạn cách tải và cài đặt Visual Studio 2022 Community cùng với SDK .NET 9. Hãy làm theo hướng dẫn trong video để thiết lập môi trường lập trình của bạn.",
                                OrderIndex = 2,
                                IsPreview = true,
                                CreatedByUserId = adminUser.Id,
                                CreatedAt = DateTime.UtcNow
                            },
                            new CourseContent
                            {
                                CourseId = course1.Id,
                                Title = "Xây dựng Controller và View đầu tiên",
                                ContentType = ContentType.Lecture,
                                Body = "Hôm nay chúng ta sẽ bắt đầu thực hành viết code. Bạn sẽ tạo một Controller mới kế thừa từ Controller base, viết các Action method, và tạo các View tương ứng sử dụng Razor syntax để render dữ liệu động ra HTML.",
                                OrderIndex = 3,
                                IsPreview = false,
                                CreatedByUserId = adminUser.Id,
                                CreatedAt = DateTime.UtcNow
                            }
                        };

                        // Seed Course Contents for course 2
                        var contents2 = new List<CourseContent>
                        {
                            new CourseContent
                            {
                                CourseId = course2.Id,
                                Title = "Tổng quan về Cơ sở dữ liệu quan hệ (RDBMS)",
                                ContentType = ContentType.Lecture,
                                Body = "Bài học giới thiệu các khái niệm cốt lõi của Cơ sở dữ liệu quan hệ bao gồm: Bảng (Table), Khóa chính (Primary Key), Khóa ngoại (Foreign Key) và các kiểu dữ liệu thông dụng trong SQL Server.",
                                OrderIndex = 1,
                                IsPreview = true,
                                CreatedByUserId = adminUser.Id,
                                CreatedAt = DateTime.UtcNow
                            },
                            new CourseContent
                            {
                                CourseId = course2.Id,
                                Title = "Viết câu lệnh SELECT và bộ lọc WHERE cơ bản",
                                ContentType = ContentType.Lecture,
                                Body = "Tìm hiểu cách truy vấn dữ liệu từ một bảng bằng SELECT, lọc dữ liệu với mệnh đề WHERE, sắp xếp kết quả bằng ORDER BY và giới hạn số bản ghi trả về.",
                                OrderIndex = 2,
                                IsPreview = false,
                                CreatedByUserId = adminUser.Id,
                                CreatedAt = DateTime.UtcNow
                            }
                        };

                        _db.CourseContents.AddRange(contents1);
                        _db.CourseContents.AddRange(contents2);
                        await _db.SaveChangesAsync();
                        _logger.LogInformation("Default courses and contents seeded successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding database");
                throw;
            }
        }

        private User CreateUser(string username, string password, string role)
        {
            CreatePasswordHash(password, out byte[] hash, out byte[] salt);

            return new User
            {
                Username = username,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = role
            };
        }

        private void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }
}
