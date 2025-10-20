using BackendApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json.Serialization;   // <-- needed for ReferenceHandler / JsonIgnoreCondition

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------- DB ----------
var conn = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(conn))
{
    throw new InvalidOperationException(
        "Missing connection string 'ConnectionStrings__Default' in environment variables.");
}

// Avoid AutoDetect (which opens a DB connection at startup)
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(conn, ServerVersion.Parse("8.0.36-mysql"));
});

var app = builder.Build();

// ---------- Middleware ----------
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// TEMP: surface exceptions while debugging deploy
app.Use(async (ctx, next) =>
{
    try { await next(); }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        ctx.Response.StatusCode = 500;
        await ctx.Response.WriteAsync(ex.Message);
    }
});

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
