using API.Middleware;
using Persistence;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddCors();

// Security headers middleware will be added to pipeline directly

var app = builder.Build();

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
app.UseMiddleware<SecurityHeadersMiddleware>();

// Add rate limiting (if needed)
app.Use(async (context, next) =>
{
    // Basic rate limiting - you can enhance this with a proper rate limiting library
    var clientIp = context.Connection.RemoteIpAddress?.ToString();
    // Add your rate limiting logic here
    
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
