using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMinutesToRequirementMom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NotesHtml",
                table: "RequirementMoms",
                newName: "Minutes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Minutes",
                table: "RequirementMoms",
                newName: "NotesHtml");
        }
    }
}
