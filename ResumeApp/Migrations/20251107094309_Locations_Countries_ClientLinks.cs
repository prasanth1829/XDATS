using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeApp.Migrations
{
    /// <inheritdoc />
    public partial class Locations_Countries_ClientLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeadquarterLocation",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "OtherOfficeLocations",
                table: "Clients");

            migrationBuilder.AddColumn<int>(
                name: "HeadquarterCountryId",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HeadquarterLocationId",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsoCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    StateOrProvince = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Timezone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClientOtherLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientOtherLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientOtherLocations_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientOtherLocations_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_HeadquarterCountryId",
                table: "Clients",
                column: "HeadquarterCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_HeadquarterLocationId",
                table: "Clients",
                column: "HeadquarterLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientOtherLocations_ClientId_LocationId",
                table: "ClientOtherLocations",
                columns: new[] { "ClientId", "LocationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientOtherLocations_LocationId",
                table: "ClientOtherLocations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_IsActive_SortOrder_Name",
                table: "Countries",
                columns: new[] { "IsActive", "SortOrder", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_CountryId_IsActive_SortOrder_Name",
                table: "Locations",
                columns: new[] { "CountryId", "IsActive", "SortOrder", "Name" });

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Countries_HeadquarterCountryId",
                table: "Clients",
                column: "HeadquarterCountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Locations_HeadquarterLocationId",
                table: "Clients",
                column: "HeadquarterLocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Countries_HeadquarterCountryId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Locations_HeadquarterLocationId",
                table: "Clients");

            migrationBuilder.DropTable(
                name: "ClientOtherLocations");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Clients_HeadquarterCountryId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_HeadquarterLocationId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "HeadquarterCountryId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "HeadquarterLocationId",
                table: "Clients");

            migrationBuilder.AddColumn<string>(
                name: "HeadquarterLocation",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OtherOfficeLocations",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
