using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class w123 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrerequisiteCourseId",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 1,
                column: "PrerequisiteCourseId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 2,
                column: "PrerequisiteCourseId",
                value: null);

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "CategoryId", "DefaultCapacity", "Description", "DurationHours", "Fee", "IsActive", "PrerequisiteCourseId", "Title" },
                values: new object[] { 3, 1, 20, "Building web applications using ASP.NET Core MVC.", 45.0, 799.99m, true, 1, "ASP.NET Core MVC" });

            migrationBuilder.UpdateData(
                table: "Enrollments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AttendanceStatus", "ResultRecordedAt", "ResultStatus", "Status" },
                values: new object[] { "Present", new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pass", "Completed" });

            migrationBuilder.UpdateData(
                table: "Enrollments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AttendanceStatus", "ResultStatus" },
                values: new object[] { "Pending", "Pending" });

            migrationBuilder.InsertData(
                table: "CertificationTrackCourses",
                columns: new[] { "Id", "CertificationTrackId", "CourseId" },
                values: new object[] { 3, 1, 3 });

            migrationBuilder.InsertData(
                table: "CourseSessions",
                columns: new[] { "Id", "Capacity", "ClassroomId", "CourseId", "CreatedAt", "EndDate", "InstructorId", "StartDate", "Status" },
                values: new object[] { 3, 20, 1, 3, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Scheduled" });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_PrerequisiteCourseId",
                table: "Courses",
                column: "PrerequisiteCourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Courses_PrerequisiteCourseId",
                table: "Courses",
                column: "PrerequisiteCourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Courses_PrerequisiteCourseId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_PrerequisiteCourseId",
                table: "Courses");

            migrationBuilder.DeleteData(
                table: "CertificationTrackCourses",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "CourseSessions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "PrerequisiteCourseId",
                table: "Courses");

            migrationBuilder.UpdateData(
                table: "Enrollments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AttendanceStatus", "ResultRecordedAt", "ResultStatus", "Status" },
                values: new object[] { "", null, "", "Enrolled" });

            migrationBuilder.UpdateData(
                table: "Enrollments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AttendanceStatus", "ResultStatus" },
                values: new object[] { "", "" });
        }
    }
}
