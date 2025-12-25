using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRequirementAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequirementAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequirementId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequirementAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequirementAssignments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequirementAssignments_ClientRequirements_RequirementId",
                        column: x => x.RequirementId,
                        principalTable: "ClientRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequirementAssignments_RequirementId",
                table: "RequirementAssignments",
                column: "RequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_RequirementAssignments_UserId",
                table: "RequirementAssignments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequirementAssignments");
        }
    }
}
