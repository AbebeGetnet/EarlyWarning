using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class addcroppestdesease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "AffectedLandInHectar",
                table: "CropPestAndDeseaseReports",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChildFemale",
                table: "CropPestAndDeseaseReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ChildhMale",
                table: "CropPestAndDeseaseReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CropDiseasesJson",
                table: "CropPestAndDeseaseReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisabledFemale",
                table: "CropPestAndDeseaseReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DisabledMale",
                table: "CropPestAndDeseaseReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ElderlyFemale",
                table: "CropPestAndDeseaseReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ElderlyMale",
                table: "CropPestAndDeseaseReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FemaleHouseHold",
                table: "CropPestAndDeseaseReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasPestAndDeseasOccured",
                table: "CropPestAndDeseaseReports",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaleHouseHold",
                table: "CropPestAndDeseaseReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegionRejectedAt",
                table: "CropPestAndDeseaseReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionRejectedById",
                table: "CropPestAndDeseaseReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionRejectionRemark",
                table: "CropPestAndDeseaseReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "CropPestAndDeseaseReports",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeOfCropAffected",
                table: "CropPestAndDeseaseReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WoredaId",
                table: "CropPestAndDeseaseReports",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "YouthFemale",
                table: "CropPestAndDeseaseReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "YouthMale",
                table: "CropPestAndDeseaseReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ZoneRejectedAt",
                table: "CropPestAndDeseaseReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZoneRejectedById",
                table: "CropPestAndDeseaseReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZoneRejectionRemark",
                table: "CropPestAndDeseaseReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CropGrowths",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AtSeedStage = table.Column<float>(type: "real", nullable: false),
                    AtGerminationStage = table.Column<float>(type: "real", nullable: false),
                    AtGrowthStage = table.Column<float>(type: "real", nullable: false),
                    AtFruitStage = table.Column<float>(type: "real", nullable: false),
                    AtHarvestingStage = table.Column<float>(type: "real", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZoneApprovedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegionApprovedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneRejectionRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneRejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZoneRejectedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionRejectionRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionRejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegionRejectedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LasModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropGrowths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CropGrowths_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CropPestAndDeseaseReports_WoredaId",
                table: "CropPestAndDeseaseReports",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_CropGrowths_WoredaId",
                table: "CropGrowths",
                column: "WoredaId");

            migrationBuilder.AddForeignKey(
                name: "FK_CropPestAndDeseaseReports_Locations_WoredaId",
                table: "CropPestAndDeseaseReports",
                column: "WoredaId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CropPestAndDeseaseReports_Locations_WoredaId",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropTable(
                name: "CropGrowths");

            migrationBuilder.DropIndex(
                name: "IX_CropPestAndDeseaseReports_WoredaId",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "AffectedLandInHectar",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "ChildFemale",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "ChildhMale",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "CropDiseasesJson",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "DisabledFemale",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "DisabledMale",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "ElderlyFemale",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "ElderlyMale",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "FemaleHouseHold",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "HasPestAndDeseasOccured",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "MaleHouseHold",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "RegionRejectedAt",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "RegionRejectedById",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "RegionRejectionRemark",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "TypeOfCropAffected",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "WoredaId",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "YouthFemale",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "YouthMale",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "ZoneRejectedAt",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "ZoneRejectedById",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "ZoneRejectionRemark",
                table: "CropPestAndDeseaseReports");
        }
    }
}
