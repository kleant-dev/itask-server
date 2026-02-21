IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// PostgreSQL — resource name "slender-postgres" becomes the container hostname.
// Aspire injects the connection string under the database resource name,
// so we name the database "Database" to match ConnectionStrings:Database
// that DependencyInjection.AddDatabase() already reads.
// ---------------------------------------------------------------------------
var postgres = builder.AddPostgres("slender-postgres")
    .WithImage("postgres", "18-alpine")
    .WithHostPort(34515)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("slender-pgdata")
    .WithPgAdmin();

var db = postgres.AddDatabase("Database", "slender-db");  // First param = resource name, second = actual DB name
// ---------------------------------------------------------------------------
// API
// ---------------------------------------------------------------------------
builder.AddProject<Projects.slender_server_API>("slender-api")
    .WithReference(db)
    .WaitFor(db)
    .WithExternalHttpEndpoints()
    .WithHttpsEndpoint(port: 7001, name: "https-custom");

await builder.Build().RunAsync();