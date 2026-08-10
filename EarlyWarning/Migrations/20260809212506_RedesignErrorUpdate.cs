using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class RedesignErrorUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Some deployed databases already had one or more of these legacy
            // columns removed manually. Drop only columns that are still present,
            // including any SQL Server-generated default constraint attached to
            // them, so this migration can reconcile those databases safely.
            migrationBuilder.Sql(
                """
                DECLARE @ColumnName sysname;
                DECLARE @ConstraintName sysname;
                DECLARE @Sql nvarchar(max);
                DECLARE LegacyLocationColumns CURSOR LOCAL FAST_FORWARD FOR
                    SELECT [ColumnName]
                    FROM (VALUES
                        (N'DeletedAt'),
                        (N'EndDate'),
                        (N'LasModifiedAt'),
                        (N'RegionApprovedAt'),
                        (N'RegionApprovedById'),
                        (N'StartDate'),
                        (N'Status'),
                        (N'SubmittedAt'),
                        (N'SubmittedById'),
                        (N'UserId'),
                        (N'ZoneApprovedAt'),
                        (N'ZoneApprovedById')
                    ) AS LegacyColumns([ColumnName]);

                OPEN LegacyLocationColumns;
                FETCH NEXT FROM LegacyLocationColumns INTO @ColumnName;

                WHILE @@FETCH_STATUS = 0
                BEGIN
                    IF COL_LENGTH(N'dbo.Locations', @ColumnName) IS NOT NULL
                    BEGIN
                        SELECT @ConstraintName = dc.[name]
                        FROM sys.default_constraints AS dc
                        INNER JOIN sys.columns AS c
                            ON c.[object_id] = dc.parent_object_id
                           AND c.column_id = dc.parent_column_id
                        WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Locations')
                          AND c.[name] = @ColumnName;

                        IF @ConstraintName IS NOT NULL
                        BEGIN
                            SET @Sql = N'ALTER TABLE [dbo].[Locations] DROP CONSTRAINT '
                                + QUOTENAME(@ConstraintName) + N';';
                            EXEC sys.sp_executesql @Sql;
                        END;

                        SET @Sql = N'ALTER TABLE [dbo].[Locations] DROP COLUMN '
                            + QUOTENAME(@ColumnName) + N';';
                        EXEC sys.sp_executesql @Sql;
                    END;

                    SET @ConstraintName = NULL;
                    FETCH NEXT FROM LegacyLocationColumns INTO @ColumnName;
                END;

                CLOSE LegacyLocationColumns;
                DEALLOCATE LegacyLocationColumns;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Locations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Locations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LasModifiedAt",
                table: "Locations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RegionApprovedAt",
                table: "Locations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionApprovedById",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Locations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Locations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "Locations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmittedById",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ZoneApprovedAt",
                table: "Locations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZoneApprovedById",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "DeletedAt", "EndDate", "LasModifiedAt", "RegionApprovedAt", "RegionApprovedById", "StartDate", "Status", "SubmittedAt", "SubmittedById", "UserId", "ZoneApprovedAt", "ZoneApprovedById" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, 0, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "DeletedAt", "EndDate", "LasModifiedAt", "RegionApprovedAt", "RegionApprovedById", "StartDate", "Status", "SubmittedAt", "SubmittedById", "UserId", "ZoneApprovedAt", "ZoneApprovedById" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, 0, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "DeletedAt", "EndDate", "LasModifiedAt", "RegionApprovedAt", "RegionApprovedById", "StartDate", "Status", "SubmittedAt", "SubmittedById", "UserId", "ZoneApprovedAt", "ZoneApprovedById" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, 0, null, null, null, null, null });
        }
    }
}
