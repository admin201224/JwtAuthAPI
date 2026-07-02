using JwtAuthAPI.Models;
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
        public DbSet<CourseEnrollment> CourseEnrollments { get; set; }
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

                // CourseContent → Course (Cascade — xóa course thì xóa luôn tất cả bài học)
                entity.HasOne(c => c.Course)
                      .WithMany(course => course.Contents)
                      .HasForeignKey(c => c.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);

                // CourseContent → User (Restrict)
                entity.HasOne(c => c.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(c => c.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── CourseEnrollment ──────────────────────────────────────────
            modelBuilder.Entity<CourseEnrollment>(entity =>
            {
                // CourseEnrollment → Course (Cascade)
                entity.HasOne(ce => ce.Course)
                      .WithMany(c => c.Enrollments)
                      .HasForeignKey(ce => ce.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);

                // CourseEnrollment → User (Restrict)
                entity.HasOne(ce => ce.User)
                      .WithMany()
                      .HasForeignKey(ce => ce.UserId)
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

                // Enrollment → Course (Cascade — xóa course thì xóa luôn tất cả enrollment)
                entity.HasOne(e => e.Course)
                      .WithMany()
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}

