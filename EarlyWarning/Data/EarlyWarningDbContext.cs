using EarlyWarning.Enums;
using EarlyWarning.Models;
using EarlyWarning.Models.Aid;
using EarlyWarning.Models.AnimalPrice;
using EarlyWarning.Models.AnimalSupply;
using EarlyWarning.Models.EpidemicDisease;
using EarlyWarning.Models.HumanDrinkWaterIssue;
using EarlyWarning.Models.Migration;
using EarlyWarning.Models.PriceofGrain;
using EarlyWarning.Models.SupplyofGrain;
using EarlyWarning.Models.WeeklyAccidents;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;

namespace EarlyWarning.Data
{
    public class EarlyWarningDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly EarlyWarningDbContext _context;
        public EarlyWarningDbContext(DbContextOptions<EarlyWarningDbContext> options) : base(options)
        {            
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }        
        public DbSet<Locations> Locations {  get; set; } 
        public DbSet<PasswordResetKey> PasswordResetKeys {  get; set; }

        public DbSet<AssistanceRecipient> AssistanceRecipients { get; set; }        
        public DbSet<OtherProblem> OtherProblems {  get; set; } 
        public DbSet<SupplyType> SupplyTypes {  get; set; }
        public DbSet<WeeklyProvided> WeeklyProvideds { get; set; }        
        public DbSet<AnimalPrice> AnimalPrice {  get; set; } 
        public DbSet<AnimalPriceDetail> AnimalPriceDetails {  get; set; }
        public DbSet<AnimalPriceIncreaseDecrease> AnimalPriceIncreaseDecreases { get; set; }        
        public DbSet<AnimalPricePerUnit> AnimalPricePerUnits {  get; set; } 
        public DbSet<AnimalType> AnimalTypes {  get; set; }
        public DbSet<AnimalSupply> AnimalSupplys { get; set; }        
        public DbSet<AnimalSupplyDetail> AnimalSupplyDetails {  get; set; } 
        public DbSet<AnimalIncreaseDecrease> AnimalIncreaseDecreases {  get; set; }
        public DbSet<EpidemicDisease> EpidemicDiseases { get; set; }        
        public DbSet<EpidemicDiseaseKebele> EpidemicDiseaseKebeles {  get; set; } 
        public DbSet<EpidemicDiseaseType> EpidemicDiseaseTypes {  get; set; }
        public DbSet<EpidemicDiseaseSelection> EpidemicDiseaseSelections {  get; set; }

        public DbSet<HumanDrinkWaterIssue> HumanDrinkWaterIssues { get; set; }        
        public DbSet<HumanDrinkWaterKebele> HumanDrinkWaterKebeles {  get; set; } 
        public DbSet<Death> Deaths {  get; set; }
        public DbSet<DeathDetail> DeathDetails {  get; set; }
        public DbSet<Migration> Migrations { get; set; }        
        public DbSet<MigrationDetail> MigrationDetails {  get; set; } 
        public DbSet<GrainPrice> GrainPrices {  get; set; }
        public DbSet<GrainPriceDetail> GrainPriceDetails {  get; set; }
        public DbSet<GrainPriceIncreaseDecrease> GrainPriceIncreaseDecreases {  get; set; }
        public DbSet<GrainPricePerQuintal> GrainPricePerQuintals {  get; set; }
        public DbSet<GrainType> GrainTypes {  get; set; }
        public DbSet<GrainIncreaseDecrease> GrainIncreaseDecreases {  get; set; }
        public DbSet<GrainSupply> GrainSupplys {  get; set; }
        public DbSet<GrainSupplyDetail> GrainSupplyDetails {  get; set; }
        public DbSet<AccidentDetail> AccidentDetails {  get; set; }
        public DbSet<AccidentType> AccidentTypes {  get; set; }
        public DbSet<WeeklyAccidents> WeeklyAccidents {  get; set; }
        public DbSet<WeeklyProvidedDetail> WeeklyProvidedDetails {  get; set; }
        

