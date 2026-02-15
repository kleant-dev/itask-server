using itask_server.API;
using itask_server.API.Extensions;
using itask_server.API.Options;
using itask_server.Infra.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.AddApiServices()
    .AddApplicationServices()
    .AddAuthenticationServices()
    .AddDatabase()
    .AddErrorHandling()
    .AddCorsPolicy();


var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.SeedInitialDataAsync();
    // await using var scope = app.Services.CreateAsyncScope();
    //
    // var appDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // await appDb.Database.MigrateAsync();
    //
    // var identityDb = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
    // await identityDb.Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors(CorsOptions.PolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();