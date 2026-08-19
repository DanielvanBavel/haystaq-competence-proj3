using System.Text.Json;
using System.Text.Json.Serialization;
using BezorgBaas.Application;
using BezorgBaas.Domain;
using BezorgBaas.Domain.Common;
using BezorgBaas.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
string port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
string database = Environment.GetEnvironmentVariable("DB_NAME") ?? "bezorgbaas";
string user = Environment.GetEnvironmentVariable("DB_USER") ?? "bezorgbaas";
string password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "bezorgbaas";
string connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password}";

builder.Services.AddDbContext<BezorgBaasDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPromoCodeRepository, PromoCodeRepository>();
builder.Services.AddScoped<CatalogQueryService>();
builder.Services.AddScoped<OrderingService>();
builder.Services.AddSingleton(new DatabaseAdmin(connectionString));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

WebApplication app = builder.Build();

// Wachten tot Postgres klaar is; de container start soms sneller dan de database.
await WaitForDatabaseAsync(connectionString, app.Logger);

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (DomainException exception)
    {
        context.Response.StatusCode = exception.Kind switch
        {
            DomainErrorKind.Invalid => StatusCodes.Status400BadRequest,
            DomainErrorKind.Conflict => StatusCodes.Status409Conflict,
            DomainErrorKind.NotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest
        };
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { code = exception.Code, message = exception.Message });
    }
    catch (Exception exception)
    {
        app.Logger.LogError(exception, "onverwachte fout op {Path}", context.Request.Path);
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { code = "internal_error", message = "Er ging iets mis." });
    }
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

app.Run();

static async Task WaitForDatabaseAsync(string connectionString, ILogger logger)
{
    for (int attempt = 1; attempt <= 30; attempt++)
    {
        try
        {
            await using NpgsqlConnection connection = new(connectionString);
            await connection.OpenAsync();
            return;
        }
        catch (Exception)
        {
            logger.LogInformation("database nog niet bereikbaar, poging {Attempt}", attempt);
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
    throw new InvalidOperationException("database niet bereikbaar");
}

/// <summary>Voert het seed-script opnieuw uit. Alleen bedoeld voor testomgevingen.</summary>
public class DatabaseAdmin
{
    private readonly string _connectionString;

    public DatabaseAdmin(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task ResetAsync(string seedFilePath)
    {
        await using NpgsqlConnection connection = new(_connectionString);
        await connection.OpenAsync();

        await using (NpgsqlCommand truncate = new(
                         "truncate order_status_change, order_line, customer_order, " +
                         "menu_item_option, menu_item, restaurant, promo_code restart identity cascade",
                         connection))
        {
            await truncate.ExecuteNonQueryAsync();
        }

        if (File.Exists(seedFilePath))
        {
            string seed = await File.ReadAllTextAsync(seedFilePath);
            await using NpgsqlCommand command = new(seed, connection);
            await command.ExecuteNonQueryAsync();
        }
    }
}
