using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using EarlyWarning.Data;

#nullable disable

namespace EarlyWarning.Migrations;

[DbContext(typeof(EarlyWarningDbContext))]
[Migration("20260812165500_CreateWeeklyAggregateSnapshotStorage")]
public partial class CreateWeeklyAggregateSnapshotStorage : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "WeeklyAggregateSnapshots",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ScopeLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ScopeLevel = table.Column<int>(type: "int", nullable: false),
                StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ApprovalStatus = table.Column<int>(type: "int", nullable: false),
                SourceReportCount = table.Column<int>(type: "int", nullable: false),
                GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                GeneratedById = table.Column<string>(type: "nvarchar(max)", nullable: false),
                AggregateJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_WeeklyAggregateSnapshots", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_WeeklyAggregateSnapshots_ScopeLocationId_ScopeLevel_StartDate_EndDate_ApprovalStatus",
            table: "WeeklyAggregateSnapshots",
            columns: new[] { "ScopeLocationId", "ScopeLevel", "StartDate", "EndDate", "ApprovalStatus" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "WeeklyAggregateSnapshots");
}
