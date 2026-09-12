using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class AddPersistentWizardDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DraftDataJson",
                table: "WeeklyReportSubmissions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DraftDataJson",
                table: "WeeklyReportSubmissions");
        }
    }
}
