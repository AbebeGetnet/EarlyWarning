using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class AddAnimalMigrationRoutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnimalMigrationRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeeklyReportSubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    OriginLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnimalType = table.Column<int>(type: "int", nullable: false),
                    OtherAnimalType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NumberOfAnimals = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalMigrationRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnimalMigrationRecords_Locations_DestinationLocationId",
                        column: x => x.DestinationLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnimalMigrationRecords_Locations_OriginLocationId",
                        column: x => x.OriginLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnimalMigrationRecords_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnimalMigrationRecords_WeeklyReportSubmissions_WeeklyReportSubmissionId",
                        column: x => x.WeeklyReportSubmissionId,
                        principalTable: "WeeklyReportSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnimalMigrationRecords_DestinationLocationId",
                table: "AnimalMigrationRecords",
                column: "DestinationLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalMigrationRecords_OriginLocationId",
                table: "AnimalMigrationRecords",
                column: "OriginLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalMigrationRecords_WeeklyReportSubmissionId_Direction_OriginLocationId_DestinationLocationId_AnimalType",
                table: "AnimalMigrationRecords",
                columns: new[] { "WeeklyReportSubmissionId", "Direction", "OriginLocationId", "DestinationLocationId", "AnimalType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnimalMigrationRecords_WoredaId",
                table: "AnimalMigrationRecords",
                column: "WoredaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnimalMigrationRecords");
        }
    }
}
