using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class AddMalnutritionReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MalnutritionReports",
                columns: table => new
                {
                    MalnutritionReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChildrenScreened = table.Column<int>(type: "int", nullable: false),
                    ChildrenSevere = table.Column<int>(type: "int", nullable: false),
                    ChildrenModerate = table.Column<int>(type: "int", nullable: false),
                    ChildrenGamRatePercent = table.Column<decimal>(type: "decimal(9,2)", nullable: true),
                    PregnantLactatingScreened = table.Column<int>(type: "int", nullable: false),
                    PregnantLactatingModerate = table.Column<int>(type: "int", nullable: false),
                    PregnantLactatingMamRatePercent = table.Column<decimal>(type: "decimal(9,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MalnutritionReports", x => x.MalnutritionReportId);
                    table.ForeignKey(
                        name: "FK_MalnutritionReports_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MalnutritionReports_WoredaId_StartDate_EndDate",
                table: "MalnutritionReports",
                columns: new[] { "WoredaId", "StartDate", "EndDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MalnutritionReports");
        }
    }
}
