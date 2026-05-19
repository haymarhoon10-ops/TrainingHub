using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrainingHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CertificationTracks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificationTracks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Classrooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomCode = table.Column<int>(type: "int", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HasProjector = table.Column<bool>(type: "bit", nullable: false),
                    HasLabComputer = table.Column<bool>(type: "bit", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classrooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Instructors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ExpertiseArea = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instructors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trainees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trainees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DurationHours = table.Column<double>(type: "float", nullable: false),
                    DefaultCapacity = table.Column<int>(type: "int", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Courses_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TraineeId = table.Column<int>(type: "int", nullable: false),
                    CertificationTrackId = table.Column<int>(type: "int", nullable: false),
                    CertificateReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificates_CertificationTracks_CertificationTrackId",
                        column: x => x.CertificationTrackId,
                        principalTable: "CertificationTracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Certificates_Trainees_TraineeId",
                        column: x => x.TraineeId,
                        principalTable: "Trainees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    TraineeId = table.Column<int>(type: "int", nullable: true),
                    InstructorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Instructors_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Instructors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Trainees_TraineeId",
                        column: x => x.TraineeId,
                        principalTable: "Trainees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CertificationTrackCourses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CertificationTrackId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificationTrackCourses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificationTrackCourses_CertificationTracks_CertificationTrackId",
                        column: x => x.CertificationTrackId,
                        principalTable: "CertificationTracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CertificationTrackCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    InstructorId = table.Column<int>(type: "int", nullable: false),
                    ClassroomId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseSessions_Classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseSessions_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseSessions_Instructors_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Instructors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TraineeId = table.Column<int>(type: "int", nullable: false),
                    CourseSessionId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EnrolledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AttendanceStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ResultStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ResultRecordedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrollments_CourseSessions_CourseSessionId",
                        column: x => x.CourseSessionId,
                        principalTable: "CourseSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_Trainees_TraineeId",
                        column: x => x.TraineeId,
                        principalTable: "Trainees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TraineeId = table.Column<int>(type: "int", nullable: false),
                    EnrollmentId = table.Column<int>(type: "int", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Trainees_TraineeId",
                        column: x => x.TraineeId,
                        principalTable: "Trainees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Courses about software development and programming languages.", "Programming" },
                    { 2, "Courses on data analysis, machine learning and statistics.", "Data Science" }
                });

            migrationBuilder.InsertData(
                table: "CertificationTracks",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Track covering front-end and back-end development.", true, "Full Stack Developer" },
                    { 2, "Track focused on data analysis and visualization.", true, "Data Analyst Track" }
                });

            migrationBuilder.InsertData(
                table: "Classrooms",
                columns: new[] { "Id", "Capacity", "HasLabComputer", "HasProjector", "IsAvailable", "Location", "RoomCode" },
                values: new object[,]
                {
                    { 1, 30, true, true, true, "Building A - Room 101", 101 },
                    { 2, 20, true, false, true, "Building B - Lab 202", 202 }
                });

            migrationBuilder.InsertData(
                table: "Instructors",
                columns: new[] { "Id", "Bio", "CreatedAt", "Email", "ExpertiseArea", "IsActive", "IsAvailable", "Name", "PhoneNumber" },
                values: new object[,]
                {
                    { 1, "Senior .NET developer and trainer.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "alice.johnson@example.com", "Software Development", true, true, "Alice Johnson", "555-0101" },
                    { 2, "Data scientist and Python instructor.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "bob.smith@example.com", "Data Science", true, true, "Bob Smith", "555-0202" }
                });

            migrationBuilder.InsertData(
                table: "Trainees",
                columns: new[] { "Id", "Email", "FullName", "IsActive", "PhoneNumber", "RegisteredAt" },
                values: new object[,]
                {
                    { 1, "john.doe@example.com", "John Doe", true, "555-1001", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "jane.smith@example.com", "Jane Smith", true, "555-1002", new DateTime(2026, 2, 5, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Certificates",
                columns: new[] { "Id", "CertificateReferenceNumber", "CertificationTrackId", "IssuedAt", "Status", "TraineeId" },
                values: new object[] { 1, "CERT-0001", 1, new DateTime(2026, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Issued", 1 });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "CategoryId", "DefaultCapacity", "Description", "DurationHours", "Fee", "IsActive", "Title" },
                values: new object[,]
                {
                    { 1, 1, 20, "Introductory course to C# and .NET fundamentals.", 24.0, 499.99m, true, "C# Fundamentals" },
                    { 2, 2, 25, "Using Python to analyze and visualize data.", 30.0, 599.50m, true, "Python for Data Analysis" }
                });

            migrationBuilder.InsertData(
                table: "Notifications",
                columns: new[] { "Id", "CreatedAt", "InstructorId", "IsRead", "Message", "Title", "TraineeId", "Type" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, "Welcome to TrainingHub, John!", "Welcome", 1, "System" },
                    { 2, new DateTime(2026, 5, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "You have been assigned to C# Fundamentals.", "Session Assigned", null, "Assignment" }
                });

            migrationBuilder.InsertData(
                table: "CertificationTrackCourses",
                columns: new[] { "Id", "CertificationTrackId", "CourseId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 2 }
                });

            migrationBuilder.InsertData(
                table: "CourseSessions",
                columns: new[] { "Id", "Capacity", "ClassroomId", "CourseId", "CreatedAt", "EndDate", "InstructorId", "StartDate", "Status" },
                values: new object[,]
                {
                    { 1, 20, 1, 1, new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Scheduled" },
                    { 2, 25, 2, 2, new DateTime(2026, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 7, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Scheduled" }
                });

            migrationBuilder.InsertData(
                table: "Enrollments",
                columns: new[] { "Id", "AttendanceStatus", "CourseSessionId", "EnrolledAt", "ResultRecordedAt", "ResultStatus", "Status", "TraineeId" },
                values: new object[,]
                {
                    { 1, "", 1, new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "", "Enrolled", 1 },
                    { 2, "", 2, new DateTime(2026, 5, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "", "Enrolled", 2 }
                });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "Id", "AmountPaid", "EnrollmentId", "Notes", "PaidAt", "PaymentMethod", "ReferenceNumber", "TraineeId" },
                values: new object[,]
                {
                    { 1, 499.99m, 1, "", new DateTime(2026, 5, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "CreditCard", "PAY-1001", 1 },
                    { 2, 599.50m, 2, "", new DateTime(2026, 5, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "CreditCard", "PAY-1002", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CertificationTrackId",
                table: "Certificates",
                column: "CertificationTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_TraineeId",
                table: "Certificates",
                column: "TraineeId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificationTrackCourses_CertificationTrackId",
                table: "CertificationTrackCourses",
                column: "CertificationTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificationTrackCourses_CourseId",
                table: "CertificationTrackCourses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CategoryId",
                table: "Courses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSessions_ClassroomId",
                table: "CourseSessions",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSessions_CourseId",
                table: "CourseSessions",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSessions_InstructorId",
                table: "CourseSessions",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CourseSessionId",
                table: "Enrollments",
                column: "CourseSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_TraineeId",
                table: "Enrollments",
                column: "TraineeId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_InstructorId",
                table: "Notifications",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TraineeId",
                table: "Notifications",
                column: "TraineeId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_EnrollmentId",
                table: "Payments",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TraineeId",
                table: "Payments",
                column: "TraineeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "CertificationTrackCourses");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "CertificationTracks");

            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "CourseSessions");

            migrationBuilder.DropTable(
                name: "Trainees");

            migrationBuilder.DropTable(
                name: "Classrooms");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Instructors");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
