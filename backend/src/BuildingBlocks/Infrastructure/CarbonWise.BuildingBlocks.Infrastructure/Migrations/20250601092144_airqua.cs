using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarbonWise.BuildingBlocks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class airqua : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AirQualities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RecordDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    City = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitude = table.Column<double>(type: "double", precision: 10, scale: 7, nullable: false),
                    Longitude = table.Column<double>(type: "double", precision: 10, scale: 7, nullable: false),
                    AQI = table.Column<int>(type: "int", nullable: false),
                    DominentPollutant = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CO = table.Column<double>(type: "double", precision: 10, scale: 2, nullable: true),
                    Humidity = table.Column<double>(type: "double", precision: 10, scale: 2, nullable: true),
                    NO2 = table.Column<double>(type: "double", precision: 10, scale: 2, nullable: true),
                    Ozone = table.Column<double>(type: "double", precision: 10, scale: 2, nullable: true),
                    Pressure = table.Column<double>(type: "double", precision: 10, scale: 2, nullable: true),
                    PM10 = table.Column<double>(type: "double", precision: 10, scale: 2, nullable: true),
                    PM25 = table.Column<double>(type: "double", precision: 10, scale: 2, nullable: true),
                    SO2 = table.Column<double>(type: "double", precision: 10, scale: 2, nullable: true),
                    Temperature = table.Column<double>(type: "double", precision: 10, scale: 2, nullable: true),
                    WindSpeed = table.Column<double>(type: "double", precision: 10, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirQualities", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AirQualities_City",
                table: "AirQualities",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_AirQualities_City_RecordDate",
                table: "AirQualities",
                columns: new[] { "City", "RecordDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AirQualities_RecordDate",
                table: "AirQualities",
                column: "RecordDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AirQualities");
        }
    }
}
