using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeApp.Migrations
{
    /// <inheritdoc />
    public partial class RequirementMom_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequirementMoms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequirementId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    MeetingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotesHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentsPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastEditedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastEditedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequirementMoms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequirementMoms_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequirementMoms_AspNetUsers_LastEditedByUserId",
                        column: x => x.LastEditedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequirementMoms_ClientRequirements_RequirementId",
                        column: x => x.RequirementId,
                        principalTable: "ClientRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequirementMomHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequirementMomId = table.Column<int>(type: "int", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NotesHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentsPath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequirementMomHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequirementMomHistories_AspNetUsers_EditedByUserId",
                        column: x => x.EditedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequirementMomHistories_RequirementMoms_RequirementMomId",
                        column: x => x.RequirementMomId,
                        principalTable: "RequirementMoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequirementMomHistories_EditedByUserId",
                table: "RequirementMomHistories",
                column: "EditedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RequirementMomHistories_RequirementMomId_EditedAt",
                table: "RequirementMomHistories",
                columns: new[] { "RequirementMomId", "EditedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RequirementMoms_CreatedByUserId",
                table: "RequirementMoms",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RequirementMoms_LastEditedByUserId",
                table: "RequirementMoms",
                column: "LastEditedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RequirementMoms_RequirementId_MeetingDate_CreatedAt",
                table: "RequirementMoms",
                columns: new[] { "RequirementId", "MeetingDate", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequirementMomHistories");

            migrationBuilder.DropTable(
                name: "RequirementMoms");
        }
    }
}
