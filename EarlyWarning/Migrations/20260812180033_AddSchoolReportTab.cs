using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolReportTab : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchoolReports",
                columns: table => new
                {
                    SchoolReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HasTeachingLearningProblem = table.Column<bool>(type: "bit", nullable: false),
                    Cause = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MaleStudentDropouts = table.Column<int>(type: "int", nullable: false),
                    FemaleStudentDropouts = table.Column<int>(type: "int", nullable: false),
                    AverageDropoutAge = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ClosedSchools = table.Column<int>(type: "int", nullable: false),
                    MajorSolutionOne = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MajorSolutionTwo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MajorSolutionThree = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WasActionSatisfactory = table.Column<bool>(type: "bit", nullable: true),
                    StudentRelatedSolution = table.Column<bool>(type: "bit", nullable: false),
                    TeacherExpenseSolution = table.Column<bool>(type: "bit", nullable: false),
                    ClassroomMaterialsSolution = table.Column<bool>(type: "bit", nullable: false),
                    SchoolBuildingServiceSolution = table.Column<bool>(type: "bit", nullable: false),
                    OtherSolution = table.Column<bool>(type: "bit", nullable: false),
                    OtherSolutionDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolReports", x => x.SchoolReportId);
                    table.ForeignKey(
                        name: "FK_SchoolReports_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolReports_WoredaId_StartDate_EndDate",
                table: "SchoolReports",
                columns: new[] { "WoredaId", "StartDate", "EndDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchoolReports");
        }
    }
}
