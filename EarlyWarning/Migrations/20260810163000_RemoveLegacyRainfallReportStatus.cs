using EarlyWarning.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    [DbContext(typeof(EarlyWarningDbContext))]
    [Migration("20260810163000_RemoveLegacyRainfallReportStatus")]
    public partial class RemoveLegacyRainfallReportStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'dbo.RainfallReports', N'U') IS NOT NULL
                   AND COL_LENGTH(N'dbo.RainfallReports', N'ReportStatus') IS NOT NULL
                BEGIN
                    -- Preserve legacy workflow state before removing the column
                    -- that is no longer mapped by RainfallReport.
                    IF COL_LENGTH(N'dbo.RainfallReports', N'Status') IS NULL
                        ALTER TABLE [dbo].[RainfallReports]
                            ADD [Status] int NOT NULL DEFAULT (0) WITH VALUES;

                    UPDATE [dbo].[RainfallReports]
                    SET [Status] = [ReportStatus];

                    DECLARE @ConstraintName sysname;
                    DECLARE @Sql nvarchar(max);

                    SELECT @ConstraintName = dc.[name]
                    FROM sys.default_constraints AS dc
                    INNER JOIN sys.columns AS c
                        ON c.[object_id] = dc.parent_object_id
                       AND c.column_id = dc.parent_column_id
                    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.RainfallReports')
                      AND c.[name] = N'ReportStatus';

                    IF @ConstraintName IS NOT NULL
                    BEGIN
                        SET @Sql = N'ALTER TABLE [dbo].[RainfallReports] DROP CONSTRAINT '
                            + QUOTENAME(@ConstraintName) + N';';
                        EXEC sys.sp_executesql @Sql;
                    END;

                    ALTER TABLE [dbo].[RainfallReports] DROP COLUMN [ReportStatus];
                END;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The legacy column is not part of the current EF model. Recreating
            // it would restore the insert failure this repair is intended to fix.
        }
    }
}
