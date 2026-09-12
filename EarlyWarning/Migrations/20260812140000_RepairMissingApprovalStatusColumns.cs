using EarlyWarning.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations;

[DbContext(typeof(EarlyWarningDbContext))]
[Migration("20260812140000_RepairMissingApprovalStatusColumns")]
public partial class RepairMissingApprovalStatusColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DECLARE @Tables TABLE ([Name] sysname);
            INSERT INTO @Tables ([Name]) VALUES
                (N'AssistanceRecipients'),
                (N'OtherProblems'),
                (N'AnimalPrice'),
                (N'AnimalPricePerUnits'),
                (N'AnimalSupplys'),
                (N'Deaths'),
                (N'Migrations'),
                (N'GrainPrices'),
                (N'GrainPricePerQuintals'),
                (N'GrainSupplys'),
                (N'WeeklyAccidents');

            DECLARE @TableName sysname;
            DECLARE @Sql nvarchar(max);
            DECLARE approval_cursor CURSOR LOCAL FAST_FORWARD FOR
                SELECT [Name] FROM @Tables;

            OPEN approval_cursor;
            FETCH NEXT FROM approval_cursor INTO @TableName;
            WHILE @@FETCH_STATUS = 0
            BEGIN
                IF OBJECT_ID(N'dbo.' + @TableName, N'U') IS NOT NULL
                   AND COL_LENGTH(N'dbo.' + @TableName, N'ApprovalStatus') IS NULL
                BEGIN
                    SET @Sql = N'ALTER TABLE [dbo].' + QUOTENAME(@TableName)
                        + N' ADD [ApprovalStatus] int NOT NULL CONSTRAINT '
                        + QUOTENAME(N'DF_' + @TableName + N'_ApprovalStatus')
                        + N' DEFAULT (0) WITH VALUES;';
                    EXEC sys.sp_executesql @Sql;
                END;
                FETCH NEXT FROM approval_cursor INTO @TableName;
            END;
            CLOSE approval_cursor;
            DEALLOCATE approval_cursor;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // This migration repairs schema drift. Removing valid mapped columns on
        // rollback would recreate the production failure.
    }
}
