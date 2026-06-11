using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace InvenScan.Controllers.Api;

[Route("api/config")]
[ApiController]
public class ConfigApiController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public ConfigApiController(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }

    [HttpPost("database")]
    public async Task<IActionResult> SaveDatabase([FromBody] DbConfigRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Server) || string.IsNullOrWhiteSpace(request.Database))
            return BadRequest(new { success = false, message = "Server and database name are required." });

        var builder = new SqlConnectionStringBuilder
        {
            DataSource = string.IsNullOrWhiteSpace(request.Port) || request.Port == "1433"
                ? request.Server
                : $"{request.Server},{request.Port}",
            InitialCatalog = request.Database,
            UserID = request.Username ?? string.Empty,
            Password = request.Password ?? string.Empty,
            TrustServerCertificate = true,
            ConnectTimeout = 10
        };

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            builder.Remove("User ID");
            builder.Remove("Password");
            builder.IntegratedSecurity = true;
        }

        var connStr = builder.ConnectionString;

        try
        {
            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();
            await conn.CloseAsync();
        }
        catch (Exception ex)
        {
            return Ok(new { success = false, message = $"Connection failed: {ex.Message}" });
        }

        var appSettingsPath = Path.Combine(_env.ContentRootPath, "appsettings.json");
        var json = await System.IO.File.ReadAllTextAsync(appSettingsPath);
        var doc = JsonDocument.Parse(json);
        using var memStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memStream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.Name == "ConnectionStrings")
                {
                    writer.WritePropertyName("ConnectionStrings");
                    writer.WriteStartObject();
                    writer.WriteString("DefaultConnection", connStr);
                    writer.WriteEndObject();
                }
                else
                {
                    prop.WriteTo(writer);
                }
            }
            writer.WriteEndObject();
        }

        await System.IO.File.WriteAllBytesAsync(appSettingsPath, memStream.ToArray());

        return Ok(new { success = true, message = "Database configuration saved. Please restart the application." });
    }

    [HttpPost("database/test")]
    public async Task<IActionResult> TestDatabase([FromBody] DbConfigRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Server) || string.IsNullOrWhiteSpace(request.Database))
            return BadRequest(new { success = false, message = "Server and database are required." });

        var builder = new SqlConnectionStringBuilder
        {
            DataSource = string.IsNullOrWhiteSpace(request.Port) || request.Port == "1433"
                ? request.Server
                : $"{request.Server},{request.Port}",
            InitialCatalog = request.Database,
            UserID = request.Username ?? string.Empty,
            Password = request.Password ?? string.Empty,
            TrustServerCertificate = true,
            ConnectTimeout = 5
        };

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            builder.Remove("User ID");
            builder.Remove("Password");
            builder.IntegratedSecurity = true;
        }

        try
        {
            await using var conn = new SqlConnection(builder.ConnectionString);
            await conn.OpenAsync();
            await conn.CloseAsync();
            return Ok(new { success = true, message = "Connection successful!" });
        }
        catch (Exception ex)
        {
            return Ok(new { success = false, message = $"Connection failed: {ex.Message}" });
        }
    }
}

public class DbConfigRequest
{
    public string Server { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Port { get; set; }
}
