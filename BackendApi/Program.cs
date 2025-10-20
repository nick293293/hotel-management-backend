// Program.cs
using BackendApi.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
// using YourNamespace.Data; // <-- replace with the namespace where your DbContext lives
// using YourNamespace;       // <-- if needed

var builder = WebApplication.CreateBuilder(args);

// ---- Connection string ----
// On Render, set: ConnectionStrings__DefaultConnection = "Server=<host>;Port=3306;Database=<db>;User Id=<user>;Password=<pass>;Ssl Mode=Required;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=10;"
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found. " +
        "Set env var ConnectionStrings__DefaultConnection in Render.");
}

// ---- Server version ----
// Optionally override with env var MYSQL_SERVER_VERSION (e.g., 8.0.36).
// Default to 8.0.36 (works for most MySQL 8 deployments).
var serverVersionEnv = builder.Configuration["MYSQL_SERVER_VERSION"];
Version version;
if (!string.IsNullOrWhiteSpace(serverVersionEnv) && Version.TryParse(serverVersionEnv, out var parsed))
{
    version = parsed;
}
else
{
    version = new Version(8, 0, 36);
}
var serverVersion = new MySqlServerVersion(version);

// ---- Services ----
builder.Services.AddControllers();

// IMPORTANT: replace AppDbContext with your actual DbContext type.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString, serverVersion, mySqlOptions =>
    {
        // Enable transient failure retries (useful on cold starts / brief network blips)
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    });

    // Optional: uncomment to see EF-generated SQL in logs
    // options.EnableSensitiveDataLogging();
    // options.EnableDetailedErrors();
});

// CORS (optional - adjust origins if you have a frontend on a different domain)
// builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
//     .AllowAnyHeader()
//     .AllowAnyMethod()
//     .WithOrigins("https://your-frontend.onrender.com")));

// Swagger (optional)
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

var app = builder.Build();

// Global error handler to ensure 500s are logged with stack traces
app.Use(async (ctx, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        // log to Render logs
        Console.Error.WriteLine($"[UNHANDLED] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await ctx.Response.WriteAsJsonAsync(new { error = "Internal Server Error" });
    }
});

// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// app.UseHttpsRedirection(); // only if you terminate TLS elsewhere; on Render HTTP->service is fine
// app.UseCors();             // if you enabled CORS above
app.UseAuthorization();

// Health check that does NOT touch the DB (useful for verifying the app is booting)
app.MapGet("/healthz", () => Results.Ok("ok"));

// Your API/controllers
app.MapControllers();

app.Run();

// ---- Notes ----
// 1) Ensure your Render env var is exactly: ConnectionStrings__DefaultConnection
// 2) Your MySQL provider likely requires TLS. Include 'Ssl Mode=Required' in the connection string.
// 3) If your DB is IP-allowlisted, allow Render's egress IPs.
// 4) If you still see 'Unable to connect to any of the specified MySQL hosts',
//    double-check the host, port, and that the DB is reachable from the internet or Render's network.
