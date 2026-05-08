using EarlyWarning.Enums;
using EarlyWarning.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;

namespace EarlyWarning.Data
{
    public class EarlyWarningDbContext : IdentityDbContext<IdentityUser>
    {
        private readonly EarlyWarningDbContext _context;
        public EarlyWarningDbContext(DbContextOptions<EarlyWarningDbContext> options) : base(options)
        {            
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }        
        public DbSet<Locations> Locations {  get; set; } 
        public DbSet<PasswordResetKey> PasswordResetKeys {  get; set; }

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
        }
    }

    public static class DbInitializer
    {
        // Existing location-only initializer (synchronous)
        public static void Initialize(EarlyWarningDbContext context)
        {
            context.Database.EnsureCreated(); // or Migrate()

            if (context.Locations.Any())
                return;

            var ethiopia = new Locations
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                LocationName = "Ethiopia",
                LocationAmharicName = "ኢትዮጵያ",
                LocationCode = "ETH",
                PhoneNumber = "+251-111-111111",
                Level = LocationLevel.ሀገር,
                CardHeaderTitle = "National HQ",
                IsActive = true
            };

            var addis = new Locations
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                LocationName = "Addis Ababa",
                LocationAmharicName = "አዲስ አበባ",
                LocationCode = "ETH-ADD",
                PhoneNumber = "+251-111-222222",
                Level = LocationLevel.ክልል,
                CardHeaderTitle = "City Administration",
                IsActive = true,
                ParentId = ethiopia.Id
            };

            context.Locations.AddRange(ethiopia, addis);
            context.SaveChanges();
        }

        // New async initializer that includes roles and users
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EarlyWarningDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // First, seed locations (calls your existing sync method)
            Initialize(context);

            // ---------- 1. Seed Roles ----------
            string[] roleNames = { "Supper Administrator", "Data Encoder", "Data Approver" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // ---------- 2. Get a default location (Ethiopia) to associate with users ----------
            var defaultLocation = context.Locations.FirstOrDefault(l => l.LocationCode == "ETH")
                                  ?? context.Locations.First();

            // ---------- 3. Seed Admin User ----------
            var adminEmail = "admin@example.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Admin",
                    LocationId = defaultLocation.Id,
                    UserLocationLevl = defaultLocation.Level ?? LocationLevel.ሀገር,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // ---------- 4. Seed a Regular User (optional) ----------
            var userEmail = "user@example.com";
            var regularUser = await userManager.FindByEmailAsync(userEmail);
            if (regularUser == null)
            {
                regularUser = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    FirstName = "Regular",
                    LastName = "User",
                    LocationId = defaultLocation.Id,
                    UserLocationLevl = LocationLevel.ክልል,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(regularUser, "User@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(regularUser, "User");
                }
            }
        }
    }
}

