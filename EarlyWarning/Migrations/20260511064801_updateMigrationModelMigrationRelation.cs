using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class updateMigrationModelMigrationRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MigrationDetails_Migrations_MigrationReportId",
                table: "MigrationDetails");

            migrationBuilder.DropIndex(
                name: "IX_MigrationDetails_MigrationReportId",
                table: "MigrationDetails");

            migrationBuilder.DropColumn(
                name: "MigrationReportId",
                table: "MigrationDetails");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationDetails_MigrationId",
                table: "MigrationDetails",
                column: "MigrationId");

            migrationBuilder.AddForeignKey(
                name: "FK_MigrationDetails_Migrations_MigrationId",
                table: "MigrationDetails",
                column: "MigrationId",
                principalTable: "Migrations",
                principalColumn: "MigrationId",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MigrationDetails_Migrations_MigrationId",
                table: "MigrationDetails");

            migrationBuilder.DropIndex(
                name: "IX_MigrationDetails_MigrationId",
                table: "MigrationDetails");

            migrationBuilder.AddColumn<Guid>(
                name: "MigrationReportId",
                table: "MigrationDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MigrationDetails_MigrationReportId",
                table: "MigrationDetails",
                column: "MigrationReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_MigrationDetails_Migrations_MigrationReportId",
                table: "MigrationDetails",
                column: "MigrationReportId",
                principalTable: "Migrations",
                principalColumn: "MigrationId");
        }
    }
}
