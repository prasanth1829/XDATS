using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeApp.Migrations
{
    /// <inheritdoc />
    public partial class DocTypes_AddIsMandatory_RemoveNotesFromClientDocumentItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ClientDocumentItems");

            migrationBuilder.AddColumn<bool>(
                name: "IsMandatory",
                table: "DocumentTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMandatory",
                table: "DocumentTypes");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ClientDocumentItems",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}
