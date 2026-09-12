using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations;

public partial class AddPerTypeImpactDetails : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => Apply(migrationBuilder);
    internal static void Apply(MigrationBuilder migrationBuilder)
    {
        string[] columns = { "MaleHouseholdHeads", "FemaleHouseholdHeads", "MaleFamilyMembers", "FemaleFamilyMembers", "MaleChildren", "FemaleChildren", "MaleYouth", "FemaleYouth", "MaleElderly", "FemaleElderly", "MaleDisabled", "FemaleDisabled", "LactatingMothers", "PregnantWomen" };
        foreach (var column in columns)
            migrationBuilder.AddColumn<int>(name: column, table: "EpidemicDiseaseSelections", type: "int", nullable: false, defaultValue: 0);

        migrationBuilder.CreateTable(name: "CropPestImpactDetails", columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), CropPestAndDeseaseReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), CropPestAndDeseaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            MaleHouseholdHeads = table.Column<int>(type: "int", nullable: false), FemaleHouseholdHeads = table.Column<int>(type: "int", nullable: false), MaleFamilyMembers = table.Column<int>(type: "int", nullable: false), FemaleFamilyMembers = table.Column<int>(type: "int", nullable: false), MaleChildren = table.Column<int>(type: "int", nullable: false), FemaleChildren = table.Column<int>(type: "int", nullable: false), MaleYouth = table.Column<int>(type: "int", nullable: false), FemaleYouth = table.Column<int>(type: "int", nullable: false), MaleElderly = table.Column<int>(type: "int", nullable: false), FemaleElderly = table.Column<int>(type: "int", nullable: false), MaleDisabled = table.Column<int>(type: "int", nullable: false), FemaleDisabled = table.Column<int>(type: "int", nullable: false), AffectedLandInHectares = table.Column<decimal>(type: "decimal(18,2)", nullable: false), AffectedCropType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true), Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
        }, constraints: table => { table.PrimaryKey("PK_CropPestImpactDetails", x => x.Id); table.ForeignKey("FK_CropPestImpactDetails_CropPestAndDesease_CropPestAndDeseaseId", x => x.CropPestAndDeseaseId, "CropPestAndDesease", "Id", onDelete: ReferentialAction.Cascade); table.ForeignKey("FK_CropPestImpactDetails_CropPestAndDeseaseReports_CropPestAndDeseaseReportId", x => x.CropPestAndDeseaseReportId, "CropPestAndDeseaseReports", "Id", onDelete: ReferentialAction.Cascade); });
        migrationBuilder.CreateIndex("IX_CropPestImpactDetails_CropPestAndDeseaseId", "CropPestImpactDetails", "CropPestAndDeseaseId");
        migrationBuilder.CreateIndex("IX_CropPestImpactDetails_Report_Pest", "CropPestImpactDetails", new[] { "CropPestAndDeseaseReportId", "CropPestAndDeseaseId" }, unique: true);

        migrationBuilder.CreateTable(name: "AnimalDiseaseImpactDetails", columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), AnimalHealthStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), AnimalDiseaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), NumberOfAffectedAnimals = table.Column<int>(type: "int", nullable: false), MaleHouseholdHeads = table.Column<int>(type: "int", nullable: false), FemaleHouseholdHeads = table.Column<int>(type: "int", nullable: false), MaleFamilyMembers = table.Column<int>(type: "int", nullable: false), FemaleFamilyMembers = table.Column<int>(type: "int", nullable: false), MaleChildren = table.Column<int>(type: "int", nullable: false), FemaleChildren = table.Column<int>(type: "int", nullable: false), MaleYouth = table.Column<int>(type: "int", nullable: false), FemaleYouth = table.Column<int>(type: "int", nullable: false), MaleElderly = table.Column<int>(type: "int", nullable: false), FemaleElderly = table.Column<int>(type: "int", nullable: false), MaleDisabled = table.Column<int>(type: "int", nullable: false), FemaleDisabled = table.Column<int>(type: "int", nullable: false), Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
        }, constraints: table => { table.PrimaryKey("PK_AnimalDiseaseImpactDetails", x => x.Id); table.ForeignKey("FK_AnimalDiseaseImpactDetails_AnimalDisease_AnimalDiseaseId", x => x.AnimalDiseaseId, "AnimalDisease", "Id", onDelete: ReferentialAction.Cascade); table.ForeignKey("FK_AnimalDiseaseImpactDetails_AnimalHealthStatuses_AnimalHealthStatusId", x => x.AnimalHealthStatusId, "AnimalHealthStatuses", "Id", onDelete: ReferentialAction.Cascade); });
        migrationBuilder.CreateIndex("IX_AnimalDiseaseImpactDetails_AnimalDiseaseId", "AnimalDiseaseImpactDetails", "AnimalDiseaseId");
        migrationBuilder.CreateIndex("IX_AnimalDiseaseImpactDetails_Report_Disease", "AnimalDiseaseImpactDetails", new[] { "AnimalHealthStatusId", "AnimalDiseaseId" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("AnimalDiseaseImpactDetails"); migrationBuilder.DropTable("CropPestImpactDetails");
        string[] columns = { "MaleHouseholdHeads", "FemaleHouseholdHeads", "MaleFamilyMembers", "FemaleFamilyMembers", "MaleChildren", "FemaleChildren", "MaleYouth", "FemaleYouth", "MaleElderly", "FemaleElderly", "MaleDisabled", "FemaleDisabled", "LactatingMothers", "PregnantWomen" };
        foreach (var column in columns) migrationBuilder.DropColumn(name: column, table: "EpidemicDiseaseSelections");
    }
}
