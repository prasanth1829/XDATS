using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeApp.Migrations
{
    /// <inheritdoc />
    public partial class PanelScreening_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PanelAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequirementId = table.Column<int>(type: "int", nullable: false),
                    ResumeId = table.Column<int>(type: "int", nullable: false),
                    PanelUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PanelAssignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PanelFeedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequirementId = table.Column<int>(type: "int", nullable: false),
                    ResumeId = table.Column<int>(type: "int", nullable: false),
                    PanelUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Decision = table.Column<int>(type: "int", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PanelFeedbacks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PanelAssignments_PanelUserId_RequirementId_ResumeId",
                table: "PanelAssignments",
                columns: new[] { "PanelUserId", "RequirementId", "ResumeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PanelFeedbacks_PanelUserId_RequirementId_ResumeId_DecidedAt",
                table: "PanelFeedbacks",
                columns: new[] { "PanelUserId", "RequirementId", "ResumeId", "DecidedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PanelAssignments");

            migrationBuilder.DropTable(
                name: "PanelFeedbacks");
        }
    }
}
