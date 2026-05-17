using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class addAttributesAffectedKebelie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_RainfallReports_RainfallReportId",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_RainfallReports_RainfallReportId1",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_RainfallReportId",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_RainfallReportId1",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "TotalKebeles",
                table: "RainfallReports");

            migrationBuilder.DropColumn(
                name: "RainfallReportId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "RainfallReportId1",
                table: "Locations");

            migrationBuilder.AddColumn<string>(
                name: "FullCoveredKebelieList",
                table: "RainfallReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HighRainKebelieList",
                table: "RainfallReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LowRainKebeliesList",
                table: "RainfallReports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NoRainKebelieList",
                table: "RainfallReports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullCoveredKebelieList",
                table: "RainfallReports");

            migrationBuilder.DropColumn(
                name: "HighRainKebelieList",
                table: "RainfallReports");

            migrationBuilder.DropColumn(
                name: "LowRainKebeliesList",
                table: "RainfallReports");

            migrationBuilder.DropColumn(
                name: "NoRainKebelieList",
                table: "RainfallReports");

            migrationBuilder.AddColumn<int>(
                name: "TotalKebeles",
                table: "RainfallReports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RainfallReportId",
                table: "Locations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RainfallReportId1",
                table: "Locations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "RainfallReportId", "RainfallReportId1" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "RainfallReportId", "RainfallReportId1" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "RainfallReportId", "RainfallReportId1" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_RainfallReportId",
                table: "Locations",
                column: "RainfallReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_RainfallReportId1",
                table: "Locations",
                column: "RainfallReportId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_RainfallReports_RainfallReportId",
                table: "Locations",
                column: "RainfallReportId",
                principalTable: "RainfallReports",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_RainfallReports_RainfallReportId1",
                table: "Locations",
                column: "RainfallReportId1",
                principalTable: "RainfallReports",
                principalColumn: "Id");
        }
    }
}
