using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace EarlyWarning.Migrations;

public partial class AddUniqueWoredaReportingWeek : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => Apply(migrationBuilder);
    internal static void Apply(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(name: "ReportingWeekStart", table: "WeeklyReportSubmissions", type: "datetime2", nullable: false, defaultValue: new DateTime(1, 1, 1));
        migrationBuilder.Sql("UPDATE WeeklyReportSubmissions SET ReportingWeekStart = DATEADD(day, -(DATEDIFF(day, '19000101', CAST(StartDate AS date)) % 7), CAST(StartDate AS date))");
        migrationBuilder.CreateIndex(name: "IX_WeeklyReportSubmissions_WoredaId_ReportingWeekStart", table: "WeeklyReportSubmissions", columns: new[] { "WoredaId", "ReportingWeekStart" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_WeeklyReportSubmissions_WoredaId_ReportingWeekStart", table: "WeeklyReportSubmissions");
        migrationBuilder.DropColumn(name: "ReportingWeekStart", table: "WeeklyReportSubmissions");
    }
}
