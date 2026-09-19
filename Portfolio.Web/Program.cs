using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Portfolio.Web.Localization;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data.Context;
using Portfolio.Data.Identity;
using Portfolio.Data.Seed;
using Portfolio.Business.Interfaces;
using Portfolio.Business.Services;
using Portfolio.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);


// =====================================================
// MVC & Localization
// =====================================================

builder.Services.AddLocalization();

builder.Services.AddControllersWithViews(options =>
{
})
.AddViewLocalization()
.AddDataAnnotationsLocalization();

builder.Services.AddSingleton<AppLocalizer>();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en"),
        new CultureInfo("ar")
    };

    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.ApplyCurrentCultureToResponseHeaders = true;

    // Use separate language preferences for the public website and Admin area.
    // PublicDefaultLanguage applies only when the visitor has not explicitly
    // selected a public language. Admin language is independent of this setting.
    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new CustomRequestCultureProvider(async context =>
    {
        var isAdmin = context.Request.Path.StartsWithSegments(
            "/Admin",
            StringComparison.OrdinalIgnoreCase);

        try
        {
            if (isAdmin)
            {
                var adminCulture = context.Request.Cookies[CultureCookieNames.Admin];

                return adminCulture is "ar" or "en"
                    ? new ProviderCultureResult(adminCulture, adminCulture)
                    : new ProviderCultureResult("en", "en");
            }

            var websiteSettings = context.RequestServices
                .GetRequiredService<IWebsiteSettingService>();

            var values = await websiteSettings.GetAllAsync(
                context.RequestAborted);

            var enabled = true;

            if (values.TryGetValue(
                    "PublicLocalizationEnabled",
                    out var raw) &&
                bool.TryParse(raw, out var parsed))
            {
                enabled = parsed;
            }

            if (!enabled)
                return new ProviderCultureResult("en", "en");

            var publicCulture = context.Request.Cookies[CultureCookieNames.Public];

            if (publicCulture is "ar" or "en")
                return new ProviderCultureResult(publicCulture, publicCulture);

            var defaultLanguage = values.GetValueOrDefault(
                "PublicDefaultLanguage",
                "en");

            return defaultLanguage is "ar"
                ? new ProviderCultureResult("ar", "ar")
                : new ProviderCultureResult("en", "en");
        }
        catch
        {
            // Never block a request because the optional website settings
            // feature could not be loaded.
            return isAdmin
                ? new ProviderCultureResult("en", "en")
                : null;
        }
    }));
});


// =====================================================
// Database
// =====================================================

builder.Services.AddDbContext<PortfolioDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

// =====================================================
// Business Services
// =====================================================

builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<IWebsiteSettingService, WebsiteSettingService>();
builder.Services.AddScoped<IContactMessageService, ContactMessageService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IRoleManagementService, RoleManagementService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();


// =====================================================
// Identity
// =====================================================

builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        // Password
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;


        // User
        options.User.RequireUniqueEmail = true;


        // Lockout
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        options.Lockout.MaxFailedAccessAttempts = 5;

        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<PortfolioDbContext>()
    .AddDefaultTokenProviders();


// =====================================================
// Cookie Configuration
// =====================================================

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Account/Login";

    options.AccessDeniedPath = "/Admin/Account/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromHours(8);

    options.SlidingExpiration = true;
});


var app = builder.Build();


