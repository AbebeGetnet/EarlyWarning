using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class updateModelAnimalModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GrainPricePerQuintalId",
                table: "AnimalPricePerUnits",
                newName: "AnimalPricePerUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AnimalPricePerUnitId",
                table: "AnimalPricePerUnits",
                newName: "GrainPricePerQuintalId");
        }
    }
}
