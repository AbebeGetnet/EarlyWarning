using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class AnimalWaterSupply : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnimalWaterSupplyStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Enough = table.Column<bool>(type: "bit", nullable: false),
                    NoOfKebeliesWithPastureShortage = table.Column<int>(type: "int", nullable: false),
                    NoOfKebeliesWithWaterSupply = table.Column<int>(type: "int", nullable: false),
                    MaleHouseHold = table.Column<int>(type: "int", nullable: false),
                    FemaleHouseHold = table.Column<int>(type: "int", nullable: false),
                    MaleFamily = table.Column<int>(type: "int", nullable: false),
                    FemaleFamily = table.Column<int>(type: "int", nullable: false),
                    ChildhMale = table.Column<int>(type: "int", nullable: false),
                    ChildFemale = table.Column<int>(type: "int", nullable: false),
                    YouthMale = table.Column<int>(type: "int", nullable: false),
                    YouthFemale = table.Column<int>(type: "int", nullable: false),
                    ElderlyMale = table.Column<int>(type: "int", nullable: false),
                    ElderlyFemale = table.Column<int>(type: "int", nullable: false),
                    DisabledMale = table.Column<int>(type: "int", nullable: false),
                    DisabledFemale = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AnimalWaterSupplyStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnimalWaterSupplyStatuses_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnimalWaterSupplyStatuses_WoredaId",
                table: "AnimalWaterSupplyStatuses",
                column: "WoredaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnimalWaterSupplyStatuses");
        }
    }
}
