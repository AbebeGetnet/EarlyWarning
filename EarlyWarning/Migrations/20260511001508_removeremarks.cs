using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class removeremarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegionRemarks",
                table: "RainfallReports");

            migrationBuilder.DropColumn(
                name: "ZoneRemarks",
                table: "RainfallReports");

            migrationBuilder.DropColumn(
                name: "RegionRemarks",
                table: "PastureStatuses");

            migrationBuilder.DropColumn(
                name: "ZoneRemarks",
                table: "PastureStatuses");

            migrationBuilder.DropColumn(
                name: "RegionRemarks",
                table: "FarmingActivities");

            migrationBuilder.DropColumn(
                name: "ZoneRemarks",
                table: "FarmingActivities");

            migrationBuilder.DropColumn(
                name: "RegionRemarks",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "ZoneRemarks",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "RegionRemarks",
                table: "CropGrowths");

            migrationBuilder.DropColumn(
                name: "ZoneRemarks",
                table: "CropGrowths");

            migrationBuilder.RenameColumn(
                name: "Low",
                table: "PastureStatuses",
                newName: "NotEnough");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NotEnough",
                table: "PastureStatuses",
                newName: "Low");

            migrationBuilder.AddColumn<string>(
                name: "RegionRemarks",
                table: "RainfallReports",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZoneRemarks",
                table: "RainfallReports",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionRemarks",
                table: "PastureStatuses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZoneRemarks",
                table: "PastureStatuses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionRemarks",
                table: "FarmingActivities",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZoneRemarks",
                table: "FarmingActivities",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionRemarks",
                table: "CropPestAndDeseaseReports",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZoneRemarks",
                table: "CropPestAndDeseaseReports",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionRemarks",
                table: "CropGrowths",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZoneRemarks",
                table: "CropGrowths",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
