using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeApp.Migrations
{
    /// <inheritdoc />
    public partial class MakeClientDocumentsOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.AddColumn<string>(
                name: "Certifications",
                table: "ClientRequirements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Deadline",
                table: "ClientRequirements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Education",
                table: "ClientRequirements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedJoiningDate",
                table: "ClientRequirements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExperienceMax",
                table: "ClientRequirements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExperienceMin",
                table: "ClientRequirements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoticePeriod",
                table: "ClientRequirements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequirementPriority",
                table: "ClientRequirements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SalaryRange",
                table: "ClientRequirements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ScreeningQuestions",
                table: "ClientRequirements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SkillsPrimary",
                table: "ClientRequirements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SkillsRequired",
                table: "ClientRequirements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SkillsSecondary",
                table: "ClientRequirements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "NDAPath",
                table: "ClientDocuments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MSAPath",
                table: "ClientDocuments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CorporatePresentationText",
                table: "ClientDocuments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CorporatePresentationPath",
                table: "ClientDocuments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentsPath",
                table: "ClientRequirements");

            migrationBuilder.DropColumn(
                name: "BillingType",
                table: "ClientRequirements");

            migrationBuilder.DropColumn(
                name: "Certifications",
                table: "ClientRequirements");

            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "ClientRequirements");

            migrationBuilder.DropColumn(
                name: "Education",
                table: "ClientRequirements");

            migrationBuilder.DropColumn(
                name: "ExpectedJoiningDate",
                table: "ClientRequirements");

            migrationBuilder.DropColumn(
                name: "ExperienceMax",
                table: "ClientRequirements");

            migrationBuilder.DropColumn(
                name: "ExperienceMin",
                table: "ClientRequirements");

            migrationBuilder.DropColumn(
                name: "NoticePeriod",
                table: "ClientRequirements");

            migrationBuilder.DropColumn(
                name: "RequirementPriority",
                table: "ClientRequirements");

            migrationBuilder.DropColumn(
                name: "SalaryRange",
                table: "ClientRequirements");

            migrationBuilder.DropColumn(
                name: "ScreeningQuestions",
                table: "ClientRequirements");

            migrationBuilder.DropColumn(
                name: "SkillsPrimary",
                table: "ClientRequirements");

            migrationBuilder.DropColumn(
                name: "SkillsRequired",
                table: "ClientRequirements");

            migrationBuilder.DropColumn(
                name: "SkillsSecondary",
                table: "ClientRequirements");

            migrationBuilder.RenameColumn(
                name: "SpecialInstructions",
                table: "ClientRequirements",
                newName: "Skills");

            migrationBuilder.AlterColumn<string>(
                name: "NDAPath",
                table: "ClientDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MSAPath",
                table: "ClientDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CorporatePresentationText",
                table: "ClientDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CorporatePresentationPath",
                table: "ClientDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
