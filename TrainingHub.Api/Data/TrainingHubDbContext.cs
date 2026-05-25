using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrainingHub.Models;

namespace TrainingHub.Data
{
    public class TrainingHubDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<Classroom> Classrooms { get; set; } = null!;
        public DbSet<Instructor> Instructors { get; set; } = null!;
        public DbSet<Trainee> Trainees { get; set; } = null!;
        public DbSet<CourseSession> CourseSessions { get; set; } = null!;
        public DbSet<Enrollment> Enrollments { get; set; } = null!;
        public DbSet<CertificationTrack> CertificationTracks { get; set; } = null!;
        public DbSet<CertificationTrackCourse> CertificationTrackCourses { get; set; } = null!;
        public DbSet<Certificate> Certificates { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        public TrainingHubDbContext(DbContextOptions<TrainingHubDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Category -> Courses
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Category)
                .WithMany(c => c.Courses)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course -> CourseSessions
            modelBuilder.Entity<CourseSession>()
                .HasOne(cs => cs.Course)
                .WithMany(c => c.CourseSessions)
                .HasForeignKey(cs => cs.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Instructor -> CourseSessions
            modelBuilder.Entity<CourseSession>()
                .HasOne(cs => cs.Instructor)
                .WithMany(i => i.CourseSessions)
                .HasForeignKey(cs => cs.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Classroom -> CourseSessions
            modelBuilder.Entity<CourseSession>()
                .HasOne(cs => cs.Classroom)
                .WithMany(cr => cr.CourseSessions)
                .HasForeignKey(cs => cs.ClassroomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Trainee -> Enrollments
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Trainee)
                .WithMany(t => t.Enrollments)
                .HasForeignKey(e => e.TraineeId)
                .OnDelete(DeleteBehavior.Restrict);

            // CourseSession -> Enrollments
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.CourseSession)
                .WithMany(cs => cs.Enrollments)
                .HasForeignKey(e => e.CourseSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // CertificationTrack -> CertificationTrackCourses
            modelBuilder.Entity<CertificationTrackCourse>()
                .HasOne(ctc => ctc.CertificationTrack)
                .WithMany(ct => ct.CertificationTrackCourses)
                .HasForeignKey(ctc => ctc.CertificationTrackId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course -> CertificationTrackCourses
            modelBuilder.Entity<CertificationTrackCourse>()
                .HasOne(ctc => ctc.Course)
                .WithMany(c => c.CertificationTrackCourses)
                .HasForeignKey(ctc => ctc.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Trainee -> Certificates
            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.Trainee)
                .WithMany(t => t.Certificates)
                .HasForeignKey(c => c.TraineeId)
                .OnDelete(DeleteBehavior.Restrict);

            // CertificationTrack -> Certificates
            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.CertificationTrack)
                .WithMany(ct => ct.Certificates)
                .HasForeignKey(c => c.CertificationTrackId)
                .OnDelete(DeleteBehavior.Restrict);

            // Trainee -> Payments
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Trainee)
                .WithMany(t => t.Payments)
                .HasForeignKey(p => p.TraineeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enrollment -> Payments
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Enrollment)
                .WithMany(e => e.Payments)
                .HasForeignKey(p => p.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Trainee -> Notifications
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Trainee)
                .WithMany(t => t.Notifications)
                .HasForeignKey(n => n.TraineeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Instructor -> Notifications
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Instructor)
                .WithMany(i => i.Notifications)
                .HasForeignKey(n => n.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .Property(c => c.Name)
                .HasMaxLength(100);

            modelBuilder.Entity<Course>()
                .Property(c => c.Title)
                .HasMaxLength(200);

            modelBuilder.Entity<Instructor>()
                .Property(i => i.Name)
                .HasMaxLength(150);

            modelBuilder.Entity<Trainee>()
                .Property(t => t.FullName)
                .HasMaxLength(150);

            modelBuilder.Entity<Notification>()
                .Property(n => n.Title)
                .HasMaxLength(200);

            modelBuilder.Entity<Certificate>()
                .Property(c => c.CertificateReferenceNumber)
                .HasMaxLength(100);
            modelBuilder.Entity<Course>()
                .Property(c => c.Fee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.AmountPaid)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Course>()
          .HasOne(c => c.PrerequisiteCourse)
          .WithMany()
          .HasForeignKey(c => c.PrerequisiteCourseId)
              .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
