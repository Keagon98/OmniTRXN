using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using OmniTrxnGateway.Service;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);
string ozowUrl = builder.Configuration["ServiceUrls:Ozow"];
string fnbUrl = builder.Configuration["ServiceUrls:Fnb"];
string omniTrxnUrl = builder.Configuration["ServiceUrls:OmniTrxn"];

builder.Services.AddHttpClient();

// ===================== Logging =====================
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// ===================== YARP Configuration (In-Memory) =====================
static RouteConfig[] GetRoutes()
{
    return new[]
    {
        // Route to SOAP external service
        new RouteConfig
        {
            RouteId = "soap",
            ClusterId = "soap-cluster",
            Match = new RouteMatch
            {
                Path = "/external/soap/{**catch-all}",
                Methods = new[] { "GET", "POST" }
            },
            Transforms = new[]
            {
                new Dictionary<string, string> { ["PathRemovePrefix"] = "/external/soap" },

            },
            Metadata = new Dictionary<string, string>
            {
                { "RateLimiterPolicy", "External" }
            }
        },
        // Route to REST external service
        new RouteConfig
        {
            RouteId = "rest",
            ClusterId = "rest-cluster",
            Match = new RouteMatch
            {
                Path = "/external/rest/{**catch-all}",
                Methods = new[] { "GET", "POST", "PUT", "DELETE" }
            },
            Transforms = new[]
            {
                new Dictionary<string, string> { ["PathRemovePrefix"] = "/external/rest" },
            },
            Metadata = new Dictionary<string, string>
            {
                { "RateLimiterPolicy", "External" }
            }
        },
        // Route to internal .NET Web API (requires authentication)
        new RouteConfig
        {
            RouteId = "internal-api",
            ClusterId = "internal-cluster",
            AuthorizationPolicy = "Authenticated",
            Match = new RouteMatch
            {
                Path = "/api/{**catch-all}",
                Methods = new[] { "GET", "POST", "PUT", "DELETE" }
            },
            Metadata = new Dictionary<string, string>
            {
                { "RateLimiterPolicy", "Global" }
            }
        },
        new RouteConfig
        {
            RouteId = "internal-api-docs",
            ClusterId = "internal-cluster",
            Match = new RouteMatch { Path = "/api/scalar/{**catch-all}" },
            Transforms = new[] { new Dictionary<string, string> { ["PathRemovePrefix"] = "/api" } }
        },
        new RouteConfig
        {
            RouteId = "internal-api-openapi",
            ClusterId = "internal-cluster",
            Match = new RouteMatch { Path = "/api/openapi/{**catch-all}" },
            Transforms = new[] { new Dictionary<string, string> { ["PathRemovePrefix"] = "/api" } }
        }
    };
}

static ClusterConfig[] GetClusters(string ozowUrl, string fnbUrl, string OmniTrxnUrl)
{
    return new[]
    {
        new ClusterConfig
        {
            ClusterId = "soap-cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "soap-dest", new DestinationConfig { Address = fnbUrl } }
            },
            HealthCheck = new HealthCheckConfig
            {
                Active = new ActiveHealthCheckConfig
                {
                    Enabled = true,
                    Interval = TimeSpan.FromSeconds(30),
                    Timeout = TimeSpan.FromSeconds(5),
                    Policy = "ConsecutiveFailures",
                    Path = "/ws/CustomerTransactions.wsdl"
                }
            },
            HttpRequest = new ForwarderRequestConfig
            {
                ActivityTimeout = TimeSpan.FromSeconds(30),
                Version = Version.Parse("1.1"),
                VersionPolicy = HttpVersionPolicy.RequestVersionOrLower
            }
        },
        new ClusterConfig
        {
            ClusterId = "rest-cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "rest-dest", new DestinationConfig { Address =  ozowUrl } }
            },
            HealthCheck = new HealthCheckConfig
            {
                Active = new ActiveHealthCheckConfig
                {
                    Enabled = true,
                    Interval = TimeSpan.FromSeconds(30),
                    Timeout = TimeSpan.FromSeconds(5),
                    Policy = "ConsecutiveFailures",
                    Path = "/"
                }
            },
            HttpRequest = new ForwarderRequestConfig
            {
                ActivityTimeout = TimeSpan.FromSeconds(20),
                Version = Version.Parse("2.0"),
                VersionPolicy = HttpVersionPolicy.RequestVersionOrLower
            }
        },
        new ClusterConfig
        {
            ClusterId = "internal-cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "internal-dest", new DestinationConfig { Address = OmniTrxnUrl } }
            },
            HealthCheck = new HealthCheckConfig
            {
                Active = new ActiveHealthCheckConfig
                {
                    Enabled = true,
                    Interval = TimeSpan.FromSeconds(30),
                    Timeout = TimeSpan.FromSeconds(5),
                    Policy = "ConsecutiveFailures",
                    Path = "/health"
                }
            },
            HttpRequest = new ForwarderRequestConfig
            {
                ActivityTimeout = TimeSpan.FromSeconds(30),
                Version = Version.Parse("2.0"),
                VersionPolicy = HttpVersionPolicy.RequestVersionOrLower
            }
        }
    };
}


