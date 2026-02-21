using System.Text;
using Asp.Versioning;
using FluentValidation;
using slender_server.API.Middleware;
using slender_server.API.Options;
using slender_server.API.Services;
using slender_server.Application.Common;
using slender_server.Application.Common.Settings;
using slender_server.Application.DTOs.Auth;
using slender_server.Application.Interfaces.Services;
using slender_server.Infra.Auth;
using slender_server.Infra.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using slender_server.Application.DTOs.CalendarEventDTOs;
using slender_server.Application.DTOs.MessageDTOs;
using slender_server.Application.DTOs.NotificationDTOs;
using slender_server.Application.DTOs.ProjectDTOs;
using slender_server.Application.DTOs.TaskDTOs;
using slender_server.Application.DTOs.WorkspaceDTOs;
using slender_server.Application.Models.Sorting;
using slender_server.Application.Services;
using slender_server.Application.SortMappings;
using slender_server.Domain.Interfaces;
using slender_server.Infra.Repositories;

namespace slender_server.API;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options =>
            {
                options.ReturnHttpNotAcceptable = true;
            })
            .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver())
            .AddXmlSerializerFormatters();
        
        builder.Services.Configure<MvcOptions>(options =>
        {
            NewtonsoftJsonOutputFormatter formatter = options.OutputFormatters
                .OfType<NewtonsoftJsonOutputFormatter>()
                .First();

            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.JsonV1);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.JsonV2);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJson);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJsonV1);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJsonV2);
        });
        
        
        builder.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1.0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionSelector = new DefaultApiVersionSelector(options);

                options.ApiVersionReader = new MediaTypeApiVersionReaderBuilder()
                    .Template("application/vnd.slender.hateoas.{version}+json")  // Only match this specific pattern
                    .Build();
        
            })
            .AddMvc();

        builder.Services.AddOpenApi();

        builder.Services.AddResponseCaching();

        builder.Services.AddScoped<ILinkService, LinkService>();

        return builder;
    }

    public static WebApplicationBuilder AddErrorHandling(this WebApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
            };
        });
        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        return builder;
    }

    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(
                    builder.Configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application))
                .UseSnakeCaseNamingConvention());

        builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
            options
                .UseNpgsql(
                    builder.Configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Identity))
                .UseSnakeCaseNamingConvention());

        return builder;
    }
    
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssembly(typeof(RegisterUserDto).Assembly);
        builder.Services.AddScoped<IPaginationService,PaginationService>();
        builder.Services.AddScoped<IDataShapingService,DataShapingService>();
        builder.Services.AddScoped<ISortingService, SortingService>();
        
        builder.Services.AddSingleton<SortMappingDefinition<TaskDto, Domain.Entities.Task>, TaskSortMapping>();
        builder.Services.AddSingleton<SortMappingDefinition<ProjectDto, Domain.Entities.Project>, ProjectSortMapping>();
        builder.Services.AddSingleton<SortMappingDefinition<WorkspaceDto, Domain.Entities.Workspace>, WorkspaceSortMapping>();
        builder.Services.AddSingleton<SortMappingDefinition<NotificationDto, Domain.Entities.Notification>, NotificationSortMapping>();
        builder.Services.AddSingleton<SortMappingDefinition<MessageDto, Domain.Entities.Message>, MessageSortMapping>();
        builder.Services.AddSingleton<SortMappingDefinition<CalendarEventDto, Domain.Entities.CalendarEvent>, CalendarEventSortMapping>();

        return builder;
    }
    
    public static WebApplicationBuilder AddAuthenticationServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationIdentityDbContext>();

        builder.Services.Configure<JwtAuthOptions>(builder.Configuration.GetSection(JwtAuthOptions.SectionName));
        JwtAuthOptions jwtAuthOptions = builder.Configuration
            .GetSection(JwtAuthOptions.SectionName)
            .Get<JwtAuthOptions>()!;

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
                
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtAuthOptions.Issuer,
                    ValidAudience = jwtAuthOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthOptions.Key))
                };
            });

        builder.Services.AddAuthorization();
        builder.Services.AddScoped<IAuthService, AuthService>();

        return builder;
    }
    
    public static WebApplicationBuilder AddInfrastructureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<ITokenProvider,TokenProvider>();
        
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<ITaskRepository,TaskRepository>();
        builder.Services.AddScoped<IProjectRepository,ProjectRepository>();
        
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUserContext,UserContext>();
        
        
        builder.Services.AddMemoryCache();
        builder.Services.AddHttpContextAccessor();

        return builder;
    }
    
    public static WebApplicationBuilder AddCorsPolicy(this WebApplicationBuilder builder)
    {
        CorsOptions corsOptions = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()!;

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(CorsOptions.PolicyName, policy =>
            {
                policy
                    .WithOrigins(corsOptions.AllowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return builder;
    }
}