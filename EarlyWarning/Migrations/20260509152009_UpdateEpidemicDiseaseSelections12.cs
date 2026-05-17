using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEpidemicDiseaseSelections12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfCases",
                table: "EpidemicDiseaseSelections");

            migrationBuilder.AddColumn<Guid>(
                name: "EpidemicDiseaseTypeId",
                table: "EpidemicDiseases",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_EpidemicDiseases_EpidemicDiseaseTypeId",
                table: "EpidemicDiseases",
                column: "EpidemicDiseaseTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EpidemicDiseases_EpidemicDiseaseTypes_EpidemicDiseaseTypeId",
                table: "EpidemicDiseases",
                column: "EpidemicDiseaseTypeId",
                principalTable: "EpidemicDiseaseTypes",
                principalColumn: "EpidemicDiseaseTypeId",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EpidemicDiseases_EpidemicDiseaseTypes_EpidemicDiseaseTypeId",
                table: "EpidemicDiseases");

            migrationBuilder.DropIndex(
                name: "IX_EpidemicDiseases_EpidemicDiseaseTypeId",
                table: "EpidemicDiseases");

            migrationBuilder.DropColumn(
                name: "EpidemicDiseaseTypeId",
                table: "EpidemicDiseases");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfCases",
                table: "EpidemicDiseaseSelections",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
