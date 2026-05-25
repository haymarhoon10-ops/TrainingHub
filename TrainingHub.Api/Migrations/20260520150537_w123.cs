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

            migrationBuilder.DropColumn(
                name: "PrerequisiteCourseId",
                table: "Courses");
        }
    }
}
