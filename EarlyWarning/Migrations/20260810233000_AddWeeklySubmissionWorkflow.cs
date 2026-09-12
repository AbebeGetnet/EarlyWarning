using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace EarlyWarning.Migrations;

public partial class AddWeeklySubmissionWorkflow : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => Apply(migrationBuilder);
    internal static void Apply(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "WeeklyReportSubmissions", columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            StartDate = table.Column<DateTime>(type: "datetime2", nullable: false), EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            Status = table.Column<int>(type: "int", nullable: false), CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false), LastActionAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
        }, constraints: table => { table.PrimaryKey("PK_WeeklyReportSubmissions", x => x.Id); table.ForeignKey("FK_WeeklyReportSubmissions_AspNetUsers_CreatedById", x => x.CreatedById, "AspNetUsers", "Id", onDelete: ReferentialAction.Restrict); table.ForeignKey("FK_WeeklyReportSubmissions_Locations_WoredaId", x => x.WoredaId, "Locations", "Id", onDelete: ReferentialAction.Restrict); });
        migrationBuilder.CreateTable(name: "WeeklyReportWorkflowHistories", columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), WeeklyReportSubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            UserId = table.Column<string>(type: "nvarchar(450)", nullable: false), ActorLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ActorLocationLevel = table.Column<int>(type: "int", nullable: false), Action = table.Column<int>(type: "int", nullable: false),
            PreviousStatus = table.Column<int>(type: "int", nullable: true), NewStatus = table.Column<int>(type: "int", nullable: false),
            Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true), PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
        }, constraints: table => { table.PrimaryKey("PK_WeeklyReportWorkflowHistories", x => x.Id); table.ForeignKey("FK_WeeklyReportWorkflowHistories_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", onDelete: ReferentialAction.Restrict); table.ForeignKey("FK_WeeklyReportWorkflowHistories_WeeklyReportSubmissions_WeeklyReportSubmissionId", x => x.WeeklyReportSubmissionId, "WeeklyReportSubmissions", "Id", onDelete: ReferentialAction.Cascade); });
        migrationBuilder.CreateIndex("IX_WeeklyReportSubmissions_CreatedById", "WeeklyReportSubmissions", "CreatedById");
        migrationBuilder.CreateIndex("IX_WeeklyReportSubmissions_WoredaId_StartDate_EndDate", "WeeklyReportSubmissions", new[] { "WoredaId", "StartDate", "EndDate" }, unique: true);
        migrationBuilder.CreateIndex("IX_WeeklyReportWorkflowHistories_UserId", "WeeklyReportWorkflowHistories", "UserId");
        migrationBuilder.CreateIndex("IX_WeeklyReportWorkflowHistories_WeeklyReportSubmissionId", "WeeklyReportWorkflowHistories", "WeeklyReportSubmissionId");
    }
    protected override void Down(MigrationBuilder migrationBuilder) { migrationBuilder.DropTable("WeeklyReportWorkflowHistories"); migrationBuilder.DropTable("WeeklyReportSubmissions"); }
}
