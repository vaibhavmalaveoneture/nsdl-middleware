using Serilog;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                      ?? throw new InvalidOperationException("CORS:AllowedOrigins configuration section is missing or empty.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


// ✨ Configure Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Replace default logging with Serilog
builder.Host.UseSerilog();

// Configure Kestrel (if you're using it)
builder.WebHost.ConfigureKestrel(options =>
{
    builder.Configuration.GetSection("Kestrel").Bind(options);
});

// Add Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.UseCors("AllowSpecificOrigins");
app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
    context.Response.Headers.TryAdd("Strict-Transport-Security", "max-age=63072000; includeSubDomains; preload");
    context.Response.Headers.TryAdd("Content-Security-Policy", "default-src 'self'; script-src 'self'; object-src 'none'; frame-ancestors 'none';");
    context.Response.Headers.TryAdd("Permissions-Policy", "geolocation=(), camera=(), microphone=()");
    
    await next();
});
app.MapReverseProxy();

app.Run();
