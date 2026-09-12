using EarlyWarning.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    [DbContext(typeof(EarlyWarningDbContext))]
    [Migration("20260810160000_RepairMissingWorkflowStatusColumns")]
    public partial class RepairMissingWorkflowStatusColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Repair databases whose physical schema drifted from the EF model.
            // WITH VALUES initializes existing reports as Draft (enum value 0).
            migrationBuilder.Sql(
                """
                DECLARE @TableName sysname;
                DECLARE @Sql nvarchar(max);

                DECLARE WorkflowTables CURSOR LOCAL FAST_FORWARD FOR
                    SELECT [TableName]
                    FROM (VALUES
                        (N'AnimalHealthStatuses'),
                        (N'AnimalWaterSupplyStatuses'),
                        (N'CropGrowths'),
                        (N'CropPestAndDeseaseReports'),
                        (N'FarmingActivities'),
                        (N'PastureStatuses'),
                        (N'RainfallReports')
                    ) AS ExpectedTables([TableName]);

                OPEN WorkflowTables;
                FETCH NEXT FROM WorkflowTables INTO @TableName;

                WHILE @@FETCH_STATUS = 0
                BEGIN
                    IF OBJECT_ID(N'dbo.' + @TableName, N'U') IS NOT NULL
                       AND COL_LENGTH(N'dbo.' + @TableName, N'Status') IS NULL
                    BEGIN
                        SET @Sql = N'ALTER TABLE [dbo].' + QUOTENAME(@TableName)
                            + N' ADD [Status] int NOT NULL DEFAULT (0) WITH VALUES;';
                        EXEC sys.sp_executesql @Sql;
                    END;

                    FETCH NEXT FROM WorkflowTables INTO @TableName;
                END;

                CLOSE WorkflowTables;
                DEALLOCATE WorkflowTables;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally non-destructive: these columns are required by the
            // current model and may have existed before this repair migration.
        }
    }
}
