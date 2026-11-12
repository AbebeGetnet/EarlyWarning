using EarlyWarning.Data;
using EarlyWarning.Models;
using EarlyWarning.Service;
using EarlyWarning.Repositories;
using EarlyWarning.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

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
builder.Services.AddScoped<IReportSaveService, ReportSaveService>();
builder.Services.AddScoped<IWeeklyAggregationService, WeeklyAggregationService>();
builder.Services.AddScoped<IWeeklyFormReportService, WeeklyFormReportService>();
builder.Services.AddScoped<IWeeklyMatrixReportService, WeeklyMatrixReportService>();
builder.Services.AddScoped<IWeeklyIndicatorService, WeeklyIndicatorService>();
builder.Services.AddScoped<IWeeklySubmissionFormLoader, WeeklySubmissionFormLoader>();
builder.Services.AddScoped<IReportScopeService, ReportScopeService>();
builder.Services.AddScoped<ILocationImportService, LocationImportService>();
builder.Services.AddHttpContextAccessor();

// DbContext ???? ApplicationDbContext ????? ????
builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<EarlyWarningDbContext>());

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

// Every POST: age and disability rows of a beneficiary table may not exceed household heads + family.
builder.Services.AddControllersWithViews(options => options.Filters.Add<EarlyWarning.Extensions.BeneficiaryValidationFilter>()).AddJsonOptions(options =>
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session expires after 30 minutes of inactivity
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IReportFlowService, ReportFlowService>();

// Compress static assets only (CSS/JS/fonts/JSON): AdminLTE's 1.4 MB stylesheet becomes ~200 KB.
// HTML is left uncompressed on purpose: compressing pages that carry anti-forgery tokens over HTTPS
// exposes them to BREACH-style attacks.
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = ["text/css", "application/javascript", "text/javascript", "application/json", "image/svg+xml", "font/woff", "font/woff2", "application/font-woff"];
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(o => o.Level = System.IO.Compression.CompressionLevel.Fastest);
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(o => o.Level = System.IO.Compression.CompressionLevel.Fastest);

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
app.UseResponseCompression();
// Browsers keep static files for a week. The app's own files are linked with asp-append-version,
// so a changed file gets a new URL and is fetched again straight away.
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context => context.Context.Response.Headers.CacheControl = "public,max-age=604800"
});

app.UseRouting();
app.UseSession();
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
        var database = scope.ServiceProvider.GetRequiredService<EarlyWarningDbContext>();
        await database.Database.MigrateAsync();
        await DbInitializer.InitializeAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database initialization failed");
    }
}

app.Run();
