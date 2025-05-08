
using NSDL.Middleware.Helpers;
using NSDL.Middleware.Interfaces;
using NSDL.Middleware.Models;
using Serilog;
using System.Text;
using System.Text.Json;

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

builder.Services.AddScoped<IEmailHelper, EmailHelper>();

var backendBaseUrl = builder.Configuration["ReverseProxy:Clusters:backend:Destinations:backend1:Address"]
    ?? throw new InvalidOperationException("Backend base URL is not configured.");

builder.Services.AddHttpClient("backend", client =>
{
    client.BaseAddress = new Uri(backendBaseUrl);
});


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

app.MapPost("/api/auth/login", async (
    HttpContext context,
    IHttpClientFactory httpClientFactory,
    IEmailHelper emailHelper,
    ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("Login");

    var httpClient = httpClientFactory.CreateClient("backend");

    var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();

    var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

    var backendResponse = await httpClient.PostAsync("api/auth/login", content);

    if (!backendResponse.IsSuccessStatusCode)
    {
        var backendContent = await backendResponse.Content.ReadAsStringAsync();
        return Results.Content(
         backendContent,
         contentType: backendResponse.Content.Headers.ContentType?.ToString() ?? "application/json",
         statusCode: (int)backendResponse.StatusCode
     );
    }


    var json = await backendResponse.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<MiddlewareResponse>(json, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    if (result?.Data is JsonElement dataElement && dataElement.ValueKind == JsonValueKind.Object)
    {
        var email = dataElement.GetProperty("email").GetString();
        var otp = dataElement.GetProperty("otp").GetString();
        var message = dataElement.GetProperty("message").GetString();

        if (!string.IsNullOrWhiteSpace(email) &&
            !string.IsNullOrWhiteSpace(otp) &&
            !string.IsNullOrWhiteSpace(message))
        {
            await emailHelper.SendOtpEmailAsync(email, otp, message);
            result.Data = null;
        }
    }

    return Results.Ok(result);
});
app.MapPost("/api/auth/send-otp", async (
    HttpContext context,
    IHttpClientFactory httpClientFactory,
    IEmailHelper emailHelper,
    ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("send-otp");

    var httpClient = httpClientFactory.CreateClient("backend");

    var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();

    var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

    var backendResponse = await httpClient.PostAsync("api/auth/send-otp", content);

    if (!backendResponse.IsSuccessStatusCode)
    {
        var backendContent = await backendResponse.Content.ReadAsStringAsync();
        return Results.Content(
         backendContent,
         contentType: backendResponse.Content.Headers.ContentType?.ToString() ?? "application/json",
         statusCode: (int)backendResponse.StatusCode
     );
    }


    var json = await backendResponse.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<MiddlewareResponse>(json, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    if (result?.Data is JsonElement dataElement && dataElement.ValueKind == JsonValueKind.Object)
    {
        var email = dataElement.GetProperty("email").GetString();
        var otp = dataElement.GetProperty("otp").GetString();
        var message = dataElement.GetProperty("message").GetString();

        if (!string.IsNullOrWhiteSpace(email) &&
            !string.IsNullOrWhiteSpace(otp) &&
            !string.IsNullOrWhiteSpace(message))
        {
            await emailHelper.SendOtpEmailAsync(email, otp, message);
            result.Data = null;
        }
    }

    return Results.Ok(result);
});
app.MapPost("/api/auth/forgot-password/send-otp", async (
    HttpContext context,
    IHttpClientFactory httpClientFactory,
    IEmailHelper emailHelper,
    ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("forgot-password/send-otp");

    var httpClient = httpClientFactory.CreateClient("backend");

    var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();

    var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

    var backendResponse = await httpClient.PostAsync("api/auth/forgot-password/send-otp", content);

    if (!backendResponse.IsSuccessStatusCode)
    {
        var backendContent = await backendResponse.Content.ReadAsStringAsync();
        return Results.Content(
         backendContent,
         contentType: backendResponse.Content.Headers.ContentType?.ToString() ?? "application/json",
         statusCode: (int)backendResponse.StatusCode
     );
    }

    var json = await backendResponse.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<MiddlewareResponse>(json, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    if (result?.Data is JsonElement dataElement && dataElement.ValueKind == JsonValueKind.Object)
    {
        var email = dataElement.GetProperty("email").GetString();
        var otp = dataElement.GetProperty("otp").GetString();
        var message = dataElement.GetProperty("message").GetString();

        if (!string.IsNullOrWhiteSpace(email) &&
            !string.IsNullOrWhiteSpace(otp) &&
            !string.IsNullOrWhiteSpace(message))
        {
            await emailHelper.SendOtpEmailAsync(email, otp, message);
            result.Data = null;
        }
    }

    return Results.Ok(result);
});
app.MapPost("/api/auth/resend-otp", async (
    HttpContext context,
    IHttpClientFactory httpClientFactory,
    IEmailHelper emailHelper,
    ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("resend-otp");

    var httpClient = httpClientFactory.CreateClient("backend");

    var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();

    var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

    var backendResponse = await httpClient.PostAsync("api/auth/resend-otp", content);

    if (!backendResponse.IsSuccessStatusCode)
    {
        var backendContent = await backendResponse.Content.ReadAsStringAsync();
        return Results.Content(
         backendContent,
         contentType: backendResponse.Content.Headers.ContentType?.ToString() ?? "application/json",
         statusCode: (int)backendResponse.StatusCode
     );
    }

    var json = await backendResponse.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<MiddlewareResponse>(json, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    if (result?.Data is JsonElement dataElement && dataElement.ValueKind == JsonValueKind.Object)
    {
        var email = dataElement.GetProperty("email").GetString();
        var otp = dataElement.GetProperty("otp").GetString();
        var message = dataElement.GetProperty("message").GetString();

        if (!string.IsNullOrWhiteSpace(email) &&
            !string.IsNullOrWhiteSpace(otp) &&
            !string.IsNullOrWhiteSpace(message))
        {
            await emailHelper.SendOtpEmailAsync(email, otp, message);
            result.Data = null;
        }
    }

    return Results.Ok(result);
});
app.MapReverseProxy();

app.Run();
