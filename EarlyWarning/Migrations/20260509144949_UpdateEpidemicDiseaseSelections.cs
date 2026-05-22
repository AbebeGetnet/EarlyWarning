using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEpidemicDiseaseSelections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EpidemicDiseaseSelections",
                columns: table => new
                {
                    EpidemicDiseaseSelectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EpidemicDiseaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EpidemicDiseaseTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumberOfCases = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EpidemicDiseaseTypeEpidemicDiseaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpidemicDiseaseSelections", x => x.EpidemicDiseaseSelectionId);
                    table.ForeignKey(
                        name: "FK_EpidemicDiseaseSelections_EpidemicDiseaseTypes_EpidemicDiseaseTypeEpidemicDiseaseId",
                        column: x => x.EpidemicDiseaseTypeEpidemicDiseaseId,
                        principalTable: "EpidemicDiseaseTypes",
                        principalColumn: "EpidemicDiseaseId");
                    table.ForeignKey(
                        name: "FK_EpidemicDiseaseSelections_EpidemicDiseaseTypes_EpidemicDiseaseTypeId",
                        column: x => x.EpidemicDiseaseTypeId,
                        principalTable: "EpidemicDiseaseTypes",
                        principalColumn: "EpidemicDiseaseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EpidemicDiseaseSelections_EpidemicDiseases_EpidemicDiseaseId",
                        column: x => x.EpidemicDiseaseId,
                        principalTable: "EpidemicDiseases",
                        principalColumn: "EpidemicDiseaseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EpidemicDiseaseSelections_EpidemicDiseaseId",
                table: "EpidemicDiseaseSelections",
                column: "EpidemicDiseaseId");

            migrationBuilder.CreateIndex(
                name: "IX_EpidemicDiseaseSelections_EpidemicDiseaseTypeEpidemicDiseaseId",
                table: "EpidemicDiseaseSelections",
                column: "EpidemicDiseaseTypeEpidemicDiseaseId");

            migrationBuilder.CreateIndex(
                name: "IX_EpidemicDiseaseSelections_EpidemicDiseaseTypeId",
                table: "EpidemicDiseaseSelections",
                column: "EpidemicDiseaseTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EpidemicDiseaseSelections");
        }
    }
}
