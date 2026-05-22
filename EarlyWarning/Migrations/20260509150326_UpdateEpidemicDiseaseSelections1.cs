using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEpidemicDiseaseSelections1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EpidemicDiseaseSelections_EpidemicDiseaseTypes_EpidemicDiseaseTypeEpidemicDiseaseId",
                table: "EpidemicDiseaseSelections");

            migrationBuilder.RenameColumn(
                name: "EpidemicDiseaseId",
                table: "EpidemicDiseaseTypes",
                newName: "EpidemicDiseaseTypeId");

            migrationBuilder.RenameColumn(
                name: "EpidemicDiseaseTypeEpidemicDiseaseId",
                table: "EpidemicDiseaseSelections",
                newName: "EpidemicDiseaseTypeId1");

            migrationBuilder.RenameIndex(
                name: "IX_EpidemicDiseaseSelections_EpidemicDiseaseTypeEpidemicDiseaseId",
                table: "EpidemicDiseaseSelections",
                newName: "IX_EpidemicDiseaseSelections_EpidemicDiseaseTypeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_EpidemicDiseaseSelections_EpidemicDiseaseTypes_EpidemicDiseaseTypeId1",
                table: "EpidemicDiseaseSelections",
                column: "EpidemicDiseaseTypeId1",
                principalTable: "EpidemicDiseaseTypes",
                principalColumn: "EpidemicDiseaseTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EpidemicDiseaseSelections_EpidemicDiseaseTypes_EpidemicDiseaseTypeId1",
                table: "EpidemicDiseaseSelections");

            migrationBuilder.RenameColumn(
                name: "EpidemicDiseaseTypeId",
                table: "EpidemicDiseaseTypes",
                newName: "EpidemicDiseaseId");

            migrationBuilder.RenameColumn(
                name: "EpidemicDiseaseTypeId1",
                table: "EpidemicDiseaseSelections",
                newName: "EpidemicDiseaseTypeEpidemicDiseaseId");

            migrationBuilder.RenameIndex(
                name: "IX_EpidemicDiseaseSelections_EpidemicDiseaseTypeId1",
                table: "EpidemicDiseaseSelections",
                newName: "IX_EpidemicDiseaseSelections_EpidemicDiseaseTypeEpidemicDiseaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_EpidemicDiseaseSelections_EpidemicDiseaseTypes_EpidemicDiseaseTypeEpidemicDiseaseId",
                table: "EpidemicDiseaseSelections",
                column: "EpidemicDiseaseTypeEpidemicDiseaseId",
                principalTable: "EpidemicDiseaseTypes",
                principalColumn: "EpidemicDiseaseId");
        }
    }
}
