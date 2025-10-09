using API.Middleware;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Activities.Queries;
using MediatR;
using Application.Core;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add OpenApi
builder.Services.AddOpenApi("internalApi");

// Add DbInitializer
builder.Services.AddScoped<DbInitializer>();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});



// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure HTTPS with enhanced security
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    options.HttpsPort = 5001;
});

// Add HSTS (HTTP Strict Transport Security) with enhanced settings
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
    // Add specific domains if needed
    // options.ExcludedHosts.Add("localhost");
});

// Add CORS for secure cross-origin requests
builder.Services.AddCors(options =>
{
    options.AddPolicy("SecurePolicy", policy =>
    {
        policy.WithOrigins("https://localhost:3000", "https://localhost:3001") // Add your frontend URLs
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

// Add MediatR for CQRS pattern (Command Query Responsibility Segregation)
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<GetActivityList.Handler>()
    .RegisterServicesFromAssemblyContaining<GetActivityDetails.Handler>()
    .RegisterServicesFromAssemblyContaining<CreateActivity.Handler>()
);

// Register AutoMapper and explicitly add the MappingProfiles to avoid overload ambiguity
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfiles>());

// Security headers middleware will be added to pipeline directly

var app = builder.Build();

var rateLimitStore = new System.Collections.Concurrent.ConcurrentDictionary<string, (int Count, DateTime WindowStart)>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Use HSTS in production
    app.UseHsts();
}

// HTTPS redirection should be early in the pipeline
app.UseHttpsRedirection();

// Add CORS
app.UseCors(x => x
    .WithOrigins("https://localhost:3000", "http://localhost:3000") // Add your frontend URLs
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
    .SetIsOriginAllowedToAllowWildcardSubdomains()
);

// Add custom security headers middleware
// Add rate limiting (if needed)
app.Use(async (context, next) =>
{
    // Basic rate limiting - you can enhance this with a proper rate limiting library
    var clientIp = context.Connection.RemoteIpAddress?.ToString();
    // Add your rate limiting logic here
    // Simple in-memory rate limiting per client IP using a shared ConcurrentDictionary
    var maxRequests = 60; // allowed requests
    var window = TimeSpan.FromMinutes(1); // time window

    if (clientIp == null)
    {
        await next();
        return;
    }

    var store = rateLimitStore;
    var now = DateTime.UtcNow;

    var entry = store.AddOrUpdate(clientIp,
        _ => (1, now),
        (_, old) =>
        {
            // reset window when expired
            if (now - old.WindowStart > window) return (1, now);
            return (old.Count + 1, old.WindowStart);
        });

    // If over limit, short-circuit with 429 and Retry-After header
    if (entry.Count > maxRequests)
    {
        var retryAfterSeconds = (int)Math.Ceiling((entry.WindowStart + window - now).TotalSeconds);
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.Headers["Retry-After"] = Math.Max(1, retryAfterSeconds).ToString();
        await context.Response.WriteAsync("Too many requests. Please try again later.");
        return;
    }

    // proceed to next middleware
    await next();
});

app.UseAuthorization();

app.MapControllers();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        await DbInitializer.Initialize(context);
        logger.LogInformation("Database seeded successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}



app.Run();
