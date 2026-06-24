using JwtAuthAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseContent> CourseContents { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ── User ──────────────────────────────────────────────────────
            modelBuilder.Entity<User>()
                        .HasIndex(u => u.Username).IsUnique();

            // ── Course ────────────────────────────────────────────────────
            modelBuilder.Entity<Course>(entity =>
            {
                entity.Property(c => c.Price).HasColumnType("decimal(18,2)");
                entity.HasIndex(c => c.Title);

                entity.Property(c => c.LearningMode).HasConversion<string>();
                entity.Property(c => c.Level).HasConversion<string>();
                entity.Property(c => c.Status).HasConversion<string>();

                // Course.CreatedBy → User (Restrict — xóa user không tự động xóa course)
                entity.HasOne(c => c.CreatedBy)
                      .WithMany()
                      .HasForeignKey(c => c.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── CourseContent ─────────────────────────────────────────────
            modelBuilder.Entity<CourseContent>(entity =>
            {
                entity.Property(c => c.ContentType).HasConversion<string>();

                // Index để sort theo thứ tự bài học
                entity.HasIndex(c => new { c.CourseId, c.OrderIndex });

                // CourseContent → Course (Cascade — xóa course thì xóa luôn tất cả bài học)
                entity.HasOne(c => c.Course)
                      .WithMany(course => course.Contents)
                      .HasForeignKey(c => c.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);

                // CourseContent → User (Restrict)
                entity.HasOne(c => c.CreatedBy)
                      .WithMany()
                      .HasForeignKey(c => c.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Enrollment ────────────────────────────────────────────────
            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.Property(e => e.Status).HasConversion<string>();

                // Unique: mỗi học viên chỉ đăng ký một khóa một lần
                entity.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();

                // Enrollment → User (Restrict)
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Enrollment → Course (Restrict — xóa course thì không tự xóa enrollment)
                entity.HasOne(e => e.Course)
                      .WithMany(c => c.Enrollments)
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}

