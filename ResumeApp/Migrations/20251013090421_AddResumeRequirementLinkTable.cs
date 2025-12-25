using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeApp.Migrations
{
    /// <inheritdoc />
    public partial class AddResumeRequirementLinkTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResumeRequirementLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResumeId = table.Column<int>(type: "int", nullable: false),
                    RequirementId = table.Column<int>(type: "int", nullable: false),
                    LinkedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LinkedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResumeRequirementLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResumeRequirementLinks_AspNetUsers_LinkedByUserId",
                        column: x => x.LinkedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResumeRequirementLinks_ClientRequirements_RequirementId",
                        column: x => x.RequirementId,
                        principalTable: "ClientRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResumeRequirementLinks_Resumes_ResumeId",
                        column: x => x.ResumeId,
                        principalTable: "Resumes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResumeRequirementLinks_LinkedByUserId",
                table: "ResumeRequirementLinks",
                column: "LinkedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResumeRequirementLinks_RequirementId",
                table: "ResumeRequirementLinks",
                column: "RequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_ResumeRequirementLinks_ResumeId",
                table: "ResumeRequirementLinks",
                column: "ResumeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResumeRequirementLinks");
        }
    }
}
