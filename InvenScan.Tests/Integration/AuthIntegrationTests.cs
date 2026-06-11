using InvenScan.Database;
using InvenScan.Entity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace InvenScan.Tests.Integration;

public class InvenScanWebFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("IntegrationTestDb"));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            SeedTestData(db);
        });

        builder.UseEnvironment("Test");
    }

    private static void SeedTestData(AppDbContext db)
    {
        if (db.Users.Any()) return;

        db.Users.Add(new User
        {
            UserId = "admin",
            FullName = "Administrator",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "ADMIN",
            IsActive = true
        });
        db.Users.Add(new User
        {
            UserId = "operator1",
            FullName = "Test Operator",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("op123"),
            Role = "OPERATOR",
            IsActive = true
        });

        db.Locations.Add(new Location { LocationCode = "LOC-001", LocationName = "Warehouse A", IsDelete = false });
        db.Items.Add(new Item { ItemCode = "ITM-001", ItemName = "Test Item", Unit = "PCS", MinStock = 10, IsDelete = false });

        db.AppSettings.Add(new AppSetting { Key = "ActivityLog.AutoDeleteDays", Value = "90", UpdatedAt = DateTime.UtcNow });

        db.SaveChanges();
    }
}

public class AuthIntegrationTests : IClassFixture<InvenScanWebFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(InvenScanWebFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsJwtToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { userId = "admin", password = "admin123" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        var token = root.GetProperty("data").GetProperty("token").GetString();
        Assert.False(string.IsNullOrEmpty(token));
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { userId = "admin", password = "wrongpassword" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_NonExistentUser_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { userId = "nobody", password = "any" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Ping_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/ping");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
