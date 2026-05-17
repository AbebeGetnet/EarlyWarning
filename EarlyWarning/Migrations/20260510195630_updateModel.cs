using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class updateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnimalPricePerUnits_AnimalTypes_AnimalTypeId",
                table: "AnimalPricePerUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_AnimalPricePerUnits_GrainTypes_GrainTypeId",
                table: "AnimalPricePerUnits");

            migrationBuilder.DropIndex(
                name: "IX_AnimalPricePerUnits_GrainTypeId",
                table: "AnimalPricePerUnits");

            migrationBuilder.DropColumn(
                name: "GrainTypeId",
                table: "AnimalPricePerUnits");

            migrationBuilder.AlterColumn<string>(
                name: "ProblemName",
                table: "OtherProblems",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalNotes",
                table: "HumanDrinkWaterIssues",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionAdditionalNotes",
                table: "HumanDrinkWaterIssues",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegionApproval",
                table: "HumanDrinkWaterIssues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RegionalReportedBy",
                table: "HumanDrinkWaterIssues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ZonalApproval",
                table: "HumanDrinkWaterIssues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ZoneAdditionalNotes",
                table: "HumanDrinkWaterIssues",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZoneReportedBy",
                table: "HumanDrinkWaterIssues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AnimalTypeId",
                table: "AnimalPricePerUnits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalPricePerUnits_AnimalTypes_AnimalTypeId",
                table: "AnimalPricePerUnits",
                column: "AnimalTypeId",
                principalTable: "AnimalTypes",
                principalColumn: "AnimalTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnimalPricePerUnits_AnimalTypes_AnimalTypeId",
                table: "AnimalPricePerUnits");

            migrationBuilder.DropColumn(
                name: "AdditionalNotes",
                table: "HumanDrinkWaterIssues");

            migrationBuilder.DropColumn(
                name: "RegionAdditionalNotes",
                table: "HumanDrinkWaterIssues");

            migrationBuilder.DropColumn(
                name: "RegionApproval",
                table: "HumanDrinkWaterIssues");

            migrationBuilder.DropColumn(
                name: "RegionalReportedBy",
                table: "HumanDrinkWaterIssues");

            migrationBuilder.DropColumn(
                name: "ZonalApproval",
                table: "HumanDrinkWaterIssues");

            migrationBuilder.DropColumn(
                name: "ZoneAdditionalNotes",
                table: "HumanDrinkWaterIssues");

            migrationBuilder.DropColumn(
                name: "ZoneReportedBy",
                table: "HumanDrinkWaterIssues");

            migrationBuilder.AlterColumn<string>(
                name: "ProblemName",
                table: "OtherProblems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<Guid>(
                name: "AnimalTypeId",
                table: "AnimalPricePerUnits",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "GrainTypeId",
                table: "AnimalPricePerUnits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AnimalPricePerUnits_GrainTypeId",
                table: "AnimalPricePerUnits",
                column: "GrainTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalPricePerUnits_AnimalTypes_AnimalTypeId",
                table: "AnimalPricePerUnits",
                column: "AnimalTypeId",
                principalTable: "AnimalTypes",
                principalColumn: "AnimalTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalPricePerUnits_GrainTypes_GrainTypeId",
                table: "AnimalPricePerUnits",
                column: "GrainTypeId",
                principalTable: "GrainTypes",
                principalColumn: "GrainTypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
