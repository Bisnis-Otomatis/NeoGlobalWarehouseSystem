using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NeoGlobalWarehouseSystem.Components;
using NeoGlobalWarehouseSystem.Components.Account;
using NeoGlobalWarehouseSystem.Data;
using NeoGlobalWarehouseSystem.Data.ApplicationDb;
using MudBlazor.Services;

namespace NeoGlobalWarehouseSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddMudServices();
            builder.Services.AddHttpClient();
            builder.Services.AddControllers();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddIdentityCookies();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentityCore<User>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<User>, IdentityNoOpEmailSender>();

            var app = builder.Build();

            // Initialize database and seed admin user
            await InitializeDatabaseAsync(app);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();
            app.MapControllers();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            app.Run();
        }

        private static async Task InitializeDatabaseAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                // Get database context and apply migrations
                var context = services.GetRequiredService<ApplicationDbContext>();
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully.");

                // Seed admin user
                await SeedAdminUserAsync(services, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }

        private static async Task SeedAdminUserAsync(IServiceProvider services, ILogger logger)
        {
            try
            {
                var userManager = services.GetRequiredService<UserManager<User>>();
                
                const string adminEmail = "evandernatanael144@gmail.com";
                const string adminPassword = "Evander_155";
                
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                
                if (adminUser == null)
                {
                    logger.LogInformation("Creating admin user...");
                    
                    var user = new User
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        Role = UserRole.Admin,
                        Name = "Evander",
                        IdCardNumber = "2171030905010008"
                    };
                    
                    var result = await userManager.CreateAsync(user, adminPassword);
                    
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Admin user created successfully.");
                        
                        // Ensure the Role property is set correctly
                        user.Role = UserRole.Admin;
                        await userManager.UpdateAsync(user);
                        
                        logger.LogInformation("Admin user role updated successfully.");
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        logger.LogError("Failed to create admin user. Errors: {Errors}", errors);
                    }
                }
                else
                {
                    logger.LogInformation("Admin user already exists.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the admin user.");
                throw;
            }
        }
    }
}
