using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class AddMigrationDirectionAndDestination : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DestinationLocationId",
                table: "MigrationDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInward",
                table: "MigrationDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_MigrationDetails_DestinationLocationId",
                table: "MigrationDetails",
                column: "DestinationLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_MigrationDetails_Locations_DestinationLocationId",
                table: "MigrationDetails",
                column: "DestinationLocationId",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MigrationDetails_Locations_DestinationLocationId",
                table: "MigrationDetails");

            migrationBuilder.DropIndex(
                name: "IX_MigrationDetails_DestinationLocationId",
                table: "MigrationDetails");

            migrationBuilder.DropColumn(
                name: "DestinationLocationId",
                table: "MigrationDetails");

            migrationBuilder.DropColumn(
                name: "IsInward",
                table: "MigrationDetails");
        }
    }
}