// =====================================================
// Database Seeding
// =====================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context =
        services.GetRequiredService<PortfolioDbContext>();

    // Create/update the existing schema first, then add the bilingual columns.
    await context.Database.MigrateAsync();

    // Backfill the bilingual website-builder columns for existing SQLite databases.
    // SQLite does not support ADD COLUMN IF NOT EXISTS, so inspect PRAGMA first.
    static async Task EnsureColumnAsync(PortfolioDbContext db, string table, string column)
    {
        var safeTable = table.Replace("\"", "\"\"");
        var safeColumn = column.Replace("\"", "\"\"");
        var exists = await db.Database
            .SqlQueryRaw<int>($"SELECT COUNT(*) AS Value FROM pragma_table_info('{safeTable}') WHERE name = '{safeColumn}'")
            .SingleAsync();

        if (exists == 0)
            await db.Database.ExecuteSqlRawAsync($@"ALTER TABLE ""{safeTable}"" ADD COLUMN ""{safeColumn}"" TEXT NULL;");
    }

    foreach (var column in new[] { "NameEn", "NameAr", "TitleEn", "TitleAr", "MetaTitleEn", "MetaTitleAr", "MetaDescriptionEn", "MetaDescriptionAr" })
        await EnsureColumnAsync(context, "Pages", column);

    foreach (var column in new[] { "TitleEn", "TitleAr", "SubtitleEn", "SubtitleAr", "DescriptionEn", "DescriptionAr" })
        await EnsureColumnAsync(context, "Sections", column);

    var userManager =
        services.GetRequiredService<UserManager<ApplicationUser>>();

    var roleManager =
        services.GetRequiredService<RoleManager<ApplicationRole>>();

    await DatabaseSeeder.SeedAsync(
        context,
        userManager,
        roleManager);

    // Feature flags are independent from content seeding so existing production data
    // is never overwritten just because a new setting was introduced.
    static async Task EnsureWebsiteSettingAsync(
        PortfolioDbContext db,
        string key,
        string value)
    {
        var exists = await db.WebsiteSettings.AnyAsync(x => x.Key == key);
        if (!exists)
        {
            db.WebsiteSettings.Add(new Portfolio.Data.Entities.WebsiteSetting
            {
                Id = Guid.NewGuid(),
                Key = key,
                Value = value
            });
            await db.SaveChangesAsync();
        }
    }

    await EnsureWebsiteSettingAsync(context, "PublicLocalizationEnabled", "True");
    await EnsureWebsiteSettingAsync(context, "PublicDarkModeEnabled", "True");
    await EnsureWebsiteSettingAsync(context, "PublicDefaultLanguage", "en");

    // AuditLogs is a cross-cutting table. Keep its creation idempotent so
    // existing development databases can adopt audit logging without a
    // destructive migration. A normal EF migration can be added later.
    await context.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS AuditLogs (
            Id TEXT NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
            UserId TEXT NULL,
            UserName TEXT NOT NULL,
            ActionType TEXT NOT NULL,
            Area TEXT NOT NULL,
            Controller TEXT NOT NULL,
            Action TEXT NOT NULL,
            EntityId TEXT NULL,
            Path TEXT NOT NULL,
            HttpMethod TEXT NOT NULL,
            StatusCode INTEGER NOT NULL,
            IpAddress TEXT NULL,
            UserAgent TEXT NULL,
            Details TEXT NULL,
            OccurredAtUtc TEXT NOT NULL
        );
        CREATE INDEX IF NOT EXISTS IX_AuditLogs_OccurredAtUtc ON AuditLogs(OccurredAtUtc);
        CREATE INDEX IF NOT EXISTS IX_AuditLogs_UserId ON AuditLogs(UserId);
        CREATE INDEX IF NOT EXISTS IX_AuditLogs_IpAddress ON AuditLogs(IpAddress);
        CREATE INDEX IF NOT EXISTS IX_AuditLogs_Controller_Action ON AuditLogs(Controller, Action);
        """);
}


// =====================================================
// HTTP Pipeline
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseRequestLocalization();

app.UseStaticFiles();

app.UseRouting();

app.UseMiddleware<AuditLoggingMiddleware>();

app.UseAuthentication();

app.UseAuthorization();


// =====================================================
// Area Route
// =====================================================

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Dynamic public pages such as /about, /products and /services.
app.MapControllerRoute(
    name: "pages",
    pattern: "{slug}",
    defaults: new { controller = "Page", action = "Index" });

// =====================================================
// Default Route
// =====================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();