// ===================== YARP Reverse Proxy =====================
builder.Services.AddReverseProxy()
    .LoadFromMemory(GetRoutes(), GetClusters(ozowUrl, fnbUrl, omniTrxnUrl))
    .AddTransforms(transformBuilderContext =>
    {
        if (transformBuilderContext.Route.ClusterId == "soap-cluster")
        {
            transformBuilderContext.AddRequestTransform(async transformContext =>
            {
                transformContext.ProxyRequest.Headers.Remove("Authorization");
                var username = builder.Configuration["ExternalServices:FNBService:Username"];
                var password = builder.Configuration["ExternalServices:FNBService:Password"];
                var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                var authHeader = new AuthenticationHeaderValue("Basic", token);
                transformContext.ProxyRequest.Headers.Authorization = authHeader;
            });
        }

        if (transformBuilderContext.Route.ClusterId == "rest-cluster")
        {
            transformBuilderContext.AddRequestTransform(async transformContext =>
            {
                transformContext.ProxyRequest.Headers.Remove("Authorization");
                var username = builder.Configuration["ExternalServices:OzowService:Username"];
                var password = builder.Configuration["ExternalServices:OzowService:Password"];
                var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                transformContext.ProxyRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Basic", token);
            });
        }

        transformBuilderContext.AddRequestTransform(transformContext =>
        {
            var requestId = transformContext.HttpContext.TraceIdentifier;
            transformContext.ProxyRequest.Headers.Add("X-Request-ID", requestId);
            return ValueTask.CompletedTask;
        });
    });

// ===================== Authentication =====================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];   
        options.Audience = builder.Configuration["Jwt:Audience"]; 

        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = ClaimTypes.NameIdentifier,  
            RoleClaimType = ClaimTypes.Role,             
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

// ===================== Authorization =====================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Authenticated", policy =>
        policy.RequireAuthenticatedUser());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowScalar", policy =>
    {
        policy.WithOrigins("https://localhost:7293")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ===================== Rate Limiting =====================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("Global", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue<int>("RateLimiting:Global:PermitLimit"),
                Window = TimeSpan.Parse(builder.Configuration["RateLimiting:Global:Window"]),
                QueueLimit = builder.Configuration.GetValue<int>("RateLimiting:Global:QueueLimit")
            }));

    options.AddPolicy("External", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue<int>("RateLimiting:External:PermitLimit"),
                Window = TimeSpan.Parse(builder.Configuration["RateLimiting:External:Window"]),
                QueueLimit = builder.Configuration.GetValue<int>("RateLimiting:External:QueueLimit")
            }));
});

// ===================== Caching =====================
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromSeconds(60)));
    options.AddPolicy("ExternalRestCache", policy =>
        policy.Expire(TimeSpan.FromMinutes(5)));
});

// ===================== Health Checks =====================
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());

// ===================== OpenTelemetry =====================
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("ApiGateway"))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter("Yarp.ReverseProxy")
        .AddConsoleExporter())
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter()
        .AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri("http://localhost:4317");
        }));

builder.Services.AddMemoryCache();

var app = builder.Build();

// ===================== Middleware Pipeline =====================
app.UseSerilogRequestLogging(); 

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var requestId = context.TraceIdentifier;
        var response = new
        {
            status = context.Response.StatusCode,
            requestId = requestId,
            message = "An unexpected error occurred.",
            detail = error?.Error?.Message
        };
        await context.Response.WriteAsJsonAsync(response);
    });
});

app.UseRouting();

app.UseCors("AllowScalar");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter(); // global rate limiter

app.UseOutputCache(); // response caching

app.MapHealthChecks("/health");

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/token")
    {
        var apiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault();
        if (apiKey != builder.Configuration["ApiKey"])
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
            return;
        }
    }

    if (context.Request.Path.StartsWithSegments("/api"))
    {
        if (HttpMethods.IsPost(context.Request.Method) ||
            HttpMethods.IsPut(context.Request.Method) ||
            HttpMethods.IsPatch(context.Request.Method))
        {
            var idempotencyKey = context.Request.Headers["Idempotency-Key"].FirstOrDefault();
            if (string.IsNullOrEmpty(idempotencyKey))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = "Idempotency-Key header is required for this method." });
                return;
            }

            var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
            if (cache.TryGetValue(idempotencyKey, out _))
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(new { error = "Duplicate request." });
                return;
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(24));
            cache.Set(idempotencyKey, true, cacheEntryOptions);
        }
    }

    await next();
});

app.MapPost("/token", async (TokenRequest request, IConfiguration config, IHttpClientFactory httpClientFactory) =>
{
    var httpClient = httpClientFactory.CreateClient();
    var tokenEndpoint = config["Jwt:TokenEndpoint"];
    var audience = config["Jwt:Audience"];

    var payload = new Dictionary<string, string>
    {
        ["client_id"] = request.ClientId,
        ["client_secret"] = request.ClientSecret,
        ["audience"] = audience,
        ["grant_type"] = "client_credentials"
    };

    using var content = new FormUrlEncodedContent(payload);
    var response = await httpClient.PostAsync(tokenEndpoint, content);

    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem(
            statusCode: (int)response.StatusCode,
            title: "Token request failed",
            detail: await response.Content.ReadAsStringAsync()
        );
    }

    var tokenJson = await response.Content.ReadAsStringAsync();
    return Results.Text(tokenJson, "application/json");
});

app.MapReverseProxy();


app.Run();

