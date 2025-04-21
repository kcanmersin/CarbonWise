using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarbonWise.BuildingBlocks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class schoolinfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampusVehicleEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CarsManagedByUniversity = table.Column<int>(type: "int", nullable: false),
                    CarsEnteringUniversity = table.Column<int>(type: "int", nullable: false),
                    MotorcyclesEnteringUniversity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampusVehicleEntries", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SchoolInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NumberOfPeople = table.Column<int>(type: "int", nullable: false),
                    CampusVehicleEntryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Year = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolInfos_CampusVehicleEntries_CampusVehicleEntryId",
                        column: x => x.CampusVehicleEntryId,
                        principalTable: "CampusVehicleEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolInfos_CampusVehicleEntryId",
                table: "SchoolInfos",
                column: "CampusVehicleEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolInfos_Year",
                table: "SchoolInfos",
                column: "Year",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchoolInfos");

            migrationBuilder.DropTable(
                name: "CampusVehicleEntries");
        }
    }
}
