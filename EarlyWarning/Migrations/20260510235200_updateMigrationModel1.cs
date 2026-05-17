using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class updateMigrationModel1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyProvideds_SupplyTypes_SupplyTypeId",
                table: "WeeklyProvideds");

            migrationBuilder.AlterColumn<Guid>(
                name: "SupplyTypeId",
                table: "WeeklyProvideds",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyProvideds_SupplyTypes_SupplyTypeId",
                table: "WeeklyProvideds",
                column: "SupplyTypeId",
                principalTable: "SupplyTypes",
                principalColumn: "SupplyTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyProvideds_SupplyTypes_SupplyTypeId",
                table: "WeeklyProvideds");

            migrationBuilder.AlterColumn<Guid>(
                name: "SupplyTypeId",
                table: "WeeklyProvideds",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyProvideds_SupplyTypes_SupplyTypeId",
                table: "WeeklyProvideds",
                column: "SupplyTypeId",
                principalTable: "SupplyTypes",
                principalColumn: "SupplyTypeId");
        }
    }
}
