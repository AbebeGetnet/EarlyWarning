using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class AddAnimalBodyCondition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnimalBodyCondition",
                table: "AnimalWaterSupplyStatuses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LowBodyConditionReason",
                table: "AnimalWaterSupplyStatuses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnimalBodyCondition",
                table: "AnimalWaterSupplyStatuses");

            migrationBuilder.DropColumn(
                name: "LowBodyConditionReason",
                table: "AnimalWaterSupplyStatuses");
        }
    }
}
