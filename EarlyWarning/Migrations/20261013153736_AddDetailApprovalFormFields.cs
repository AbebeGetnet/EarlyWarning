using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailApprovalFormFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "SupplyTypes",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "DamagePercentage",
                table: "CropPestImpactDetails",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DamageCategories",
                table: "AccidentDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OtherDamageDescription",
                table: "AccidentDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CropDefensiveActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CropPestAndDeseaseReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activity = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    AreaCoveredInHectares = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropDefensiveActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CropDefensiveActivities_CropPestAndDeseaseReports_CropPestAndDeseaseReportId",
                        column: x => x.CropPestAndDeseaseReportId,
                        principalTable: "CropPestAndDeseaseReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CropDefensiveActivities_CropPestAndDeseaseReportId",
                table: "CropDefensiveActivities",
                column: "CropPestAndDeseaseReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CropDefensiveActivities");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "SupplyTypes");

            migrationBuilder.DropColumn(
                name: "DamagePercentage",
                table: "CropPestImpactDetails");

            migrationBuilder.DropColumn(
                name: "DamageCategories",
                table: "AccidentDetails");

            migrationBuilder.DropColumn(
                name: "OtherDamageDescription",
                table: "AccidentDetails");
        }
    }
}
