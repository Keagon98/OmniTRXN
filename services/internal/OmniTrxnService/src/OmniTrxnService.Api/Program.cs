using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using OmniTrxnService.Api.Middleware;
using OmniTrxnService.Application.Common.Interfaces;
using OmniTrxnService.Application.Common.Mappings;
using OmniTrxnService.Application.Services;
using OmniTrxnService.Infrastructure;
using OmniTrxnService.Infrastructure.Adapters;
using OmniTrxnService.Infrastructure.BackgroundJobs;
using OmniTrxnService.Infrastructure.ExternalServices;
using OmniTrxnService.Infrastructure.Persistence;
using OmniTrxnService.Infrastructure.Persistence.Repositories;
using Scalar.AspNetCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

string gatewayUrl = builder.Configuration["ServiceUrls:Gateway"];

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];
        options.Audience = builder.Configuration["Jwt:Audience"];
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("x-api-version"),
        new UrlSegmentApiVersionReader()
    );

}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "My Internal API";
        document.Info.Version = "v1";
        document.Info.Description = "API exposed through the gateway";
        document.Servers = new List<OpenApiServer>
        {
            new OpenApiServer { Url = gatewayUrl }
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

       
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };

        return Task.CompletedTask;
    });

    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        bool requiresAuth = metadata.OfType<IAuthorizeData>().Any();
        bool allowAnonymous = metadata.OfType<IAllowAnonymous>().Any();

        if (requiresAuth && !allowAnonymous)
        {
            operation.Security ??= new List<OpenApiSecurityRequirement>();
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", null)] = new List<string>()
            });
        }

        return Task.CompletedTask;
    });
});

builder.Services.AddDbContext<OmniTrxnDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("OmniTrxnDb"),
        sqlOptions => sqlOptions.MigrationsAssembly(typeof(OmniTrxnDbContext).Assembly.FullName)));


builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IVendorCustomerMapRepository, VendorCustomerMapRepository>();


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddHttpClient<ApiGatewayClient>(client =>
{
    client.BaseAddress = new Uri(gatewayUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<IVendorApiClient, OzowApiClient>();
builder.Services.AddScoped<IVendorApiClient, FnbSoapClient>();

builder.Services.AddScoped<IXmlToJsonAdapter, XmlToJsonAdapter>();
builder.Services.AddScoped<ITransactionNormalizer, TransactionNormalizer>();

builder.Services.AddScoped<ITransactionIngestionService, TransactionIngestionService>();
builder.Services.AddScoped<ITransactionQueryService, TransactionQueryService>();

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

builder.Services.AddHostedService<TransactionPollingService>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<OmniTrxnDbContext>("database");

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/v1.json");

    app.MapScalarApiReference(options =>
    {
        options.Title = "OmniTRXN Transaction API";
        options.Theme = ScalarTheme.Moon;
    });

}

// Global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();


app.MapControllers();
app.MapHealthChecks("/health");

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.SeedAsync(services);
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
        throw;
    }
}

app.Run();