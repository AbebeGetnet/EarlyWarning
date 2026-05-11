using EarlyWarning.Data;
using EarlyWarning.Models;
using EarlyWarning.Repositories;
using EarlyWarning.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddScoped<IAnimalHealthStatusRepository, AnimalHealthStatusRepository>();
builder.Services.AddScoped<AnimalHealthStatusService>(); 
builder.Services.AddScoped<IAnimalWaterSupplyStatusRepository, AnimalWaterSupplyStatusRepository>();
builder.Services.AddScoped<AnimalWaterSupplyStatusService>();
builder.Services.AddScoped<IPastureStatusRepository, PastureStatusRepository>();
builder.Services.AddScoped<PastureStatusService>();
builder.Services.AddScoped<IRainfallReportRepository, RainfallReportRepository>();
builder.Services.AddScoped<ICropPestAndDeseasReportRepository, CropPestAndDeseasReportRepository>();
builder.Services.AddScoped<ICropGrowthRepository, CropGrowthRepository>();
builder.Services.AddScoped<RainfallReportService>();
builder.Services.AddScoped<IFarmingActivityRepository, FarmingActivityRepository>();
builder.Services.AddScoped<FarmingActivityService>();
builder.Services.AddScoped<ICropGrowthRepository, CropGrowthRepository>();
builder.Services.AddScoped<CropGrowthService>();
builder.Services.AddHttpContextAccessor();


builder.Services.AddDbContext<EarlyWarningDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    // Optional: customize cookie settings here if needed
    //options.Cookie.SameSite = SameSiteMode.Lax;
    //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
    .AddEntityFrameworkStores<EarlyWarningDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();   // <-- Added
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

 //Initialize database
using (var scope = app.Services.CreateScope())
{
    try
    {
        await DbInitializer.InitializeAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database initialization failed");
    }
}

app.Run();