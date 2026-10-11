using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Locations",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Locations",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "Latitude", "Longitude" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "Latitude", "Longitude" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "Latitude", "Longitude" },
                values: new object[] { null, null });

            // Geospatial point for SQL spatial queries (distance, nearest, within-area), derived from the
            // two coordinate columns. It is not mapped in EF: the app reads/writes Latitude/Longitude only.
            // Note: altering Latitude/Longitude later requires dropping this column and index first.
            migrationBuilder.Sql(@"
ALTER TABLE [Locations] ADD [GeoPoint] AS (
    CASE WHEN [Latitude] IS NOT NULL AND [Longitude] IS NOT NULL
         THEN geography::Point([Latitude], [Longitude], 4326) END) PERSISTED;");
            migrationBuilder.Sql("CREATE SPATIAL INDEX [SIX_Locations_GeoPoint] ON [Locations] ([GeoPoint]);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX [SIX_Locations_GeoPoint] ON [Locations];");
            migrationBuilder.Sql("ALTER TABLE [Locations] DROP COLUMN [GeoPoint];");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Locations");
        }
    }
}
