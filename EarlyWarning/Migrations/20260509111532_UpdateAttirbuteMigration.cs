using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttirbuteMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HumanDrinkWaterIssueId",
                table: "Locations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AffectedKebeleIds",
                table: "HumanDrinkWaterIssues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "HumanDrinkWaterIssueId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "HumanDrinkWaterIssueId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "HumanDrinkWaterIssueId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_HumanDrinkWaterIssueId",
                table: "Locations",
                column: "HumanDrinkWaterIssueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_HumanDrinkWaterIssues_HumanDrinkWaterIssueId",
                table: "Locations",
                column: "HumanDrinkWaterIssueId",
                principalTable: "HumanDrinkWaterIssues",
                principalColumn: "HumanDrinkWaterIssueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_HumanDrinkWaterIssues_HumanDrinkWaterIssueId",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_HumanDrinkWaterIssueId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "HumanDrinkWaterIssueId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "AffectedKebeleIds",
                table: "HumanDrinkWaterIssues");
        }
    }
}
