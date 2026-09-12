using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateWizardAndWeeklyWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            AddPerTypeImpactDetails.Apply(migrationBuilder);
            AddWeeklySubmissionWorkflow.Apply(migrationBuilder);
            AddUniqueWoredaReportingWeek.Apply(migrationBuilder);
            return;
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
                name: "AnimalDisease",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalDisease", x => x.Id);
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
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CropPestAndDesease",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CPDType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropPestAndDesease", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EpidemicDiseaseTypes",
                columns: table => new
                {
                    EpidemicDiseaseTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpidemicDiseaseTypes", x => x.EpidemicDiseaseTypeId);
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
                name: "PasswordResetKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PasswordResetValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KeyNote = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetKeys", x => x.Id);
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
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                });

            migrationBuilder.CreateTable(
                name: "AnimalDiseaseImpactDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnimalHealthStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnimalDiseaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumberOfAffectedAnimals = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AnimalDiseaseImpactDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnimalDiseaseImpactDetails_AnimalDisease_AnimalDiseaseId",
                        column: x => x.AnimalDiseaseId,
                        principalTable: "AnimalDisease",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnimalHealthStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Enough = table.Column<bool>(type: "bit", nullable: false),
                    MaleHouseHold = table.Column<int>(type: "int", nullable: false),
                    NoOfKebeliesWithWaterSupply = table.Column<int>(type: "int", nullable: false),
                    FemaleHouseHold = table.Column<int>(type: "int", nullable: false),
                    MaleFamily = table.Column<int>(type: "int", nullable: false),
                    FemaleFamily = table.Column<int>(type: "int", nullable: false),
                    ChildhMale = table.Column<int>(type: "int", nullable: false),
                    ChildFemale = table.Column<int>(type: "int", nullable: false),
                    YouthMale = table.Column<int>(type: "int", nullable: false),
                    YouthFemale = table.Column<int>(type: "int", nullable: false),
                    ElderlyMale = table.Column<int>(type: "int", nullable: false),
                    ElderlyFemale = table.Column<int>(type: "int", nullable: false),
                    DisabledMale = table.Column<int>(type: "int", nullable: false),
                    DisabledFemale = table.Column<int>(type: "int", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZoneApprovedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegionApprovedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneRejectionRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneRejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZoneRejectedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionRejectionRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionRejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegionRejectedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AnimalDiseaseJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LasModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalHealthStatuses", x => x.Id);
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
                name: "AnimalPricePerUnits",
                columns: table => new
                {
                    AnimalPricePerUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnimalTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_AnimalPricePerUnits", x => x.AnimalPricePerUnitId);
                    table.ForeignKey(
                        name: "FK_AnimalPricePerUnits_AnimalTypes_AnimalTypeId",
                        column: x => x.AnimalTypeId,
                        principalTable: "AnimalTypes",
                        principalColumn: "AnimalTypeId",
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
                });

            migrationBuilder.CreateTable(
                name: "AnimalWaterSupplyStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Enough = table.Column<bool>(type: "bit", nullable: false),
                    NoOfKebeliesWithPastureShortage = table.Column<int>(type: "int", nullable: false),
                    NoOfKebeliesWithWaterSupply = table.Column<int>(type: "int", nullable: false),
                    MaleHouseHold = table.Column<int>(type: "int", nullable: false),
                    FemaleHouseHold = table.Column<int>(type: "int", nullable: false),
                    MaleFamily = table.Column<int>(type: "int", nullable: false),
                    FemaleFamily = table.Column<int>(type: "int", nullable: false),
                    ChildhMale = table.Column<int>(type: "int", nullable: false),
                    ChildFemale = table.Column<int>(type: "int", nullable: false),
                    YouthMale = table.Column<int>(type: "int", nullable: false),
                    YouthFemale = table.Column<int>(type: "int", nullable: false),
                    ElderlyMale = table.Column<int>(type: "int", nullable: false),
                    ElderlyFemale = table.Column<int>(type: "int", nullable: false),
                    DisabledMale = table.Column<int>(type: "int", nullable: false),
                    DisabledFemale = table.Column<int>(type: "int", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZoneApprovedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegionApprovedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneRejectionRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneRejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZoneRejectedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionRejectionRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionRejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegionRejectedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LasModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalWaterSupplyStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserLocationLevl = table.Column<int>(type: "int", nullable: false),
                    UsernameChangeLimit = table.Column<int>(type: "int", nullable: false),
                    ProfilePicture = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Signature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                });

            migrationBuilder.CreateTable(
                name: "CropGrowths",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AtSeedStage = table.Column<float>(type: "real", nullable: false),
                    AtGerminationStage = table.Column<float>(type: "real", nullable: false),
                    AtGrowthStage = table.Column<float>(type: "real", nullable: false),
                    AtFruitStage = table.Column<float>(type: "real", nullable: false),
                    AtHarvestingStage = table.Column<float>(type: "real", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZoneApprovedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegionApprovedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneRejectionRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneRejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZoneRejectedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionRejectionRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionRejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegionRejectedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LasModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropGrowths", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CropPestAndDeseaseReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HasPestAndDeseasOccured = table.Column<bool>(type: "bit", nullable: false),
                    MaleHouseHold = table.Column<int>(type: "int", nullable: false),
                    FemaleHouseHold = table.Column<int>(type: "int", nullable: false),
                    MaleFamily = table.Column<int>(type: "int", nullable: false),
                    FemaleFamily = table.Column<int>(type: "int", nullable: false),
                    ChildhMale = table.Column<int>(type: "int", nullable: false),
                    ChildFemale = table.Column<int>(type: "int", nullable: false),
                    YouthMale = table.Column<int>(type: "int", nullable: false),
                    YouthFemale = table.Column<int>(type: "int", nullable: false),
                    ElderlyMale = table.Column<int>(type: "int", nullable: false),
                    ElderlyFemale = table.Column<int>(type: "int", nullable: false),
                    DisabledMale = table.Column<int>(type: "int", nullable: false),
                    DisabledFemale = table.Column<int>(type: "int", nullable: false),
                    AffectedLandInHectar = table.Column<float>(type: "real", nullable: true),
                    TypeOfCropAffected = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZoneApprovedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegionApprovedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneRejectionRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneRejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZoneRejectedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionRejectionRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionRejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegionRejectedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CropDiseasesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LasModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropPestAndDeseaseReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CropPestImpactDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CropPestAndDeseaseReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CropPestAndDeseaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    AffectedLandInHectares = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AffectedCropType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropPestImpactDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CropPestImpactDetails_CropPestAndDeseaseReports_CropPestAndDeseaseReportId",
                        column: x => x.CropPestAndDeseaseReportId,
                        principalTable: "CropPestAndDeseaseReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CropPestImpactDetails_CropPestAndDesease_CropPestAndDeseaseId",
                        column: x => x.CropPestAndDeseaseId,
                        principalTable: "CropPestAndDesease",
                        principalColumn: "Id",
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
                });

            migrationBuilder.CreateTable(
                name: "EpidemicDiseaseSelections",
                columns: table => new
                {
                    EpidemicDiseaseSelectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EpidemicDiseaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EpidemicDiseaseTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    PregnantWomen = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpidemicDiseaseSelections", x => x.EpidemicDiseaseSelectionId);
                    table.ForeignKey(
                        name: "FK_EpidemicDiseaseSelections_EpidemicDiseaseTypes_EpidemicDiseaseTypeId",
                        column: x => x.EpidemicDiseaseTypeId,
                        principalTable: "EpidemicDiseaseTypes",
                        principalColumn: "EpidemicDiseaseTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EpidemicDiseaseSelections_EpidemicDiseases_EpidemicDiseaseId",
                        column: x => x.EpidemicDiseaseId,
                        principalTable: "EpidemicDiseases",
                        principalColumn: "EpidemicDiseaseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FarmingActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeherFarmPlan = table.Column<float>(type: "real", nullable: false),
                    MeherPloughed = table.Column<float>(type: "real", nullable: false),
                    MeherSown = table.Column<float>(type: "real", nullable: false),
                    MeherHarvestingHHarvesting = table.Column<float>(type: "real", nullable: false),
                    MeherSownWithResidualMoisture = table.Column<float>(type: "real", nullable: false),
                    AutumnFarmPlan = table.Column<float>(type: "real", nullable: false),
                    AutumnPloughed = table.Column<float>(type: "real", nullable: false),
                    AutumnSown = table.Column<float>(type: "real", nullable: false),
                    AutumnHarvestingHHarvesting = table.Column<float>(type: "real", nullable: false),
                    AutumnSownWithResidualMoisture = table.Column<float>(type: "real", nullable: false),
                    CropGrowth = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZoneApprovedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegionApprovedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneRejectionRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneRejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZoneRejectedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionRejectionRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionRejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegionRejectedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LasModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmingActivities", x => x.Id);
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
                    AdditionalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AffectedKebeleIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ZonalApproval = table.Column<int>(type: "int", nullable: false),
                    ZoneReportedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneAdditionalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RegionApproval = table.Column<int>(type: "int", nullable: false),
                    RegionalReportedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionAdditionalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HumanDrinkWaterIssues", x => x.HumanDrinkWaterIssueId);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocationAmharicName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocationCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    CardHeaderTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HumanDrinkWaterIssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_HumanDrinkWaterIssues_HumanDrinkWaterIssueId",
                        column: x => x.HumanDrinkWaterIssueId,
                        principalTable: "HumanDrinkWaterIssues",
                        principalColumn: "HumanDrinkWaterIssueId");
                    table.ForeignKey(
                        name: "FK_Locations_Locations_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HumanDrinkWaterKebeles",
                columns: table => new
                {
                    HumanDrinkWaterKebeleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HumanDrinkWaterIssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KebeleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    ProblemName = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
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
                name: "PastureStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Enough = table.Column<bool>(type: "bit", nullable: false),
                    NumberOfAnimalsAffected = table.Column<int>(type: "int", nullable: false),
                    MaleHouseHold = table.Column<int>(type: "int", nullable: false),
                    FemaleHouseHold = table.Column<int>(type: "int", nullable: false),
                    MaleFamily = table.Column<int>(type: "int", nullable: false),
                    FemaleFamily = table.Column<int>(type: "int", nullable: false),
                    ChildhMale = table.Column<int>(type: "int", nullable: false),
                    ChildFemale = table.Column<int>(type: "int", nullable: false),
                    YouthMale = table.Column<int>(type: "int", nullable: false),
                    YouthFemale = table.Column<int>(type: "int", nullable: false),
                    ElderlyMale = table.Column<int>(type: "int", nullable: false),
                    ElderlyFemale = table.Column<int>(type: "int", nullable: false),
                    DisabledMale = table.Column<int>(type: "int", nullable: false),
                    DisabledFemale = table.Column<int>(type: "int", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZoneApprovedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegionApprovedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneRejectionRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneRejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZoneRejectedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionRejectionRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionRejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegionRejectedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LasModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastureStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastureStatuses_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RainfallReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullCoverageKebeles = table.Column<int>(type: "int", nullable: false),
                    FullCoveredKebelieList = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartialCoverageKebeles = table.Column<int>(type: "int", nullable: false),
                    NoRainKebeles = table.Column<int>(type: "int", nullable: false),
                    NoRainKebelieList = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoDataKebeles = table.Column<int>(type: "int", nullable: false),
                    LowAmountKebeles = table.Column<int>(type: "int", nullable: false),
                    LowRainKebeliesList = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LowMediumAmountKebeles = table.Column<int>(type: "int", nullable: false),
                    MediumAmountKebeles = table.Column<int>(type: "int", nullable: false),
                    MediumHighAmountKebeles = table.Column<int>(type: "int", nullable: false),
                    HighAmountKebeles = table.Column<int>(type: "int", nullable: false),
                    HighRainKebelieList = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZoneApprovedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegionApprovedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneRejectionRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZoneRejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZoneRejectedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionRejectionRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionRejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegionRejectedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AffectedKebelesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DroughtAffectedKebelesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LasModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RainfallReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RainfallReports_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "WeeklyProvideds",
                columns: table => new
                {
                    WeeklyProvidedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                name: "WeeklyReportSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WoredaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportingWeekStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActionAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyReportSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklyReportSubmissions_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeeklyReportSubmissions_Locations_WoredaId",
                        column: x => x.WoredaId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MigrationDetails",
                columns: table => new
                {
                    MigrationDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MigrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                        name: "FK_MigrationDetails_Migrations_MigrationId",
                        column: x => x.MigrationId,
                        principalTable: "Migrations",
                        principalColumn: "MigrationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyProvidedDetails",
                columns: table => new
                {
                    WeeklyProvidedDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeeklyProvidedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplyTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Measurement = table.Column<int>(type: "int", nullable: false),
                    ProvidedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DistributedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PercentageOfDistributedFromProvided = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Donor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyProvidedDetails", x => x.WeeklyProvidedDetailId);
                    table.ForeignKey(
                        name: "FK_WeeklyProvidedDetails_SupplyTypes_SupplyTypeId",
                        column: x => x.SupplyTypeId,
                        principalTable: "SupplyTypes",
                        principalColumn: "SupplyTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeeklyProvidedDetails_WeeklyProvideds_WeeklyProvidedId",
                        column: x => x.WeeklyProvidedId,
                        principalTable: "WeeklyProvideds",
                        principalColumn: "WeeklyProvidedId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyReportWorkflowHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeeklyReportSubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActorLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorLocationLevel = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    PreviousStatus = table.Column<int>(type: "int", nullable: true),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyReportWorkflowHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklyReportWorkflowHistories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeeklyReportWorkflowHistories_WeeklyReportSubmissions_WeeklyReportSubmissionId",
                        column: x => x.WeeklyReportSubmissionId,
                        principalTable: "WeeklyReportSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Locations",
                columns: new[] { "Id", "CardHeaderTitle", "HumanDrinkWaterIssueId", "IsActive", "Level", "LocationAmharicName", "LocationCode", "LocationName", "ParentId", "PhoneNumber" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "National HQ", null, true, 0, "ኢትዮጵያ", "ETH", "Ethiopia", null, "+251-111-111111" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "City Administration", null, true, 1, "አዲስ አበባ", "ETH-ADD", "Addis Ababa", new Guid("11111111-1111-1111-1111-111111111111"), "+251-111-222222" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Kirkos District", null, true, 3, "ቂርቆስ", "ETH-ADD-KIR", "Kirkos", new Guid("22222222-2222-2222-2222-222222222222"), "+251-111-333333" }
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
                name: "IX_AnimalDiseaseImpactDetails_AnimalDiseaseId",
                table: "AnimalDiseaseImpactDetails",
                column: "AnimalDiseaseId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalDiseaseImpactDetails_AnimalHealthStatusId_AnimalDiseaseId",
                table: "AnimalDiseaseImpactDetails",
                columns: new[] { "AnimalHealthStatusId", "AnimalDiseaseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnimalHealthStatuses_WoredaId",
                table: "AnimalHealthStatuses",
                column: "WoredaId");

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
                name: "IX_AnimalWaterSupplyStatuses_WoredaId",
                table: "AnimalWaterSupplyStatuses",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_LocationId",
                table: "AspNetUsers",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AssistanceRecipients_WoredaId",
                table: "AssistanceRecipients",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_CropGrowths_WoredaId",
                table: "CropGrowths",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_CropPestAndDeseaseReports_WoredaId",
                table: "CropPestAndDeseaseReports",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_CropPestImpactDetails_CropPestAndDeseaseId",
                table: "CropPestImpactDetails",
                column: "CropPestAndDeseaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CropPestImpactDetails_CropPestAndDeseaseReportId_CropPestAndDeseaseId",
                table: "CropPestImpactDetails",
                columns: new[] { "CropPestAndDeseaseReportId", "CropPestAndDeseaseId" },
                unique: true);

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
                name: "IX_EpidemicDiseaseSelections_EpidemicDiseaseId",
                table: "EpidemicDiseaseSelections",
                column: "EpidemicDiseaseId");

            migrationBuilder.CreateIndex(
                name: "IX_EpidemicDiseaseSelections_EpidemicDiseaseTypeId",
                table: "EpidemicDiseaseSelections",
                column: "EpidemicDiseaseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmingActivities_WoredaId",
                table: "FarmingActivities",
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
                name: "IX_Locations_HumanDrinkWaterIssueId",
                table: "Locations",
                column: "HumanDrinkWaterIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_ParentId",
                table: "Locations",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationDetails_MigrationId",
                table: "MigrationDetails",
                column: "MigrationId");

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
                name: "IX_PastureStatuses_WoredaId",
                table: "PastureStatuses",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_RainfallReports_WoredaId",
                table: "RainfallReports",
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
                name: "IX_WeeklyProvidedDetails_SupplyTypeId",
                table: "WeeklyProvidedDetails",
                column: "SupplyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyProvidedDetails_WeeklyProvidedId",
                table: "WeeklyProvidedDetails",
                column: "WeeklyProvidedId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyProvideds_SupplyTypeId",
                table: "WeeklyProvideds",
                column: "SupplyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyProvideds_WoredaId",
                table: "WeeklyProvideds",
                column: "WoredaId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyReportSubmissions_CreatedById",
                table: "WeeklyReportSubmissions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyReportSubmissions_WoredaId_ReportingWeekStart",
                table: "WeeklyReportSubmissions",
                columns: new[] { "WoredaId", "ReportingWeekStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyReportSubmissions_WoredaId_StartDate_EndDate",
                table: "WeeklyReportSubmissions",
                columns: new[] { "WoredaId", "StartDate", "EndDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyReportWorkflowHistories_UserId",
                table: "WeeklyReportWorkflowHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyReportWorkflowHistories_WeeklyReportSubmissionId",
                table: "WeeklyReportWorkflowHistories",
                column: "WeeklyReportSubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccidentDetails_WeeklyAccidents_WeeklyAccidentsId",
                table: "AccidentDetails",
                column: "WeeklyAccidentsId",
                principalTable: "WeeklyAccidents",
                principalColumn: "WeeklyAccidentsId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalDiseaseImpactDetails_AnimalHealthStatuses_AnimalHealthStatusId",
                table: "AnimalDiseaseImpactDetails",
                column: "AnimalHealthStatusId",
                principalTable: "AnimalHealthStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalHealthStatuses_Locations_WoredaId",
                table: "AnimalHealthStatuses",
                column: "WoredaId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalPrice_Locations_WoredaId",
                table: "AnimalPrice",
                column: "WoredaId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalPricePerUnits_Locations_WoredaId",
                table: "AnimalPricePerUnits",
                column: "WoredaId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalSupplyDetails_AnimalSupplys_AnimalSupplyId",
                table: "AnimalSupplyDetails",
                column: "AnimalSupplyId",
                principalTable: "AnimalSupplys",
                principalColumn: "GrainSupplyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalSupplys_Locations_WoredaId",
                table: "AnimalSupplys",
                column: "WoredaId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalWaterSupplyStatuses_Locations_WoredaId",
                table: "AnimalWaterSupplyStatuses",
                column: "WoredaId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Locations_LocationId",
                table: "AspNetUsers",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssistanceRecipients_Locations_WoredaId",
                table: "AssistanceRecipients",
                column: "WoredaId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CropGrowths_Locations_WoredaId",
                table: "CropGrowths",
                column: "WoredaId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CropPestAndDeseaseReports_Locations_WoredaId",
                table: "CropPestAndDeseaseReports",
                column: "WoredaId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeathDetails_Deaths_DeathId",
                table: "DeathDetails",
                column: "DeathId",
                principalTable: "Deaths",
                principalColumn: "DeathId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deaths_Locations_WoredaId",
                table: "Deaths",
                column: "WoredaId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EpidemicDiseaseKebeles_EpidemicDiseases_EpidemicDiseaseReportId",
                table: "EpidemicDiseaseKebeles",
                column: "EpidemicDiseaseReportId",
                principalTable: "EpidemicDiseases",
                principalColumn: "EpidemicDiseaseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EpidemicDiseaseKebeles_Locations_KebeleId",
                table: "EpidemicDiseaseKebeles",
                column: "KebeleId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EpidemicDiseases_Locations_WoredaId",
                table: "EpidemicDiseases",
                column: "WoredaId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FarmingActivities_Locations_WoredaId",
                table: "FarmingActivities",
                column: "WoredaId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GrainPriceDetails_GrainPrices_GrainPriceId",
                table: "GrainPriceDetails",
                column: "GrainPriceId",
                principalTable: "GrainPrices",
                principalColumn: "GrainPriceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GrainPricePerQuintals_Locations_WoredaId",
                table: "GrainPricePerQuintals",
                column: "WoredaId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GrainPrices_Locations_WoredaId",
                table: "GrainPrices",
                column: "WoredaId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GrainSupplyDetails_GrainSupplys_GrainSupplyId",
                table: "GrainSupplyDetails",
                column: "GrainSupplyId",
                principalTable: "GrainSupplys",
                principalColumn: "GrainSupplyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GrainSupplys_Locations_WoredaId",
                table: "GrainSupplys",
                column: "WoredaId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HumanDrinkWaterIssues_Locations_WoredaId",
                table: "HumanDrinkWaterIssues",
                column: "WoredaId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            return;
            migrationBuilder.DropForeignKey(
                name: "FK_HumanDrinkWaterIssues_Locations_WoredaId",
                table: "HumanDrinkWaterIssues");

            migrationBuilder.DropTable(
                name: "AccidentDetails");

            migrationBuilder.DropTable(
                name: "AnimalDiseaseImpactDetails");

            migrationBuilder.DropTable(
                name: "AnimalPriceDetails");

            migrationBuilder.DropTable(
                name: "AnimalPricePerUnits");

            migrationBuilder.DropTable(
                name: "AnimalSupplyDetails");

            migrationBuilder.DropTable(
                name: "AnimalWaterSupplyStatuses");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AssistanceRecipients");

            migrationBuilder.DropTable(
                name: "CropGrowths");

            migrationBuilder.DropTable(
                name: "CropPestImpactDetails");

            migrationBuilder.DropTable(
                name: "DeathDetails");

            migrationBuilder.DropTable(
                name: "EpidemicDiseaseKebeles");

            migrationBuilder.DropTable(
                name: "EpidemicDiseaseSelections");

            migrationBuilder.DropTable(
                name: "FarmingActivities");

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
                name: "PasswordResetKeys");

            migrationBuilder.DropTable(
                name: "PastureStatuses");

            migrationBuilder.DropTable(
                name: "RainfallReports");

            migrationBuilder.DropTable(
                name: "WeeklyProvidedDetails");

            migrationBuilder.DropTable(
                name: "WeeklyReportWorkflowHistories");

            migrationBuilder.DropTable(
                name: "WeeklyAccidents");

            migrationBuilder.DropTable(
                name: "AnimalDisease");

            migrationBuilder.DropTable(
                name: "AnimalHealthStatuses");

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
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "CropPestAndDeseaseReports");

            migrationBuilder.DropTable(
                name: "CropPestAndDesease");

            migrationBuilder.DropTable(
                name: "Deaths");

            migrationBuilder.DropTable(
                name: "EpidemicDiseaseTypes");

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
                name: "Migrations");

            migrationBuilder.DropTable(
                name: "WeeklyProvideds");

            migrationBuilder.DropTable(
                name: "WeeklyReportSubmissions");

            migrationBuilder.DropTable(
                name: "AccidentTypes");

            migrationBuilder.DropTable(
                name: "SupplyTypes");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "HumanDrinkWaterIssues");
        }
    }
}
