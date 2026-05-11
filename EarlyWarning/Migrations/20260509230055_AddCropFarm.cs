using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class AddCropFarm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CropPestAndDesease",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CPDType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropPestAndDesease", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CropPestAndDeseaseReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LasModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZoneApprovedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegionApprovedById = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropPestAndDeseaseReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FarmingActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeherFarmPlan = table.Column<float>(type: "real", nullable: false),
                    MeherPloughed = table.Column<float>(type: "real", nullable: false),
                    MeherSown = table.Column<float>(type: "real", nullable: false),
                    MeherHarvestingHHarvesting = table.Column<float>(type: "real", nullable: false),
                    MeherSownWithResidualMoisture = table.Column<float>(type: "real", nullable: false),
                    AutumnFarmPlan = table.Column<float>(type: "real", nullable: false),
                    AutumnPloughed = table.Column<float>(type: "real", nullable: false),
                    AutumnSown = table.Column<float>(type: "real", nullable: false),
                    AutumnHarvestingHHarvesting = table.Column<float>(type: "real", nullable: false),
                    AutumnSownWithResidualMoisture = table.Column<float>(type: "real", nullable: false),
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
                    table.PrimaryKey("PK_FarmingActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FarmingActivities_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FarmingActivities_WoredaId",
                table: "FarmingActivities",
                column: "WoredaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CropPestAndDesease");

            migrationBuilder.DropTable(
                name: "CropPestAndDeseaseReports");

            migrationBuilder.DropTable(
                name: "FarmingActivities");
        }
    }
}
