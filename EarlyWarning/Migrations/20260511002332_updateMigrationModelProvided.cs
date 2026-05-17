using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class updateMigrationModelProvided : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyProvideds_SupplyTypes_SupplyTypeId",
                table: "WeeklyProvideds");

            migrationBuilder.DropColumn(
                name: "DistributedQuantity",
                table: "WeeklyProvideds");

            migrationBuilder.DropColumn(
                name: "Donor",
                table: "WeeklyProvideds");

            migrationBuilder.DropColumn(
                name: "Measurement",
                table: "WeeklyProvideds");

            migrationBuilder.DropColumn(
                name: "PercentageOfDistributedFromProvided",
                table: "WeeklyProvideds");

            migrationBuilder.DropColumn(
                name: "ProvidedQuantity",
                table: "WeeklyProvideds");

            migrationBuilder.AlterColumn<Guid>(
                name: "SupplyTypeId",
                table: "WeeklyProvideds",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateTable(
                name: "WeeklyProvidedDetail",
                columns: table => new
                {
                    WeeklyProvidedDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeeklyProvidedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplyTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Measurement = table.Column<int>(type: "int", nullable: false),
                    ProvidedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DistributedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PercentageOfDistributedFromProvided = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Donor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyProvidedDetail", x => x.WeeklyProvidedDetailId);
                    table.ForeignKey(
                        name: "FK_WeeklyProvidedDetail_SupplyTypes_SupplyTypeId",
                        column: x => x.SupplyTypeId,
                        principalTable: "SupplyTypes",
                        principalColumn: "SupplyTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeeklyProvidedDetail_WeeklyProvideds_WeeklyProvidedId",
                        column: x => x.WeeklyProvidedId,
                        principalTable: "WeeklyProvideds",
                        principalColumn: "WeeklyProvidedId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyProvidedDetail_SupplyTypeId",
                table: "WeeklyProvidedDetail",
                column: "SupplyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyProvidedDetail_WeeklyProvidedId",
                table: "WeeklyProvidedDetail",
                column: "WeeklyProvidedId");

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyProvideds_SupplyTypes_SupplyTypeId",
                table: "WeeklyProvideds",
                column: "SupplyTypeId",
                principalTable: "SupplyTypes",
                principalColumn: "SupplyTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyProvideds_SupplyTypes_SupplyTypeId",
                table: "WeeklyProvideds");

            migrationBuilder.DropTable(
                name: "WeeklyProvidedDetail");

            migrationBuilder.AlterColumn<Guid>(
                name: "SupplyTypeId",
                table: "WeeklyProvideds",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DistributedQuantity",
                table: "WeeklyProvideds",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Donor",
                table: "WeeklyProvideds",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Measurement",
                table: "WeeklyProvideds",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PercentageOfDistributedFromProvided",
                table: "WeeklyProvideds",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProvidedQuantity",
                table: "WeeklyProvideds",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyProvideds_SupplyTypes_SupplyTypeId",
                table: "WeeklyProvideds",
                column: "SupplyTypeId",
                principalTable: "SupplyTypes",
                principalColumn: "SupplyTypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
