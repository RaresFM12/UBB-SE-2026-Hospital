using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.IO;
using System.Text.Json;

namespace Hospital.Data;

public class HospitalDbContextFactory : IDesignTimeDbContextFactory<HospitalDbContext>
{
    private const string FallbackConnectionString = "Server=localhost\\SQLEXPRESS;Database=HospitalDatabase;Trusted_Connection=True;TrustServerCertificate=True;";

    public HospitalDbContext CreateDbContext(string[] arguments)
    {
        string connectionString = null;
        try
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Hospital.API");
            if (!Directory.Exists(basePath))
            {
                basePath = Directory.GetCurrentDirectory();
            }
            var configPath = Path.Combine(basePath, "appsettings.json");
            if (File.Exists(configPath))
            {
                var jsonContent = File.ReadAllText(configPath);
                var jsonDocumentOptions = new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip
                };
                using (var jsonDocument = JsonDocument.Parse(jsonContent, jsonDocumentOptions))
                {
                    if (jsonDocument.RootElement.TryGetProperty("ConnectionStrings", out var connectionStringsSection) &&
                        connectionStringsSection.TryGetProperty("DefaultConnection", out var defaultConnectionProperty))
                    {
                        connectionString = defaultConnectionProperty.GetString();
                    }
                }
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Error reading connection string from appsettings: {exception.Message}");
        }

        connectionString ??= FallbackConnectionString;

        var dbContextOptionsBuilder = new DbContextOptionsBuilder<HospitalDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new HospitalDbContext(dbContextOptionsBuilder);
    }
}
