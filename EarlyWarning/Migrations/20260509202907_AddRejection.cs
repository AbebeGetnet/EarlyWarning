using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class AddRejection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RegionRejectedAt",
                table: "RainfallReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionRejectedById",
                table: "RainfallReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionRejectionRemark",
                table: "RainfallReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ZoneRejectedAt",
                table: "RainfallReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZoneRejectedById",
                table: "RainfallReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZoneRejectionRemark",
                table: "RainfallReports",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegionRejectedAt",
                table: "RainfallReports");

            migrationBuilder.DropColumn(
                name: "RegionRejectedById",
                table: "RainfallReports");

            migrationBuilder.DropColumn(
                name: "RegionRejectionRemark",
                table: "RainfallReports");

            migrationBuilder.DropColumn(
                name: "ZoneRejectedAt",
                table: "RainfallReports");

            migrationBuilder.DropColumn(
                name: "ZoneRejectedById",
                table: "RainfallReports");

            migrationBuilder.DropColumn(
                name: "ZoneRejectionRemark",
                table: "RainfallReports");
        }
    }
}
