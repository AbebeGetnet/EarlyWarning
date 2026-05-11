using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class AddPasture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FemaleFamily",
                table: "CropPestAndDeseaseReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaleFamily",
                table: "CropPestAndDeseaseReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PastureStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Enough = table.Column<bool>(type: "bit", nullable: false),
                    Low = table.Column<int>(type: "int", nullable: false),
                    NumberOfAnimalsAffected = table.Column<int>(type: "int", nullable: false),
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
                    ZoneRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RegionRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LasModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastureStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastureStatuses_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PastureStatuses_WoredaId",
                table: "PastureStatuses",
                column: "WoredaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PastureStatuses");

            migrationBuilder.DropColumn(
                name: "FemaleFamily",
                table: "CropPestAndDeseaseReports");

            migrationBuilder.DropColumn(
                name: "MaleFamily",
                table: "CropPestAndDeseaseReports");
        }
    }
}
