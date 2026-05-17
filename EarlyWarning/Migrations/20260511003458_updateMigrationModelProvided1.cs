using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class updateMigrationModelProvided1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyProvidedDetail_SupplyTypes_SupplyTypeId",
                table: "WeeklyProvidedDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyProvidedDetail_WeeklyProvideds_WeeklyProvidedId",
                table: "WeeklyProvidedDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WeeklyProvidedDetail",
                table: "WeeklyProvidedDetail");

            migrationBuilder.RenameTable(
                name: "WeeklyProvidedDetail",
                newName: "WeeklyProvidedDetails");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyProvidedDetail_WeeklyProvidedId",
                table: "WeeklyProvidedDetails",
                newName: "IX_WeeklyProvidedDetails_WeeklyProvidedId");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyProvidedDetail_SupplyTypeId",
                table: "WeeklyProvidedDetails",
                newName: "IX_WeeklyProvidedDetails_SupplyTypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WeeklyProvidedDetails",
                table: "WeeklyProvidedDetails",
                column: "WeeklyProvidedDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyProvidedDetails_SupplyTypes_SupplyTypeId",
                table: "WeeklyProvidedDetails",
                column: "SupplyTypeId",
                principalTable: "SupplyTypes",
                principalColumn: "SupplyTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyProvidedDetails_WeeklyProvideds_WeeklyProvidedId",
                table: "WeeklyProvidedDetails",
                column: "WeeklyProvidedId",
                principalTable: "WeeklyProvideds",
                principalColumn: "WeeklyProvidedId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyProvidedDetails_SupplyTypes_SupplyTypeId",
                table: "WeeklyProvidedDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyProvidedDetails_WeeklyProvideds_WeeklyProvidedId",
                table: "WeeklyProvidedDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WeeklyProvidedDetails",
                table: "WeeklyProvidedDetails");

            migrationBuilder.RenameTable(
                name: "WeeklyProvidedDetails",
                newName: "WeeklyProvidedDetail");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyProvidedDetails_WeeklyProvidedId",
                table: "WeeklyProvidedDetail",
                newName: "IX_WeeklyProvidedDetail_WeeklyProvidedId");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyProvidedDetails_SupplyTypeId",
                table: "WeeklyProvidedDetail",
                newName: "IX_WeeklyProvidedDetail_SupplyTypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WeeklyProvidedDetail",
                table: "WeeklyProvidedDetail",
                column: "WeeklyProvidedDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyProvidedDetail_SupplyTypes_SupplyTypeId",
                table: "WeeklyProvidedDetail",
                column: "SupplyTypeId",
                principalTable: "SupplyTypes",
                principalColumn: "SupplyTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyProvidedDetail_WeeklyProvideds_WeeklyProvidedId",
                table: "WeeklyProvidedDetail",
                column: "WeeklyProvidedId",
                principalTable: "WeeklyProvideds",
                principalColumn: "WeeklyProvidedId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
