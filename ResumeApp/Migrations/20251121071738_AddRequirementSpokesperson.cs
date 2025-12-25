using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRequirementSpokesperson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpokespersonId",
                table: "ClientRequirements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendorNotes",
                table: "ClientRequirements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientRequirements_SpokespersonId",
                table: "ClientRequirements",
                column: "SpokespersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRequirements_Spokespersons_SpokespersonId",
                table: "ClientRequirements",
                column: "SpokespersonId",
                principalTable: "Spokespersons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequirements_Spokespersons_SpokespersonId",
                table: "ClientRequirements");

            migrationBuilder.DropIndex(
                name: "IX_ClientRequirements_SpokespersonId",
                table: "ClientRequirements");

            migrationBuilder.DropColumn(
                name: "SpokespersonId",
                table: "ClientRequirements");

            migrationBuilder.DropColumn(
                name: "VendorNotes",
                table: "ClientRequirements");
        }
    }
}
