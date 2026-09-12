using EarlyWarning.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    [DbContext(typeof(EarlyWarningDbContext))]
    [Migration("20260811124500_RemoveAllLegacyDuplicateReportStatusColumns")]
    public partial class RemoveAllLegacyDuplicateReportStatusColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DECLARE @SchemaName sysname;
                DECLARE @TableName sysname;
                DECLARE @ConstraintName sysname;
                DECLARE @Sql nvarchar(max);

                -- A table containing both columns is a legacy CommonAttribute table:
                -- EF maps Status, while the old required ReportStatus column blocks inserts.
                DECLARE duplicate_status_cursor CURSOR LOCAL FAST_FORWARD FOR
                    SELECT s.[name], t.[name]
                    FROM sys.tables AS t
                    INNER JOIN sys.schemas AS s ON s.[schema_id] = t.[schema_id]
                    WHERE EXISTS (
                        SELECT 1 FROM sys.columns AS c
                        WHERE c.[object_id] = t.[object_id] AND c.[name] = N'Status')
                      AND EXISTS (
                        SELECT 1 FROM sys.columns AS c
                        WHERE c.[object_id] = t.[object_id] AND c.[name] = N'ReportStatus');

                OPEN duplicate_status_cursor;
                FETCH NEXT FROM duplicate_status_cursor INTO @SchemaName, @TableName;

                WHILE @@FETCH_STATUS = 0
                BEGIN
                    SET @Sql = N'UPDATE ' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName)
                        + N' SET [Status] = [ReportStatus];';
                    EXEC sys.sp_executesql @Sql;

                    SET @ConstraintName = NULL;
                    SELECT @ConstraintName = dc.[name]
                    FROM sys.default_constraints AS dc
                    INNER JOIN sys.columns AS c
                        ON c.[object_id] = dc.[parent_object_id]
                       AND c.[column_id] = dc.[parent_column_id]
                    INNER JOIN sys.tables AS t ON t.[object_id] = c.[object_id]
                    INNER JOIN sys.schemas AS s ON s.[schema_id] = t.[schema_id]
                    WHERE s.[name] = @SchemaName
                      AND t.[name] = @TableName
                      AND c.[name] = N'ReportStatus';

                    IF @ConstraintName IS NOT NULL
                    BEGIN
                        SET @Sql = N'ALTER TABLE ' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName)
                            + N' DROP CONSTRAINT ' + QUOTENAME(@ConstraintName) + N';';
                        EXEC sys.sp_executesql @Sql;
                    END;

                    SET @Sql = N'ALTER TABLE ' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName)
                        + N' DROP COLUMN [ReportStatus];';
                    EXEC sys.sp_executesql @Sql;

                    FETCH NEXT FROM duplicate_status_cursor INTO @SchemaName, @TableName;
                END;

                CLOSE duplicate_status_cursor;
                DEALLOCATE duplicate_status_cursor;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The removed columns are obsolete and unmapped. Recreating them as
            // NOT NULL columns would make EF inserts fail again.
        }
    }
}
