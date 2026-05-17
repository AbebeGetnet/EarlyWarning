using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class FirstMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccidentTypes",
                columns: table => new
                {
                    AccidentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccidentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccidentTypes", x => x.AccidentTypeId);
                });

            migrationBuilder.CreateTable(
                name: "AnimalIncreaseDecreases",
                columns: table => new
                {
                    AnimalIncreaseDecreaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReasonName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReasonType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalIncreaseDecreases", x => x.AnimalIncreaseDecreaseId);
                });

            migrationBuilder.CreateTable(
                name: "AnimalPrice",
                columns: table => new
                {
                    AnimalPriceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrainSupplyStatus = table.Column<int>(type: "int", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalPrice", x => x.AnimalPriceId);
                    table.ForeignKey(
                        name: "FK_AnimalPrice_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnimalPriceIncreaseDecreases",
                columns: table => new
                {
                    AnimalPriceIncreaseDecreaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReasonName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReasonType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalPriceIncreaseDecreases", x => x.AnimalPriceIncreaseDecreaseId);
                });

            migrationBuilder.CreateTable(
                name: "AnimalSupplys",
                columns: table => new
                {
                    GrainSupplyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrainSupplyStatus = table.Column<int>(type: "int", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalSupplys", x => x.GrainSupplyId);
                    table.ForeignKey(
                        name: "FK_AnimalSupplys_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnimalTypes",
                columns: table => new
                {
                    AnimalTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnimalTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalTypes", x => x.AnimalTypeId);
                });

            migrationBuilder.CreateTable(
                name: "AssistanceRecipients",
                columns: table => new
                {
                    AssistanceRecipientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaleHouseholdHeads = table.Column<int>(type: "int", nullable: false),
                    FemaleHouseholdHeads = table.Column<int>(type: "int", nullable: false),
                    MaleFamilyMembers = table.Column<int>(type: "int", nullable: false),
                    FemaleFamilyMembers = table.Column<int>(type: "int", nullable: false),
                    MaleChildren = table.Column<int>(type: "int", nullable: false),
                    FemaleChildren = table.Column<int>(type: "int", nullable: false),
                    MaleYouth = table.Column<int>(type: "int", nullable: false),
                    FemaleYouth = table.Column<int>(type: "int", nullable: false),
                    MaleElderly = table.Column<int>(type: "int", nullable: false),
                    FemaleElderly = table.Column<int>(type: "int", nullable: false),
                    MaleDisabled = table.Column<int>(type: "int", nullable: false),
                    FemaleDisabled = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistanceRecipients", x => x.AssistanceRecipientId);
                    table.ForeignKey(
                        name: "FK_AssistanceRecipients_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deaths",
                columns: table => new
                {
                    DeathId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HasDeath = table.Column<bool>(type: "bit", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deaths", x => x.DeathId);
                    table.ForeignKey(
                        name: "FK_Deaths_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EpidemicDiseases",
                columns: table => new
                {
                    EpidemicDiseaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HasEpidemicDisease = table.Column<bool>(type: "bit", nullable: false),
                    NumberOfAffectedKebeles = table.Column<int>(type: "int", nullable: false),
                    MaleHouseholdHeads = table.Column<int>(type: "int", nullable: false),
                    FemaleHouseholdHeads = table.Column<int>(type: "int", nullable: false),
                    MaleFamilyMembers = table.Column<int>(type: "int", nullable: false),
                    FemaleFamilyMembers = table.Column<int>(type: "int", nullable: false),
                    MaleChildren = table.Column<int>(type: "int", nullable: false),
                    FemaleChildren = table.Column<int>(type: "int", nullable: false),
                    MaleYouth = table.Column<int>(type: "int", nullable: false),
                    FemaleYouth = table.Column<int>(type: "int", nullable: false),
                    MaleElderly = table.Column<int>(type: "int", nullable: false),
                    FemaleElderly = table.Column<int>(type: "int", nullable: false),
                    MaleDisabled = table.Column<int>(type: "int", nullable: false),
                    FemaleDisabled = table.Column<int>(type: "int", nullable: false),
                    LactatingMothers = table.Column<int>(type: "int", nullable: false),
                    PregnantWomen = table.Column<int>(type: "int", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpidemicDiseases", x => x.EpidemicDiseaseId);
                    table.ForeignKey(
                        name: "FK_EpidemicDiseases_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EpidemicDiseaseTypes",
                columns: table => new
                {
                    EpidemicDiseaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpidemicDiseaseTypes", x => x.EpidemicDiseaseId);
                });

            migrationBuilder.CreateTable(
                name: "GrainIncreaseDecreases",
                columns: table => new
                {
                    GrainIncreaseDecreaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReasonName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReasonType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrainIncreaseDecreases", x => x.GrainIncreaseDecreaseId);
                });

            migrationBuilder.CreateTable(
                name: "GrainPriceIncreaseDecreases",
                columns: table => new
                {
                    GrainIncreaseDecreaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReasonName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReasonType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrainPriceIncreaseDecreases", x => x.GrainIncreaseDecreaseId);
                });

            migrationBuilder.CreateTable(
                name: "GrainPrices",
                columns: table => new
                {
                    GrainPriceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrainSupplyStatus = table.Column<int>(type: "int", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrainPrices", x => x.GrainPriceId);
                    table.ForeignKey(
                        name: "FK_GrainPrices_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrainSupplys",
                columns: table => new
                {
                    GrainSupplyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrainSupplyStatus = table.Column<int>(type: "int", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrainSupplys", x => x.GrainSupplyId);
                    table.ForeignKey(
                        name: "FK_GrainSupplys_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrainTypes",
                columns: table => new
                {
                    GrainTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrainName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrainTypes", x => x.GrainTypeId);
                });

            migrationBuilder.CreateTable(
                name: "HumanDrinkWaterIssues",
                columns: table => new
                {
                    HumanDrinkWaterIssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HasWaterProblem = table.Column<bool>(type: "bit", nullable: false),
                    NumberOfAffectedKebeles = table.Column<int>(type: "int", nullable: false),
                    MaleHouseholdHeads = table.Column<int>(type: "int", nullable: false),
                    FemaleHouseholdHeads = table.Column<int>(type: "int", nullable: false),
                    MaleFamilyMembers = table.Column<int>(type: "int", nullable: false),
                    FemaleFamilyMembers = table.Column<int>(type: "int", nullable: false),
                    MaleChildren = table.Column<int>(type: "int", nullable: false),
                    FemaleChildren = table.Column<int>(type: "int", nullable: false),
                    MaleYouth = table.Column<int>(type: "int", nullable: false),
                    FemaleYouth = table.Column<int>(type: "int", nullable: false),
                    MaleElderly = table.Column<int>(type: "int", nullable: false),
                    FemaleElderly = table.Column<int>(type: "int", nullable: false),
                    MaleDisabled = table.Column<int>(type: "int", nullable: false),
                    FemaleDisabled = table.Column<int>(type: "int", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HumanDrinkWaterIssues", x => x.HumanDrinkWaterIssueId);
                    table.ForeignKey(
                        name: "FK_HumanDrinkWaterIssues_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Migrations",
                columns: table => new
                {
                    MigrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HasMigration = table.Column<bool>(type: "bit", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GeneralNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Migrations", x => x.MigrationId);
                    table.ForeignKey(
                        name: "FK_Migrations_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OtherProblems",
                columns: table => new
                {
                    OtherProblemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HasOtherProblem = table.Column<bool>(type: "bit", nullable: false),
                    ProblemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaleHouseholdHeads = table.Column<int>(type: "int", nullable: false),
                    FemaleHouseholdHeads = table.Column<int>(type: "int", nullable: false),
                    MaleFamilyMembers = table.Column<int>(type: "int", nullable: false),
                    FemaleFamilyMembers = table.Column<int>(type: "int", nullable: false),
                    MaleChildren = table.Column<int>(type: "int", nullable: false),
                    FemaleChildren = table.Column<int>(type: "int", nullable: false),
                    MaleYouth = table.Column<int>(type: "int", nullable: false),
                    FemaleYouth = table.Column<int>(type: "int", nullable: false),
                    MaleElderly = table.Column<int>(type: "int", nullable: false),
                    FemaleElderly = table.Column<int>(type: "int", nullable: false),
                    MaleDisabled = table.Column<int>(type: "int", nullable: false),
                    FemaleDisabled = table.Column<int>(type: "int", nullable: false),
                    GeneralNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtherProblems", x => x.OtherProblemId);
                    table.ForeignKey(
                        name: "FK_OtherProblems_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplyTypes",
                columns: table => new
                {
                    SupplyTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplyTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyTypes", x => x.SupplyTypeId);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyAccidents",
                columns: table => new
                {
                    WeeklyAccidentsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HasAccident = table.Column<bool>(type: "bit", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    AccidentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyAccidents", x => x.WeeklyAccidentsId);
                    table.ForeignKey(
                        name: "FK_WeeklyAccidents_AccidentTypes_AccidentTypeId",
                        column: x => x.AccidentTypeId,
                        principalTable: "AccidentTypes",
                        principalColumn: "AccidentTypeId");
                    table.ForeignKey(
                        name: "FK_WeeklyAccidents_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnimalPriceDetails",
                columns: table => new
                {
                    AnimalPriceDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnimalPriceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnimalPriceIncreaseDecreaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalPriceDetails", x => x.AnimalPriceDetailId);
                    table.ForeignKey(
                        name: "FK_AnimalPriceDetails_AnimalPriceIncreaseDecreases_AnimalPriceIncreaseDecreaseId",
                        column: x => x.AnimalPriceIncreaseDecreaseId,
                        principalTable: "AnimalPriceIncreaseDecreases",
                        principalColumn: "AnimalPriceIncreaseDecreaseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnimalPriceDetails_AnimalPrice_AnimalPriceId",
                        column: x => x.AnimalPriceId,
                        principalTable: "AnimalPrice",
                        principalColumn: "AnimalPriceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnimalSupplyDetails",
                columns: table => new
                {
                    AnimalSupplyDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnimalSupplyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnimalIncreaseDecreaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalSupplyDetails", x => x.AnimalSupplyDetailId);
                    table.ForeignKey(
                        name: "FK_AnimalSupplyDetails_AnimalIncreaseDecreases_AnimalIncreaseDecreaseId",
                        column: x => x.AnimalIncreaseDecreaseId,
                        principalTable: "AnimalIncreaseDecreases",
                        principalColumn: "AnimalIncreaseDecreaseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnimalSupplyDetails_AnimalSupplys_AnimalSupplyId",
                        column: x => x.AnimalSupplyId,
                        principalTable: "AnimalSupplys",
                        principalColumn: "GrainSupplyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeathDetails",
                columns: table => new
                {
                    DeathDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeathId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeathReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MaleHouseholdHeads = table.Column<int>(type: "int", nullable: false),
                    FemaleHouseholdHeads = table.Column<int>(type: "int", nullable: false),
                    MaleFamilyMembers = table.Column<int>(type: "int", nullable: false),
                    FemaleFamilyMembers = table.Column<int>(type: "int", nullable: false),
                    MaleChildren = table.Column<int>(type: "int", nullable: false),
                    FemaleChildren = table.Column<int>(type: "int", nullable: false),
                    MaleYouth = table.Column<int>(type: "int", nullable: false),
                    FemaleYouth = table.Column<int>(type: "int", nullable: false),
                    MaleElderly = table.Column<int>(type: "int", nullable: false),
                    FemaleElderly = table.Column<int>(type: "int", nullable: false),
                    MaleDisabled = table.Column<int>(type: "int", nullable: false),
                    FemaleDisabled = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeathDetails", x => x.DeathDetailId);
                    table.ForeignKey(
                        name: "FK_DeathDetails_Deaths_DeathId",
                        column: x => x.DeathId,
                        principalTable: "Deaths",
                        principalColumn: "DeathId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EpidemicDiseaseKebeles",
                columns: table => new
                {
                    EpidemicDiseaseKebeleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EpidemicDiseaseReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KebeleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegisteredDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpidemicDiseaseKebeles", x => x.EpidemicDiseaseKebeleId);
                    table.ForeignKey(
                        name: "FK_EpidemicDiseaseKebeles_EpidemicDiseases_EpidemicDiseaseReportId",
                        column: x => x.EpidemicDiseaseReportId,
                        principalTable: "EpidemicDiseases",
                        principalColumn: "EpidemicDiseaseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EpidemicDiseaseKebeles_Locations_KebeleId",
                        column: x => x.KebeleId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "GrainPriceDetails",
                columns: table => new
                {
                    GrainPriceDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrainPriceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrainPriceIncreaseDecreaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrainPriceDetails", x => x.GrainPriceDetailId);
                    table.ForeignKey(
                        name: "FK_GrainPriceDetails_GrainPriceIncreaseDecreases_GrainPriceIncreaseDecreaseId",
                        column: x => x.GrainPriceIncreaseDecreaseId,
                        principalTable: "GrainPriceIncreaseDecreases",
                        principalColumn: "GrainIncreaseDecreaseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrainPriceDetails_GrainPrices_GrainPriceId",
                        column: x => x.GrainPriceId,
                        principalTable: "GrainPrices",
                        principalColumn: "GrainPriceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrainSupplyDetails",
                columns: table => new
                {
                    GrainSupplyDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrainSupplyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrainIncreaseDecreaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrainSupplyDetails", x => x.GrainSupplyDetailId);
                    table.ForeignKey(
                        name: "FK_GrainSupplyDetails_GrainIncreaseDecreases_GrainIncreaseDecreaseId",
                        column: x => x.GrainIncreaseDecreaseId,
                        principalTable: "GrainIncreaseDecreases",
                        principalColumn: "GrainIncreaseDecreaseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrainSupplyDetails_GrainSupplys_GrainSupplyId",
                        column: x => x.GrainSupplyId,
                        principalTable: "GrainSupplys",
                        principalColumn: "GrainSupplyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnimalPricePerUnits",
                columns: table => new
                {
                    GrainPricePerQuintalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrainTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeeklyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WeeklyMarketStatus = table.Column<int>(type: "int", nullable: false),
                    PriceDifference = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    AnimalTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalPricePerUnits", x => x.GrainPricePerQuintalId);
                    table.ForeignKey(
                        name: "FK_AnimalPricePerUnits_AnimalTypes_AnimalTypeId",
                        column: x => x.AnimalTypeId,
                        principalTable: "AnimalTypes",
                        principalColumn: "AnimalTypeId");
                    table.ForeignKey(
                        name: "FK_AnimalPricePerUnits_GrainTypes_GrainTypeId",
                        column: x => x.GrainTypeId,
                        principalTable: "GrainTypes",
                        principalColumn: "GrainTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnimalPricePerUnits_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrainPricePerQuintals",
                columns: table => new
                {
                    GrainPricePerQuintalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrainTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeeklyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WeeklyMarketStatus = table.Column<int>(type: "int", nullable: false),
                    PriceDifference = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrainPricePerQuintals", x => x.GrainPricePerQuintalId);
                    table.ForeignKey(
                        name: "FK_GrainPricePerQuintals_GrainTypes_GrainTypeId",
                        column: x => x.GrainTypeId,
                        principalTable: "GrainTypes",
                        principalColumn: "GrainTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrainPricePerQuintals_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HumanDrinkWaterKebeles",
                columns: table => new
                {
                    HumanDrinkWaterKebeleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HumanDrinkWaterIssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KebeleListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KebeleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RegisteredDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HumanDrinkWaterKebeles", x => x.HumanDrinkWaterKebeleId);
                    table.ForeignKey(
                        name: "FK_HumanDrinkWaterKebeles_HumanDrinkWaterIssues_HumanDrinkWaterIssueId",
                        column: x => x.HumanDrinkWaterIssueId,
                        principalTable: "HumanDrinkWaterIssues",
                        principalColumn: "HumanDrinkWaterIssueId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HumanDrinkWaterKebeles_Locations_KebeleId",
                        column: x => x.KebeleId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MigrationDetails",
                columns: table => new
                {
                    MigrationDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MigrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MigrationReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OriginLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MigrationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MaleHouseholdHeads = table.Column<int>(type: "int", nullable: false),
                    FemaleHouseholdHeads = table.Column<int>(type: "int", nullable: false),
                    MaleFamilyMembers = table.Column<int>(type: "int", nullable: false),
                    FemaleFamilyMembers = table.Column<int>(type: "int", nullable: false),
                    MaleChildren = table.Column<int>(type: "int", nullable: false),
                    FemaleChildren = table.Column<int>(type: "int", nullable: false),
                    MaleYouth = table.Column<int>(type: "int", nullable: false),
                    FemaleYouth = table.Column<int>(type: "int", nullable: false),
                    MaleElderly = table.Column<int>(type: "int", nullable: false),
                    FemaleElderly = table.Column<int>(type: "int", nullable: false),
                    MaleDisabled = table.Column<int>(type: "int", nullable: false),
                    FemaleDisabled = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MigrationDetails", x => x.MigrationDetailId);
                    table.ForeignKey(
                        name: "FK_MigrationDetails_Locations_OriginLocationId",
                        column: x => x.OriginLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MigrationDetails_Migrations_MigrationReportId",
                        column: x => x.MigrationReportId,
                        principalTable: "Migrations",
                        principalColumn: "MigrationId");
                });

            migrationBuilder.CreateTable(
                name: "WeeklyProvideds",
                columns: table => new
                {
                    WeeklyProvidedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Measurement = table.Column<int>(type: "int", nullable: false),
                    ProvidedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DistributedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PercentageOfDistributedFromProvided = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Donor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IfProblem = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportStatus = table.Column<int>(type: "int", nullable: false),
                    SupplyTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyProvideds", x => x.WeeklyProvidedId);
                    table.ForeignKey(
                        name: "FK_WeeklyProvideds_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeeklyProvideds_SupplyTypes_SupplyTypeId",
                        column: x => x.SupplyTypeId,
                        principalTable: "SupplyTypes",
                        principalColumn: "SupplyTypeId");
                });

            migrationBuilder.CreateTable(
                name: "AccidentDetails",
                columns: table => new
                {
                    AccidentDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeeklyAccidentsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccidentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DamagedLandInHectares = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DamageRateInPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AffectedHouseholdsMale = table.Column<int>(type: "int", nullable: false),
                    AffectedHouseholdsFemale = table.Column<int>(type: "int", nullable: false),
                    AffectedChildrenMale = table.Column<int>(type: "int", nullable: false),
                    AffectedChildrenFemale = table.Column<int>(type: "int", nullable: false),
                    AffectedYouthMale = table.Column<int>(type: "int", nullable: false),
                    AffectedYouthFemale = table.Column<int>(type: "int", nullable: false),
                    AffectedElderlyMale = table.Column<int>(type: "int", nullable: false),
                    AffectedElderlyFemale = table.Column<int>(type: "int", nullable: false),
                    AffectedDisabledMale = table.Column<int>(type: "int", nullable: false),
                    AffectedDisabledFemale = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccidentDetails", x => x.AccidentDetailId);
                    table.ForeignKey(
                        name: "FK_AccidentDetails_AccidentTypes_AccidentTypeId",
                        column: x => x.AccidentTypeId,
                        principalTable: "AccidentTypes",
                        principalColumn: "AccidentTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccidentDetails_WeeklyAccidents_WeeklyAccidentsId",
                        column: x => x.WeeklyAccidentsId,
                        principalTable: "WeeklyAccidents",
                        principalColumn: "WeeklyAccidentsId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccidentDetails_AccidentTypeId",
                table: "AccidentDetails",
                column: "AccidentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AccidentDetails_WeeklyAccidentsId",
                table: "AccidentDetails",
                column: "WeeklyAccidentsId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalPrice_WoredaId",
                table: "AnimalPrice",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalPriceDetails_AnimalPriceId",
                table: "AnimalPriceDetails",
                column: "AnimalPriceId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalPriceDetails_AnimalPriceIncreaseDecreaseId",
                table: "AnimalPriceDetails",
                column: "AnimalPriceIncreaseDecreaseId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalPricePerUnits_AnimalTypeId",
                table: "AnimalPricePerUnits",
                column: "AnimalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalPricePerUnits_GrainTypeId",
                table: "AnimalPricePerUnits",
                column: "GrainTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalPricePerUnits_WoredaId",
                table: "AnimalPricePerUnits",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalSupplyDetails_AnimalIncreaseDecreaseId",
                table: "AnimalSupplyDetails",
                column: "AnimalIncreaseDecreaseId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalSupplyDetails_AnimalSupplyId",
                table: "AnimalSupplyDetails",
                column: "AnimalSupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalSupplys_WoredaId",
                table: "AnimalSupplys",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_AssistanceRecipients_WoredaId",
                table: "AssistanceRecipients",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_DeathDetails_DeathId",
                table: "DeathDetails",
                column: "DeathId");

            migrationBuilder.CreateIndex(
                name: "IX_Deaths_WoredaId",
                table: "Deaths",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_EpidemicDiseaseKebeles_EpidemicDiseaseReportId",
                table: "EpidemicDiseaseKebeles",
                column: "EpidemicDiseaseReportId");

            migrationBuilder.CreateIndex(
                name: "IX_EpidemicDiseaseKebeles_KebeleId",
                table: "EpidemicDiseaseKebeles",
                column: "KebeleId");

            migrationBuilder.CreateIndex(
                name: "IX_EpidemicDiseases_WoredaId",
                table: "EpidemicDiseases",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_GrainPriceDetails_GrainPriceId",
                table: "GrainPriceDetails",
                column: "GrainPriceId");

            migrationBuilder.CreateIndex(
                name: "IX_GrainPriceDetails_GrainPriceIncreaseDecreaseId",
                table: "GrainPriceDetails",
                column: "GrainPriceIncreaseDecreaseId");

            migrationBuilder.CreateIndex(
                name: "IX_GrainPricePerQuintals_GrainTypeId",
                table: "GrainPricePerQuintals",
                column: "GrainTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_GrainPricePerQuintals_WoredaId",
                table: "GrainPricePerQuintals",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_GrainPrices_WoredaId",
                table: "GrainPrices",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_GrainSupplyDetails_GrainIncreaseDecreaseId",
                table: "GrainSupplyDetails",
                column: "GrainIncreaseDecreaseId");

            migrationBuilder.CreateIndex(
                name: "IX_GrainSupplyDetails_GrainSupplyId",
                table: "GrainSupplyDetails",
                column: "GrainSupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_GrainSupplys_WoredaId",
                table: "GrainSupplys",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_HumanDrinkWaterIssues_WoredaId",
                table: "HumanDrinkWaterIssues",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_HumanDrinkWaterKebeles_HumanDrinkWaterIssueId",
                table: "HumanDrinkWaterKebeles",
                column: "HumanDrinkWaterIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_HumanDrinkWaterKebeles_KebeleId",
                table: "HumanDrinkWaterKebeles",
                column: "KebeleId");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationDetails_MigrationReportId",
                table: "MigrationDetails",
                column: "MigrationReportId");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationDetails_OriginLocationId",
                table: "MigrationDetails",
                column: "OriginLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Migrations_WoredaId",
                table: "Migrations",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_OtherProblems_WoredaId",
                table: "OtherProblems",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyAccidents_AccidentTypeId",
                table: "WeeklyAccidents",
                column: "AccidentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyAccidents_WoredaId",
                table: "WeeklyAccidents",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyProvideds_SupplyTypeId",
                table: "WeeklyProvideds",
                column: "SupplyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyProvideds_WoredaId",
                table: "WeeklyProvideds",
                column: "WoredaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccidentDetails");

            migrationBuilder.DropTable(
                name: "AnimalPriceDetails");

            migrationBuilder.DropTable(
                name: "AnimalPricePerUnits");

            migrationBuilder.DropTable(
                name: "AnimalSupplyDetails");

            migrationBuilder.DropTable(
                name: "AssistanceRecipients");

            migrationBuilder.DropTable(
                name: "DeathDetails");

            migrationBuilder.DropTable(
                name: "EpidemicDiseaseKebeles");

            migrationBuilder.DropTable(
                name: "EpidemicDiseaseTypes");

            migrationBuilder.DropTable(
                name: "GrainPriceDetails");

            migrationBuilder.DropTable(
                name: "GrainPricePerQuintals");

            migrationBuilder.DropTable(
                name: "GrainSupplyDetails");

            migrationBuilder.DropTable(
                name: "HumanDrinkWaterKebeles");

            migrationBuilder.DropTable(
                name: "MigrationDetails");

            migrationBuilder.DropTable(
                name: "OtherProblems");

            migrationBuilder.DropTable(
                name: "WeeklyProvideds");

            migrationBuilder.DropTable(
                name: "WeeklyAccidents");

            migrationBuilder.DropTable(
                name: "AnimalPriceIncreaseDecreases");

            migrationBuilder.DropTable(
                name: "AnimalPrice");

            migrationBuilder.DropTable(
                name: "AnimalTypes");

            migrationBuilder.DropTable(
                name: "AnimalIncreaseDecreases");

            migrationBuilder.DropTable(
                name: "AnimalSupplys");

            migrationBuilder.DropTable(
                name: "Deaths");

            migrationBuilder.DropTable(
                name: "EpidemicDiseases");

            migrationBuilder.DropTable(
                name: "GrainPriceIncreaseDecreases");

            migrationBuilder.DropTable(
                name: "GrainPrices");

            migrationBuilder.DropTable(
                name: "GrainTypes");

            migrationBuilder.DropTable(
                name: "GrainIncreaseDecreases");

            migrationBuilder.DropTable(
                name: "GrainSupplys");

            migrationBuilder.DropTable(
                name: "HumanDrinkWaterIssues");

            migrationBuilder.DropTable(
                name: "Migrations");

            migrationBuilder.DropTable(
                name: "SupplyTypes");

            migrationBuilder.DropTable(
                name: "AccidentTypes");
        }
    }
}
