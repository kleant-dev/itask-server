using System.Text;
using System.Threading.RateLimiting;
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
using Microsoft.AspNetCore.RateLimiting;
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
using slender_server.Infra.Services;

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

        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, ct) =>
            {
                document.Info.Title = "Slender API";
                document.Info.Version = "1.0";
                document.Info.Description = "Backend API for Slender. Use this schema for frontend type generation and API discovery.";
                return Task.CompletedTask;
            });
        });

        builder.Services.AddResponseCaching();
        builder.Services.AddRateLimiter(options =>
            {
                options.AddPolicy("auth-rate-limiting", httpContext =>
                {
                    string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetSlidingWindowLimiter(ip, _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
                });

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.Headers.RetryAfter = "60";

                    await context.HttpContext.Response.WriteAsync(
                        "Too many requests. Please try again later.", cancellationToken);
                };
            }
        );
        builder.Services.AddScoped<ILinkService, LinkService>();
        builder.Services.AddSignalR(); 

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
        var jwtAuthOptions = builder.Configuration
            .GetSection(JwtAuthOptions.SectionName)
            .Get<JwtAuthOptions>()
            ?? throw new InvalidOperationException($"Configuration section '{JwtAuthOptions.SectionName}' is missing or invalid.");

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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthOptions.Key)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var token = context.Request.Query["access_token"];
                            if (!string.IsNullOrEmpty(token) &&
                                context.HttpContext.Request.Path.StartsWithSegments("/hubs/chat"))
                            {
                                context.Token = token;
                            }
                            return Task.CompletedTask;
                        }
                    };
            });

        builder.Services.AddAuthorization();
        builder.Services.AddScoped<IAuthService, AuthService>();

        return builder;
    }
    
    public static WebApplicationBuilder AddInfrastructureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<ITokenProvider, TokenProvider>();
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<IUserContext, UserContext>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ITaskService, TaskService>();
        builder.Services.AddScoped<IProjectService, ProjectService>();
        builder.Services.AddScoped<ILabelService, LabelService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
        builder.Services.AddScoped<IChannelService, ChannelService>();
        builder.Services.AddScoped<ICalendarEventService, CalendarEventService>();

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ITaskRepository, TaskRepository>();
        builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
        builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        builder.Services.AddScoped<IWorkspaceMemberRepository, WorkspaceMemberRepository>();
        builder.Services.AddScoped<IWorkspaceInviteRepository, WorkspaceInviteRepository>();
        builder.Services.AddScoped<ILabelRepository, LabelRepository>();
        builder.Services.AddScoped<IChannelRepository, ChannelRepository>();
        builder.Services.AddScoped<IChannelMemberRepository, ChannelMemberRepository>();
        builder.Services.AddScoped<IMessageRepository, MessageRepository>();
        builder.Services.AddScoped<ICalendarEventRepository, CalendarEventRepository>();
        builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        builder.Services.AddScoped<IMessageService, MessageService>();
        builder.Services.AddMemoryCache();
        builder.Services.AddHttpContextAccessor();

        return builder;
    }
    
    public static WebApplicationBuilder AddCorsPolicy(this WebApplicationBuilder builder)
    {
        var corsOptions = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()
            ?? throw new InvalidOperationException($"Configuration section '{CorsOptions.SectionName}' is missing or invalid.");

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