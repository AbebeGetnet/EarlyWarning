using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttirbuteKebeleIdOnWater : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HumanDrinkWaterKebeles_Locations_KebeleId",
                table: "HumanDrinkWaterKebeles");

            migrationBuilder.DropColumn(
                name: "KebeleListId",
                table: "HumanDrinkWaterKebeles");

            migrationBuilder.AlterColumn<Guid>(
                name: "KebeleId",
                table: "HumanDrinkWaterKebeles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_HumanDrinkWaterKebeles_Locations_KebeleId",
                table: "HumanDrinkWaterKebeles",
                column: "KebeleId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HumanDrinkWaterKebeles_Locations_KebeleId",
                table: "HumanDrinkWaterKebeles");

            migrationBuilder.AlterColumn<Guid>(
                name: "KebeleId",
                table: "HumanDrinkWaterKebeles",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "KebeleListId",
                table: "HumanDrinkWaterKebeles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_HumanDrinkWaterKebeles_Locations_KebeleId",
                table: "HumanDrinkWaterKebeles",
                column: "KebeleId",
                principalTable: "Locations",
                principalColumn: "Id");
        }
    }
}
