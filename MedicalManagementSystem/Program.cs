using MedicalManagementSystem.Components;
using MedicalManagementSystem.Data;
using MedicalManagementSystem.Models;
using MedicalManagementSystem.Repositories;
using MedicalManagementSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.Migrations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure Entity Framework Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(30);
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableServiceProviderCaching();
});

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Set to true in production
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("DoctorOrAdmin", policy => policy.RequireRole("Admin", "Doctor"));
    options.AddPolicy("StaffOrAdmin", policy => policy.RequireRole("Admin", "Doctor", "Nurse", "LabStaff"));
});

// Add authentication state provider for Blazor
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// Register Repository Pattern (DRY Principle)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Register Services
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IProcedureService, ProcedureService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ILabTestService, LabTestService>();

var app = builder.Build();

// Ensure database is created (Docker-friendly approach)
// This will automatically create the database schema when the container starts
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Checking database connection...");
        
        // Wait for database to be ready (with retry logic for Docker)
        var maxRetries = 10;
        var retryDelay = TimeSpan.FromSeconds(3);
        
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                if (context.Database.CanConnect())
                {
                    logger.LogInformation("Database connection successful. Applying migrations...");
                    
                    // Always use migrations - never use EnsureCreated to avoid partial table creation
                    try
                    {
                        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                        
                        if (pendingMigrations.Any())
                        {
                            logger.LogInformation($"Found {pendingMigrations.Count()} pending migration(s). Applying...");
                            logger.LogInformation($"Pending migrations: {string.Join(", ", pendingMigrations)}");
                            await context.Database.MigrateAsync();
                            logger.LogInformation("Database migrations applied successfully.");
                        }
                        else if (appliedMigrations.Any())
                        {
                            logger.LogInformation($"No pending migrations. Database is up to date. Applied migrations: {string.Join(", ", appliedMigrations)}");
                        }
                        else
                        {
                            logger.LogWarning("No migrations found in the database. Please create an initial migration.");
                            logger.LogWarning("Run: docker exec blazor-app dotnet ef migrations add InitialIdentityMigration --project MedicalManagementSystem/MedicalManagementSystem.csproj");
                            logger.LogWarning("Then restart the container to apply the migration.");
                            // Don't use EnsureCreated as it can create partial schemas
                            throw new InvalidOperationException("No migrations found. Please create an initial migration first.");
                        }
                    }
                    catch (Exception migrateEx)
                    {
                        logger.LogError(migrateEx, "Failed to apply migrations.");
                        // Don't fallback to EnsureCreated - it can create incomplete schemas
                        logger.LogError("Please ensure migrations are created and applied correctly.");
                        throw;
                    }

                    // Initialize roles and admin user
                    await InitializeRolesAndUsersAsync(services, logger);
                    break;
                }
            }
            catch (Exception ex)
            {
                if (i == maxRetries - 1)
                {
                    logger.LogError(ex, "Failed to connect to database after {MaxRetries} attempts.", maxRetries);
                    // Don't throw - let the app start anyway, database might be available later
                    logger.LogWarning("Application will continue without database initialization. Database may become available later.");
                    break;
                }
                logger.LogWarning("Database not ready yet. Retrying in {Delay} seconds... (Attempt {Attempt}/{MaxRetries}). Error: {Error}", 
                    retryDelay.TotalSeconds, i + 1, maxRetries, ex.Message);
                Thread.Sleep(retryDelay);
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating/connecting to the database.");
        // Don't throw - let the app start anyway, migrations can be run manually if needed
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// Add authentication & authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

async Task InitializeRolesAndUsersAsync(IServiceProvider serviceProvider, ILogger logger)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Create roles
    string[] roles = { "Admin", "Doctor", "Nurse", "LabStaff", "Receptionist" };
    
    foreach (var roleName in roles)
    {
        var roleExists = await roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
            logger.LogInformation($"Created role: {roleName}");
        }
    }

    // Create default admin user
    var adminEmail = "admin@mmgc.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "System",
            LastName = "Administrator",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(adminUser, "Arsal@112231");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger.LogInformation("Default admin user created successfully.");
            logger.LogInformation($"Admin Email: {adminEmail}");
            logger.LogInformation("Admin Password: Arsal@112231");
        }
        else
        {
            logger.LogError($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        // Ensure admin is in Admin role
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger.LogInformation("Admin user added to Admin role.");
        }
    }
}

app.Run();
