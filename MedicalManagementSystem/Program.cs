using MedicalManagementSystem.Components;
using MedicalManagementSystem.Data;
using MedicalManagementSystem.Repositories;
using MedicalManagementSystem.Services;
using Microsoft.EntityFrameworkCore;

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
                    logger.LogInformation("Database connection successful. Ensuring database schema...");
                    context.Database.EnsureCreated();
                    logger.LogInformation("Database schema ensured successfully.");
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
