using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeApp.Migrations
{
    /// <inheritdoc />
    public partial class InterviewOutcome_Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActualEndUtc",
                table: "InterviewSchedules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualStartUtc",
                table: "InterviewSchedules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Outcome",
                table: "InterviewSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OutcomeNote",
                table: "InterviewSchedules",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualEndUtc",
                table: "InterviewSchedules");

            migrationBuilder.DropColumn(
                name: "ActualStartUtc",
                table: "InterviewSchedules");

            migrationBuilder.DropColumn(
                name: "Outcome",
                table: "InterviewSchedules");

            migrationBuilder.DropColumn(
                name: "OutcomeNote",
                table: "InterviewSchedules");
        }
    }
}
