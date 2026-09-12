using EarlyWarning.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations;

[DbContext(typeof(EarlyWarningDbContext))]
[Migration("20260812143000_RemoveLegacyRequiredReportStatusColumns")]
public partial class RemoveLegacyRequiredReportStatusColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DECLARE @Tables TABLE ([Name] sysname);
            INSERT INTO @Tables ([Name]) VALUES
                (N'AnimalPrice'),
                (N'AnimalPricePerUnits'),
                (N'AnimalSupplys'),
                (N'AssistanceRecipients'),
                (N'Deaths'),
                (N'GrainPricePerQuintals'),
                (N'GrainPrices'),
                (N'GrainSupplys'),
                (N'Migrations'),
                (N'OtherProblems'),
                (N'WeeklyAccidents');

            DECLARE @TableName sysname;
            DECLARE @ConstraintName sysname;
            DECLARE @Sql nvarchar(max);
            DECLARE report_status_cursor CURSOR LOCAL FAST_FORWARD FOR
                SELECT [Name] FROM @Tables;

            OPEN report_status_cursor;
            FETCH NEXT FROM report_status_cursor INTO @TableName;
            WHILE @@FETCH_STATUS = 0
            BEGIN
                IF OBJECT_ID(N'dbo.' + @TableName, N'U') IS NOT NULL
                   AND COL_LENGTH(N'dbo.' + @TableName, N'ReportStatus') IS NOT NULL
                BEGIN
                    IF COL_LENGTH(N'dbo.' + @TableName, N'ApprovalStatus') IS NOT NULL
                    BEGIN
                        SET @Sql = N'UPDATE [dbo].' + QUOTENAME(@TableName)
                            + N' SET [ApprovalStatus] = [ReportStatus];';
                        EXEC sys.sp_executesql @Sql;
                    END;

                    SET @ConstraintName = NULL;
                    SELECT @ConstraintName = dc.[name]
                    FROM sys.default_constraints AS dc
                    INNER JOIN sys.columns AS c
                      ON c.[object_id] = dc.[parent_object_id]
                     AND c.[column_id] = dc.[parent_column_id]
                    WHERE dc.[parent_object_id] = OBJECT_ID(N'dbo.' + @TableName)
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
                FETCH NEXT FROM report_status_cursor INTO @TableName;
            END;
            CLOSE report_status_cursor;
            DEALLOCATE report_status_cursor;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // ReportStatus is obsolete and unmapped; recreating it would block inserts.
    }
}
