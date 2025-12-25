using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeApp.Migrations
{
    /// <inheritdoc />
    public partial class InterviewFeedback_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InterviewFeedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InterviewScheduleId = table.Column<int>(type: "int", nullable: false),
                    RequirementId = table.Column<int>(type: "int", nullable: false),
                    ResumeId = table.Column<int>(type: "int", nullable: false),
                    Round = table.Column<int>(type: "int", nullable: false),
                    PanelUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Decision = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TechScore = table.Column<int>(type: "int", nullable: true),
                    CommScore = table.Column<int>(type: "int", nullable: true),
                    CultureScore = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewFeedbacks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewFeedbacks_InterviewScheduleId_PanelUserId",
                table: "InterviewFeedbacks",
                columns: new[] { "InterviewScheduleId", "PanelUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterviewFeedbacks_RequirementId_ResumeId_Round",
                table: "InterviewFeedbacks",
                columns: new[] { "RequirementId", "ResumeId", "Round" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewFeedbacks");
        }
    }
}
