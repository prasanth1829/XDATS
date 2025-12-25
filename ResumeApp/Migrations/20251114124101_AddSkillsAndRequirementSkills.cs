using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillsAndRequirementSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequirementSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequirementId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequirementSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequirementSkills_ClientRequirements_RequirementId",
                        column: x => x.RequirementId,
                        principalTable: "ClientRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequirementSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequirementSkills_RequirementId_SkillId_IsPrimary",
                table: "RequirementSkills",
                columns: new[] { "RequirementId", "SkillId", "IsPrimary" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequirementSkills_SkillId",
                table: "RequirementSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_IsActive_SortOrder",
                table: "Skills",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Name",
                table: "Skills",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequirementSkills");

            migrationBuilder.DropTable(
                name: "Skills");
        }
    }
}
