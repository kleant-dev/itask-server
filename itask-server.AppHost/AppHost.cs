IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// PostgreSQL — resource name "itask-postgres" becomes the container hostname.
// Aspire injects the connection string under the database resource name,
// so we name the database "Database" to match ConnectionStrings:Database
// that DependencyInjection.AddDatabase() already reads.
// ---------------------------------------------------------------------------
var postgres = builder.AddPostgres("itask-postgres")
    .WithImage("postgres", "18-alpine")
    .WithHostPort(34515)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin();

var db = postgres.AddDatabase("Database", "itask-db");  // First param = resource name, second = actual DB name
// ---------------------------------------------------------------------------
// API
// ---------------------------------------------------------------------------
builder.AddProject<Projects.itask_server_API>("itask-api")
    .WithReference(db)
    .WaitFor(db)
    .WithExternalHttpEndpoints()
    .WithHttpsEndpoint(port: 7001)
    .WithEnvironment("ASPNETCORE_Kestrel__Certificates__Default__Path", 
        $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.aspnet/https/aspnetapp.pfx")
    .WithEnvironment("ASPNETCORE_Kestrel__Certificates__Default__Password", "YourSecurePassword");

await builder.Build().RunAsync();