        public DbSet<RainfallReport> RainfallReports {  get; set; }
        public DbSet<FarmingActivity> FarmingActivities {  get; set; }
        public DbSet<CropPestAndDesease> CropPestAndDesease { get; set; } = default!;
        public DbSet<CropPestAndDeseaseReport> CropPestAndDeseaseReports { get; set; } = default!;
        public DbSet<CropGrowth> CropGrowths { get; set; } = default!;
        public DbSet<PastureStatus> PastureStatuses { get; set; } = default!;
        public DbSet<AnimalWaterSupplyStatus> AnimalWaterSupplyStatuses { get; set; } = default!;
        public DbSet<AnimalHealthStatus> AnimalHealthStatuses { get; set; } = default!;
        public DbSet<CropPestImpactDetail> CropPestImpactDetails { get; set; } = default!;
        public DbSet<AnimalDiseaseImpactDetail> AnimalDiseaseImpactDetails { get; set; } = default!;
        public DbSet<WeeklyReportSubmission> WeeklyReportSubmissions { get; set; } = default!;
        public DbSet<WeeklyReportWorkflowHistory> WeeklyReportWorkflowHistories { get; set; } = default!;
        public DbSet<AnimalMigrationRecord> AnimalMigrationRecords { get; set; } = default!;
        public DbSet<WeeklyAggregateSnapshot> WeeklyAggregateSnapshots { get; set; } = default!;
        public DbSet<SchoolReport> SchoolReports { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure self-referencing relationship
            modelBuilder.Entity<Locations>()
                .HasOne(l => l.Parent)
                .WithMany(l => l.Children)
                .HasForeignKey(l => l.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial data
            modelBuilder.Entity<Locations>().HasData(
                new Locations
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    LocationName = "Ethiopia",
                    LocationAmharicName = "ኢትዮጵያ",
                    LocationCode = "ETH",
                    PhoneNumber = "+251-111-111111",
                    Level = LocationLevel.ሀገር,
                    CardHeaderTitle = "National HQ",
                    IsActive = true,
                    ParentId = null
                },
                new Locations
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    LocationName = "Addis Ababa",
                    LocationAmharicName = "አዲስ አበባ",
                    LocationCode = "ETH-ADD",
                    PhoneNumber = "+251-111-222222",
                    Level = LocationLevel.ክልል,
                    CardHeaderTitle = "City Administration",
                    IsActive = true,
                    ParentId = Guid.Parse("11111111-1111-1111-1111-111111111111")
                },
                new Locations
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    LocationName = "Kirkos",
                    LocationAmharicName = "ቂርቆስ",
                    LocationCode = "ETH-ADD-KIR",
                    PhoneNumber = "+251-111-333333",
                    Level = LocationLevel.ወረዳ,
                    CardHeaderTitle = "Kirkos District",
                    IsActive = true,
                    ParentId = Guid.Parse("22222222-2222-2222-2222-222222222222")
                }
            );

            modelBuilder.Entity<EpidemicDiseaseSelection>()
        .HasOne(e => e.EpidemicDisease)
        .WithMany(e => e.EpidemicDiseaseSelections)
        .HasForeignKey(e => e.EpidemicDiseaseId);

            modelBuilder.Entity<EpidemicDiseaseSelection>()
                .HasOne(e => e.EpidemicDiseaseType)
                .WithMany(e => e.EpidemicDiseaseSelections)
                .HasForeignKey(e => e.EpidemicDiseaseTypeId);

            modelBuilder.Entity<CropPestImpactDetail>().HasIndex(x => new { x.CropPestAndDeseaseReportId, x.CropPestAndDeseaseId }).IsUnique();
            modelBuilder.Entity<AnimalDiseaseImpactDetail>().HasIndex(x => new { x.AnimalHealthStatusId, x.AnimalDiseaseId }).IsUnique();
            modelBuilder.Entity<WeeklyReportSubmission>().HasIndex(x => new { x.WoredaId, x.StartDate, x.EndDate }).IsUnique();
            modelBuilder.Entity<WeeklyReportSubmission>().HasIndex(x => new { x.WoredaId, x.ReportingWeekStart }).IsUnique();
            modelBuilder.Entity<WeeklyReportSubmission>().Property(x => x.RowVersion).IsRowVersion();
            modelBuilder.Entity<WeeklyReportSubmission>().HasOne(x => x.Woreda).WithMany()
                .HasForeignKey(x => x.WoredaId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<WeeklyReportSubmission>().HasOne(x => x.CreatedBy).WithMany()
                .HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<WeeklyReportWorkflowHistory>().HasOne(x => x.Submission).WithMany(x => x.History)
                .HasForeignKey(x => x.WeeklyReportSubmissionId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WeeklyReportWorkflowHistory>().HasOne(x => x.User).WithMany()
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AnimalMigrationRecord>().HasOne(x => x.Submission).WithMany()
                .HasForeignKey(x => x.WeeklyReportSubmissionId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AnimalMigrationRecord>().HasOne(x => x.Woreda).WithMany().HasForeignKey(x => x.WoredaId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AnimalMigrationRecord>().HasOne(x => x.OriginLocation).WithMany().HasForeignKey(x => x.OriginLocationId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AnimalMigrationRecord>().HasOne(x => x.DestinationLocation).WithMany().HasForeignKey(x => x.DestinationLocationId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AnimalMigrationRecord>().HasIndex(x => new { x.WeeklyReportSubmissionId, x.Direction, x.OriginLocationId, x.DestinationLocationId, x.AnimalType }).IsUnique();
            modelBuilder.Entity<WeeklyAggregateSnapshot>()
                .HasIndex(x => new { x.ScopeLocationId, x.ScopeLevel, x.StartDate, x.EndDate, x.ApprovalStatus })
                .IsUnique();
            modelBuilder.Entity<SchoolReport>().HasIndex(x => new { x.WoredaId, x.StartDate, x.EndDate }).IsUnique();
            modelBuilder.Entity<SchoolReport>().HasOne(x => x.Woreda).WithMany().HasForeignKey(x => x.WoredaId).OnDelete(DeleteBehavior.Restrict);
        }
        public DbSet<AnimalDisease> AnimalDisease { get; set; } = default!;
    }

    public static class DbInitializer
    {
        private const string SeedPassword = "ChangeMe@123";

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EarlyWarningDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // EnsureCreated bypasses migration history and must not be mixed with
            // Database.Migrate. Always bring the schema up to date before seeding.
            await context.Database.MigrateAsync();

            // Seed a complete country -> region -> zone -> woreda chain.
            var locations = await SeedLocationsAsync(context);

            string[] roleNames =
            {
                "Supper Administrator",
                "Administrator",
                "RegionAdmin",
                "ZoneAdmin",
                "Data Encoder",
                "Data Approver",
                "Data Evaluator",
                "User"
            };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    EnsureSucceeded(
                        await roleManager.CreateAsync(new IdentityRole(roleName)),
                        $"creating role '{roleName}'");
                }
            }

            // Each operational role gets a user at the hierarchy level where it
            // performs its work. Existing users are updated instead of duplicated.
            await SeedUserAsync(userManager, "superadmin@example.com", "System", "Super Administrator",
                "Supper Administrator", locations.Country);
            await SeedUserAsync(userManager, "administrator@example.com", "National", "Administrator",
                "Administrator", locations.Country);
            await SeedUserAsync(userManager, "regionadmin@example.com", "Regional", "Administrator",
                "RegionAdmin", locations.Region);
            await SeedUserAsync(userManager, "zoneadmin@example.com", "Zonal", "Administrator",
                "ZoneAdmin", locations.Zone);
            await SeedUserAsync(userManager, "encoder@example.com", "Woreda", "Encoder",
                "Data Encoder", locations.Woreda);
            await SeedUserAsync(userManager, "approver@example.com", "Zonal", "Approver",
                "Data Approver", locations.Zone);
            await SeedUserAsync(userManager, "evaluator@example.com", "Regional", "Evaluator",
                "Data Evaluator", locations.Region);
            await SeedUserAsync(userManager, "user@example.com", "Woreda", "User",
                "User", locations.Woreda);
        }

        private static async Task<(Locations Country, Locations Region, Locations Zone, Locations Woreda)>
            SeedLocationsAsync(EarlyWarningDbContext context)
        {
            var country = await UpsertLocationAsync(context,
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "ETH", "Ethiopia", "ኢትዮጵያ", LocationLevel.ሀገር,
                "+251-11-000-0000", "National Headquarters", null);

            var region = await UpsertLocationAsync(context,
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                "ETH-AM", "Amhara", "አማራ", LocationLevel.ክልል,
                "+251-58-000-0000", "Amhara Regional Administration", country.Id);

            var zone = await UpsertLocationAsync(context,
                Guid.Parse("44444444-4444-4444-4444-444444444444"),
                "ETH-AM-CG", "Central Gondar", "ማዕከላዊ ጎንደር", LocationLevel.ዞን,
                "+251-58-100-0000", "Central Gondar Zone Administration", region.Id);

            var woreda = await UpsertLocationAsync(context,
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                "ETH-AM-CG-DT", "Dembiya", "ደምቢያ", LocationLevel.ወረዳ,
                "+251-58-200-0000", "Dembiya Woreda Administration", zone.Id);

            await UpsertLocationAsync(context, Guid.Parse("66666666-6666-6666-6666-666666666661"),
                "ETH-AM-CG-AL", "Alefa", "አለፋ", LocationLevel.ወረዳ,
                "+251-58-210-0000", "Alefa Woreda Administration", zone.Id);
            await UpsertLocationAsync(context, Guid.Parse("66666666-6666-6666-6666-666666666662"),
                "ETH-AM-CG-TK", "Takusa", "ጣቁሳ", LocationLevel.ወረዳ,
                "+251-58-220-0000", "Takusa Woreda Administration", zone.Id);

            // Operational Kebele records for the seeded Woreda encoder and Wizard.
            await UpsertLocationAsync(context, Guid.Parse("55555555-5555-5555-5555-555555555551"),
                "ETH-AM-CG-DT-01", "Aymba", "አይምባ", LocationLevel.ቀበሌ,
                "+251-58-200-0101", "Aymba Kebele", woreda.Id);
            await UpsertLocationAsync(context, Guid.Parse("55555555-5555-5555-5555-555555555552"),
                "ETH-AM-CG-DT-02", "Chuahit", "ጭዋሂት", LocationLevel.ቀበሌ,
                "+251-58-200-0102", "Chuahit Kebele", woreda.Id);
            await UpsertLocationAsync(context, Guid.Parse("55555555-5555-5555-5555-555555555553"),
                "ETH-AM-CG-DT-03", "Gorgora", "ጎርጎራ", LocationLevel.ቀበሌ,
                "+251-58-200-0103", "Gorgora Kebele", woreda.Id);
            await UpsertLocationAsync(context, Guid.Parse("55555555-5555-5555-5555-555555555554"),
                "ETH-AM-CG-DT-04", "Kola Diba", "ኮላ ድባ", LocationLevel.ቀበሌ,
                "+251-58-200-0104", "Kola Diba Kebele", woreda.Id);
            await UpsertLocationAsync(context, Guid.Parse("55555555-5555-5555-5555-555555555555"),
                "ETH-AM-CG-DT-05", "Seraba", "ሰራባ", LocationLevel.ቀበሌ,
                "+251-58-200-0105", "Seraba Kebele", woreda.Id);

            return (country, region, zone, woreda);
        }

        private static async Task<Locations> UpsertLocationAsync(
            EarlyWarningDbContext context,
            Guid id,
            string code,
            string name,
            string amharicName,
            LocationLevel level,
            string phoneNumber,
            string headerTitle,
            Guid? parentId)
        {
            // Prefer the stable seed ID, but also adopt a location previously
            // created with the same code to avoid a unique logical duplicate.
            var location = await context.Locations
                .FirstOrDefaultAsync(l => l.Id == id || l.LocationCode == code);

            if (location == null)
            {
                location = new Locations { Id = id };
                context.Locations.Add(location);
            }

            location.LocationCode = code;
            location.LocationName = name;
            location.LocationAmharicName = amharicName;
            location.Level = level;
            location.PhoneNumber = phoneNumber;
            location.CardHeaderTitle = headerTitle;
            location.ParentId = parentId;
            location.IsActive = true;

            await context.SaveChangesAsync();
            return location;
        }

        private static async Task SeedUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string firstName,
            string lastName,
            string role,
            Locations location)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    LocationId = location.Id,
                    UserLocationLevl = location.Level!.Value,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };

                EnsureSucceeded(await userManager.CreateAsync(user, SeedPassword),
                    $"creating seed user '{email}'");
            }
            else
            {
                user.FirstName = firstName;
                user.LastName = lastName;
                user.LocationId = location.Id;
                user.UserLocationLevl = location.Level!.Value;
                user.EmailConfirmed = true;
                EnsureSucceeded(await userManager.UpdateAsync(user),
                    $"updating seed user '{email}'");
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                EnsureSucceeded(await userManager.AddToRoleAsync(user, role),
                    $"assigning role '{role}' to '{email}'");
            }
        }

        private static void EnsureSucceeded(IdentityResult result, string operation)
        {
            if (result.Succeeded)
                return;

            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Database seeding failed while {operation}: {errors}");
        }
    }
}

