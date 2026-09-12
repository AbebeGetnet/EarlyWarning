using EarlyWarning.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    [DbContext(typeof(EarlyWarningDbContext))]
    [Migration("20260811123000_RemoveLegacyFarmAndCropReportStatus")]
    public partial class RemoveLegacyFarmAndCropReportStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DECLARE @Tables TABLE ([Name] sysname);
                INSERT INTO @Tables ([Name]) VALUES
                    (N'FarmingActivities'),
                    (N'CropPestAndDeseaseReports');

                DECLARE @TableName sysname;
                DECLARE @ConstraintName sysname;
                DECLARE @Sql nvarchar(max);

                DECLARE legacy_status_cursor CURSOR LOCAL FAST_FORWARD FOR
                    SELECT [Name] FROM @Tables;

                OPEN legacy_status_cursor;
                FETCH NEXT FROM legacy_status_cursor INTO @TableName;

                WHILE @@FETCH_STATUS = 0
                BEGIN
                    IF OBJECT_ID(N'dbo.' + @TableName, N'U') IS NOT NULL
                       AND COL_LENGTH(N'dbo.' + @TableName, N'ReportStatus') IS NOT NULL
                    BEGIN
                        IF COL_LENGTH(N'dbo.' + @TableName, N'Status') IS NULL
                        BEGIN
                            SET @Sql = N'ALTER TABLE [dbo].' + QUOTENAME(@TableName)
                                + N' ADD [Status] int NOT NULL CONSTRAINT '
                                + QUOTENAME(N'DF_' + @TableName + N'_Status')
                                + N' DEFAULT (0) WITH VALUES;';
                            EXEC sys.sp_executesql @Sql;
                        END;

                        SET @Sql = N'UPDATE [dbo].' + QUOTENAME(@TableName)
                            + N' SET [Status] = [ReportStatus];';
                        EXEC sys.sp_executesql @Sql;

                        SET @ConstraintName = NULL;
                        SELECT @ConstraintName = dc.[name]
                        FROM sys.default_constraints AS dc
                        INNER JOIN sys.columns AS c
                            ON c.[object_id] = dc.parent_object_id
                           AND c.[column_id] = dc.parent_column_id
                        WHERE dc.parent_object_id = OBJECT_ID(N'dbo.' + @TableName)
                          AND c.[name] = N'ReportStatus';

                        IF @ConstraintName IS NOT NULL
                        BEGIN
                            SET @Sql = N'ALTER TABLE [dbo].' + QUOTENAME(@TableName)
                                + N' DROP CONSTRAINT ' + QUOTENAME(@ConstraintName) + N';';
                            EXEC sys.sp_executesql @Sql;
                        END;

                        SET @Sql = N'ALTER TABLE [dbo].' + QUOTENAME(@TableName)
                            + N' DROP COLUMN [ReportStatus];';
                        EXEC sys.sp_executesql @Sql;
                    END;

                    FETCH NEXT FROM legacy_status_cursor INTO @TableName;
                END;

                CLOSE legacy_status_cursor;
                DEALLOCATE legacy_status_cursor;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ReportStatus is obsolete and unmapped. Recreating it would restore
            // the NOT NULL insert failure fixed by this migration.
        }
    }
}
