using Capitol_Theatre.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using Capitol_Theatre.Services;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ⬇️ Ensure launchSettings.json is included
        builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.AddJsonFile("Properties/launchSettings.json", optional: true, reloadOnChange: true);
        });

        builder.Logging.AddConsole();
        var logger = builder.Logging.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
        logger.LogInformation("🚀 Logging initialized");

        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            options.SignIn.RequireConfirmedAccount = false)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
        builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo("/app/keyring"));
        builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
        builder.Services.AddRazorPages();
        SQLitePCL.Batteries_V2.Init();

        var tempProvider = builder.Services.BuildServiceProvider();
        var tempLogger = tempProvider.GetRequiredService<ILogger<Program>>();

        tempLogger.LogWarning("💡 AdminEmail = {email}", builder.Configuration["AdminEmail"]);
        tempLogger.LogWarning("💡 EnableAdminSeeding = {enabled}", builder.Configuration["EnableAdminSeeding"]);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
        }

        app.UseDeveloperExceptionPage();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();

        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var error = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(error, "Unhandled exception");

                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("An unexpected error occurred.");
            });
        });

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages();

        using (var migrationScope = app.Services.CreateScope())
        {
            var db = migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        }

        if (!app.Environment.IsDevelopment() || builder.Configuration["EnableAdminSeeding"] == "true")
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

            var adminEmail = builder.Configuration["AdminEmail"];
            var adminPassword = builder.Configuration["AdminPassword"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                throw new InvalidOperationException("AdminEmail or AdminPassword missing from environment.");
            }

            if (!await roleManager.RoleExistsAsync("Administrator"))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole("Administrator"));
                if (!roleResult.Succeeded)
                    throw new Exception("Failed to create Administrator role.");
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var user = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var createResult = await userManager.CreateAsync(user, adminPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create admin user: {errors}");
                }

                await userManager.AddToRoleAsync(user, "Administrator");
            }
        }

        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            Console.WriteLine("🔴 Unhandled exception: " + e.ExceptionObject?.ToString());
            System.IO.File.AppendAllText("crash.log", $"[Unhandled] {e.ExceptionObject}\n");
        };

        await app.RunAsync();
    }
}
