using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeApp.Migrations
{
    /// <inheritdoc />
    public partial class InterviewScheduling_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InterviewSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequirementId = table.Column<int>(type: "int", nullable: false),
                    ResumeId = table.Column<int>(type: "int", nullable: false),
                    Round = table.Column<int>(type: "int", nullable: false),
                    ScheduledStartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledEndUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    LocationOrLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PanelUserIdsCsv = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewSchedules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSchedules_RequirementId_ResumeId_Round",
                table: "InterviewSchedules",
                columns: new[] { "RequirementId", "ResumeId", "Round" });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSchedules_ScheduledStartUtc",
                table: "InterviewSchedules",
                column: "ScheduledStartUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewSchedules");
        }
    }
}
