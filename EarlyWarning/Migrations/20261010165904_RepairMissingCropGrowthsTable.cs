using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EarlyWarning.Migrations
{
    /// <inheritdoc />
    public partial class RepairMissingCropGrowthsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Some databases record ConsolidateWizardAndWeeklyWorkflow as applied but lack the
            // CropGrowths table it creates. Recreate it (matching the current model) only if missing.
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[CropGrowths]', N'U') IS NULL
BEGIN
    CREATE TABLE [CropGrowths] (
        [Id] uniqueidentifier NOT NULL,
        [AtSeedStage] real NOT NULL,
        [AtGerminationStage] real NOT NULL,
        [AtGrowthStage] real NOT NULL,
        [AtFruitStage] real NOT NULL,
        [AtHarvestingStage] real NOT NULL,
        [WoredaId] uniqueidentifier NOT NULL,
        [Status] int NOT NULL,
        [SubmittedAt] datetime2 NULL,
        [SubmittedById] nvarchar(max) NULL,
        [ZoneApprovedAt] datetime2 NULL,
        [ZoneApprovedById] nvarchar(max) NULL,
        [RegionApprovedAt] datetime2 NULL,
        [RegionApprovedById] nvarchar(max) NULL,
        [ZoneRejectionRemark] nvarchar(max) NULL,
        [ZoneRejectedAt] datetime2 NULL,
        [ZoneRejectedById] nvarchar(max) NULL,
        [RegionRejectionRemark] nvarchar(max) NULL,
        [RegionRejectedAt] datetime2 NULL,
        [RegionRejectedById] nvarchar(max) NULL,
        [Remarks] nvarchar(500) NULL,
        [UserId] nvarchar(max) NULL,
        [LasModifiedAt] datetime2 NOT NULL,
        [DeletedAt] datetime2 NOT NULL,
        [StartDate] datetime2 NULL,
        [EndDate] datetime2 NULL,
        CONSTRAINT [PK_CropGrowths] PRIMARY KEY ([Id])
    );
    CREATE INDEX [IX_CropGrowths_WoredaId] ON [CropGrowths] ([WoredaId]);
    ALTER TABLE [CropGrowths] ADD CONSTRAINT [FK_CropGrowths_Locations_WoredaId]
        FOREIGN KEY ([WoredaId]) REFERENCES [Locations] ([Id]) ON DELETE CASCADE;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
