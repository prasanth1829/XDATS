using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeApp.Migrations
{
    /// <inheritdoc />
    public partial class MatchScore_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ResumeRequirementLinks_RequirementId",
                table: "ResumeRequirementLinks");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastScoredAt",
                table: "ResumeRequirementLinks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatchBreakdownJson",
                table: "ResumeRequirementLinks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "MatchScore",
                table: "ResumeRequirementLinks",
                type: "smallint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResumeRequirementLinks_RequirementId_MatchScore",
                table: "ResumeRequirementLinks",
                columns: new[] { "RequirementId", "MatchScore" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ResumeRequirementLinks_RequirementId_MatchScore",
                table: "ResumeRequirementLinks");

            migrationBuilder.DropColumn(
                name: "LastScoredAt",
                table: "ResumeRequirementLinks");

            migrationBuilder.DropColumn(
                name: "MatchBreakdownJson",
                table: "ResumeRequirementLinks");

            migrationBuilder.DropColumn(
                name: "MatchScore",
                table: "ResumeRequirementLinks");

            migrationBuilder.CreateIndex(
                name: "IX_ResumeRequirementLinks_RequirementId",
                table: "ResumeRequirementLinks",
                column: "RequirementId");
        }
    }
}
