using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeApp.Migrations
{
    /// <inheritdoc />
    public partial class ClientRequirement_JobLocationFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JobLocationId",
                table: "ClientRequirements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientRequirements_JobLocationId",
                table: "ClientRequirements",
                column: "JobLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRequirements_Locations_JobLocationId",
                table: "ClientRequirements",
                column: "JobLocationId",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequirements_Locations_JobLocationId",
                table: "ClientRequirements");

            migrationBuilder.DropIndex(
                name: "IX_ClientRequirements_JobLocationId",
                table: "ClientRequirements");

            migrationBuilder.DropColumn(
                name: "JobLocationId",
                table: "ClientRequirements");
        }
    }
}
