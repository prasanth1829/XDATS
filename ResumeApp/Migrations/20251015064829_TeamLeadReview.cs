using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeApp.Migrations
{
    /// <inheritdoc />
    public partial class TeamLeadReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastComment",
                table: "ResumeRequirementLinks",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ResumeRequirementLinks",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ResumeRequirementLinks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ResumeRequirementLinks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "CandidateStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResumeId = table.Column<int>(type: "int", nullable: false),
                    RequirementId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateStatusHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateStatusHistories_RequirementId_ResumeId_ChangedAt",
                table: "CandidateStatusHistories",
                columns: new[] { "RequirementId", "ResumeId", "ChangedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateStatusHistories");

            migrationBuilder.DropColumn(
                name: "LastComment",
                table: "ResumeRequirementLinks");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ResumeRequirementLinks");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ResumeRequirementLinks");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ResumeRequirementLinks");
        }
    }
}
