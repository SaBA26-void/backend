using Microsoft.EntityFrameworkCore;
using OnlineShop.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Railway injects PORT; bind explicitly so the public URL reaches the app.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = ResolveConnectionString(builder.Configuration);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin => IsAllowedOrigin(origin, allowedOrigins))
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

// Railway terminates TLS at the proxy; do not force HTTPS redirect inside the container.
if (!app.Environment.IsProduction() && !IsRunningOnRailway())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    status = "ok",
    message = "Online Shop API is running",
    swagger = "/swagger",
    categories = "/api/categories",
    products = "/api/products"
}));

app.MapControllers();

app.Run();

static bool IsAllowedOrigin(string origin, string[] configuredOrigins)
{
    if (configuredOrigins.Any(o =>
            string.Equals(o, origin, StringComparison.OrdinalIgnoreCase)))
    {
        return true;
    }

    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
    {
        return false;
    }

    // Local Next.js
    if (uri.Host is "localhost" or "127.0.0.1")
    {
        return true;
    }

    // Vercel previews + production
    if (uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    return false;
}

static bool IsRunningOnRailway() =>
    !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT"))
    || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RAILWAY_PUBLIC_DOMAIN"));

static string ResolveConnectionString(IConfiguration configuration)
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        return ConvertDatabaseUrlToNpgsql(databaseUrl);
    }

    return configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("No database connection string configured.");
}

static string ConvertDatabaseUrlToNpgsql(string databaseUrl)
{
    // Railway: postgresql://user:pass@host:port/railway
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    var username = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
    var database = uri.AbsolutePath.Trim('/');

    return $"Host={uri.Host};Port={uri.Port};Database={database};Username={username};Password={password};SSL Mode=Prefer;Trust Server Certificate=true";
}
