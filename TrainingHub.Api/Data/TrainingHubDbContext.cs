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


            // ------------------------
            // Seed data
            // ------------------------

            modelBuilder.Entity<Category>().HasData(
                new { Id = 1, Name = "Programming", Description = "Courses about software development and programming languages." },
                new { Id = 2, Name = "Data Science", Description = "Courses on data analysis, machine learning and statistics." }
            );

            modelBuilder.Entity<Instructor>().HasData(
                new { Id = 1, Name = "Alice Johnson", Email = "alice.johnson@example.com", PhoneNumber = "555-0101", Bio = "Senior .NET developer and trainer.", ExpertiseArea = "Software Development", IsAvailable = true, IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
                new { Id = 2, Name = "Bob Smith", Email = "bob.smith@example.com", PhoneNumber = "555-0202", Bio = "Data scientist and Python instructor.", ExpertiseArea = "Data Science", IsAvailable = true, IsActive = true, CreatedAt = new DateTime(2026, 1, 1) }
            );

            modelBuilder.Entity<Classroom>().HasData(
                new { Id = 1, RoomCode = 101, Capacity = 30, Location = "Building A - Room 101", HasProjector = true, HasLabComputer = true, IsAvailable = true },
                new { Id = 2, RoomCode = 202, Capacity = 20, Location = "Building B - Lab 202", HasProjector = false, HasLabComputer = true, IsAvailable = true }
            );

            modelBuilder.Entity<Course>().HasData(
                new
                {
                    Id = 1,
                    Title = "C# Fundamentals",
                    Description = "Introductory course to C# and .NET fundamentals.",
                    DurationHours = 24.0,
                    DefaultCapacity = 20,
                    Fee = 499.99m,
                    IsActive = true,
                    CategoryId = 1,
                    PrerequisiteCourseId = (int?)null
                },
               new
               {
                   Id = 2,
                   Title = "Python for Data Analysis",
                   Description = "Using Python to analyze and visualize data.",
                   DurationHours = 30.0,
                   DefaultCapacity = 25,
                   Fee = 599.50m,
                   IsActive = true,
                   CategoryId = 2,
                   PrerequisiteCourseId = (int?)null
               },
               new
               {
                   Id = 3,
                   Title = "ASP.NET Core MVC",
                   Description = "Building web applications using ASP.NET Core MVC.",
                   DurationHours = 45.0,
                   DefaultCapacity = 20,
                   Fee = 799.99m,
                   IsActive = true,
                   CategoryId = 1,
                   PrerequisiteCourseId = 1
               }
            );

            modelBuilder.Entity<CertificationTrack>().HasData(
                new { Id = 1, Name = "Full Stack Developer", Description = "Track covering front-end and back-end development.", IsActive = true },
                new { Id = 2, Name = "Data Analyst Track", Description = "Track focused on data analysis and visualization.", IsActive = true }
            );

            modelBuilder.Entity<CertificationTrackCourse>().HasData(
                new { Id = 1, CertificationTrackId = 1, CourseId = 1 },
                new { Id = 2, CertificationTrackId = 2, CourseId = 2 },
                new
                {
                    Id = 3,
                    CertificationTrackId = 1,
                    CourseId = 3
                }
            );

            modelBuilder.Entity<Trainee>().HasData(
                new { Id = 1, FullName = "John Doe", Email = "john.doe@example.com", PhoneNumber = "555-1001", RegisteredAt = new DateTime(2026, 1, 10), IsActive = true },
                new { Id = 2, FullName = "Jane Smith", Email = "jane.smith@example.com", PhoneNumber = "555-1002", RegisteredAt = new DateTime(2026, 2, 5), IsActive = true }
            );

            modelBuilder.Entity<CourseSession>().HasData(
                new
                {
                    Id = 1,
                    CourseId = 1,
                    InstructorId = 1,
                    ClassroomId = 1,
                    StartDate = new DateTime(2026, 6, 1),
                    EndDate = new DateTime(2026, 6, 10),
                    Capacity = 20,
                    Status = "Scheduled",
                    CreatedAt = new DateTime(2026, 5, 1)
                },
                new
                {
                    Id = 2,
                    CourseId = 2,
                    InstructorId = 2,
                    ClassroomId = 2,
                    StartDate = new DateTime(2026, 7, 1),
                    EndDate = new DateTime(2026, 7, 15),
                    Capacity = 25,
                    Status = "Scheduled",
                    CreatedAt = new DateTime(2026, 5, 10)
                }, new
                {
                    Id = 3,
                    CourseId = 3,
                    InstructorId = 1,
                    ClassroomId = 1,
                    StartDate = new DateTime(2026, 8, 1),
                    EndDate = new DateTime(2026, 8, 10),
                    Capacity = 20,
                    Status = "Scheduled",
                    CreatedAt = new DateTime(2026, 7, 1)
                }
            );

            modelBuilder.Entity<Enrollment>().HasData(
               new
               {
                   Id = 1,
                   TraineeId = 1,
                   CourseSessionId = 1,
                   Status = "Completed",
                   EnrolledAt = new DateTime(2026, 5, 20),
                   AttendanceStatus = "Present",
                   ResultStatus = "Pass",
                   ResultRecordedAt = new DateTime(2026, 6, 11)
               },
new
{
    Id = 3,
    TraineeId = 1,
    CourseSessionId = 3,
    Status = "Completed",
    EnrolledAt = new DateTime(2026, 7, 5),
    AttendanceStatus = "Present",
    ResultStatus = "Pass",
    ResultRecordedAt = new DateTime(2026, 8, 11)
},
new
{
    Id = 2,
    TraineeId = 2,
    CourseSessionId = 2,
    Status = "Enrolled",
    EnrolledAt = new DateTime(2026, 5, 22),
    AttendanceStatus = "Pending",
    ResultStatus = "Pending",
    ResultRecordedAt = (DateTime?)null
});

            modelBuilder.Entity<Payment>().HasData(
                new { Id = 1, TraineeId = 1, EnrollmentId = 1, AmountPaid = 499.99m, PaidAt = new DateTime(2026, 5, 21), PaymentMethod = "CreditCard", ReferenceNumber = "PAY-1001", Notes = "" },
                new { Id = 2, TraineeId = 2, EnrollmentId = 2, AmountPaid = 599.50m, PaidAt = new DateTime(2026, 5, 23), PaymentMethod = "CreditCard", ReferenceNumber = "PAY-1002", Notes = "" }
            );

            modelBuilder.Entity<Certificate>().HasData(
                new { Id = 1, TraineeId = 1, CertificationTrackId = 1, CertificateReferenceNumber = "CERT-0001", IssuedAt = new DateTime(2026, 8, 15), Status = "Issued" }
            );

            modelBuilder.Entity<Notification>().HasData(
                new { Id = 1, Title = "Welcome", Message = "Welcome to TrainingHub, John!", Type = "System", CreatedAt = new DateTime(2026, 1, 11), IsRead = false, TraineeId = 1, InstructorId = (int?)null },
                new { Id = 2, Title = "Session Assigned", Message = "You have been assigned to C# Fundamentals.", Type = "Assignment", CreatedAt = new DateTime(2026, 5, 2), IsRead = false, TraineeId = (int?)null, InstructorId = 1 }
            );
        }
    }


}
