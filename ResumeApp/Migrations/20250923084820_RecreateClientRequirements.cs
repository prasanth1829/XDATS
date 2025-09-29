using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeApp.Migrations
{
    /// <inheritdoc />
   
        /// <inheritdoc />
        public partial class RecreateClientRequirements : Migration
        {
            /// <inheritdoc />
            protected override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.CreateTable(
                    name: "ClientRequirements",
                    columns: table => new
                    {
                        Id = table.Column<int>(nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),

                        ClientId = table.Column<int>(nullable: false),

                        JobTitle = table.Column<string>(nullable: false),
                        Positions = table.Column<int>(nullable: false),
                        JobLocation = table.Column<string>(nullable: true),
                        EmploymentType = table.Column<string>(nullable: true),
                        WorkShift = table.Column<string>(nullable: true),

                        SkillsPrimary = table.Column<string>(nullable: true),
                        SkillsSecondary = table.Column<string>(nullable: true),
                        SkillsRequired = table.Column<string>(nullable: true),

                        Responsibilities = table.Column<string>(nullable: true),

                        ExperienceMin = table.Column<int>(nullable: true),
                        ExperienceMax = table.Column<int>(nullable: true),
                        Education = table.Column<string>(nullable: true),
                        Certifications = table.Column<string>(nullable: true),

                        SalaryRange = table.Column<string>(nullable: true),
                        BillingType = table.Column<string>(nullable: true),
                        NoticePeriod = table.Column<string>(nullable: true),

                        RequirementPriority = table.Column<string>(nullable: true),
                        Deadline = table.Column<DateTime>(nullable: true),
                        ExpectedJoiningDate = table.Column<DateTime>(nullable: true),

                        ScreeningQuestions = table.Column<string>(nullable: true),
                        SpecialInstructions = table.Column<string>(nullable: true),
                        AttachmentsPath = table.Column<string>(nullable: true),

                        CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()")
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_ClientRequirements", x => x.Id);
                        table.ForeignKey(
                            name: "FK_ClientRequirements_Clients_ClientId",
                            column: x => x.ClientId,
                            principalTable: "Clients",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    });

                migrationBuilder.CreateIndex(
                    name: "IX_ClientRequirements_ClientId",
                    table: "ClientRequirements",
                    column: "ClientId");
            }

            /// <inheritdoc />
            protected override void Down(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.DropTable(
                    name: "ClientRequirements");
            }
        }
    
}
