using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarbonWise.BuildingBlocks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class e : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Papers_Buildings_BuildingId",
                table: "Papers");

            migrationBuilder.DropForeignKey(
                name: "FK_Waters_Buildings_BuildingId",
                table: "Waters");

            migrationBuilder.DropIndex(
                name: "IX_Waters_BuildingId",
                table: "Waters");

            migrationBuilder.DropIndex(
                name: "IX_Papers_BuildingId",
                table: "Papers");

            migrationBuilder.DropColumn(
                name: "BuildingId",
                table: "Waters");

            migrationBuilder.DropColumn(
                name: "BuildingId",
                table: "Papers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BuildingId",
                table: "Waters",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "BuildingId",
                table: "Papers",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Waters_BuildingId",
                table: "Waters",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_Papers_BuildingId",
                table: "Papers",
                column: "BuildingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Papers_Buildings_BuildingId",
                table: "Papers",
                column: "BuildingId",
                principalTable: "Buildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Waters_Buildings_BuildingId",
                table: "Waters",
                column: "BuildingId",
                principalTable: "Buildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
