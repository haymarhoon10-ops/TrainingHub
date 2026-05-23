using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingHub.Api.Migrations
{
    public partial class SyncCertificateSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [Enrollments] WHERE [Id] = 3)
BEGIN
    SET IDENTITY_INSERT [Enrollments] ON;
    INSERT INTO [Enrollments] ([Id], [AttendanceStatus], [CourseSessionId], [EnrolledAt], [ResultRecordedAt], [ResultStatus], [Status], [TraineeId])
    VALUES (3, 'Present', 3, '2026-07-05T00:00:00', '2026-08-11T00:00:00', 'Pass', 'Completed', 1)
    SET IDENTITY_INSERT [Enrollments] OFF;
END

UPDATE [Certificates]
SET [IssuedAt] = '2026-08-15T00:00:00'
WHERE [Id] = 1;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM [Enrollments]
WHERE [Id] = 3;

UPDATE [Certificates]
SET [IssuedAt] = '2026-06-15T00:00:00'
WHERE [Id] = 1;
            ");
        }
    }
}