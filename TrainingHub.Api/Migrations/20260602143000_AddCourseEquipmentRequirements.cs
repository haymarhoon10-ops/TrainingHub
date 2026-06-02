using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TrainingHub.Data;

#nullable disable

namespace TrainingHub.Api.Migrations
{
    [DbContext(typeof(TrainingHubDbContext))]
    [Migration("20260602143000_AddCourseEquipmentRequirements")]
    public partial class AddCourseEquipmentRequirements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresLabComputer",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresProjector",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiresLabComputer",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "RequiresProjector",
                table: "Courses");
        }
    }
}
