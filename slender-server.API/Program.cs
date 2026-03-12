using slender_server.API;
using slender_server.API.Extensions;
using slender_server.API.Options;
using slender_server.Infra.Database;
using Microsoft.EntityFrameworkCore;
using slender_server.Infra.Database.Seeders;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.AddApiServices()
    .AddApplicationServices()
    .AddInfrastructureServices()
    .AddAuthenticationServices()
    .AddDatabase()
    .AddErrorHandling()
    .AddCorsPolicy();


var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var identityContext = services.GetRequiredService<ApplicationIdentityDbContext>();
            var appContext = services.GetRequiredService<ApplicationDbContext>();

        
            logger.LogInformation("⏳ Applying Identity migrations...");
            await identityContext.Database.MigrateAsync();
            logger.LogInformation("✅ Identity migrations done");

            logger.LogInformation("⏳ Applying Application migrations...");
            await appContext.Database.MigrateAsync();
            logger.LogInformation("✅ Application migrations done");
        
            // Seed Identity
            await IdentitySeeder.SeedAsync(userManager, roleManager);
        
            // Seed Application Data
            await ApplicationSeeder.SeedAsync(appContext, userManager);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Migration failed - {Message}", ex.Message);
            throw; // fail fast so you know immediately
        }
    }
}

app.UseExceptionHandler();
// app.UseHttpsRedirection();
app.UseResponseCaching();
app.UseCors(CorsOptions.PolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();