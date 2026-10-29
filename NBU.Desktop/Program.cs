using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NBU.Data;

namespace NBU.Desktop;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        var connectionString = GetConnectionString();
        var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        Application.Run(new Form1(dbOptions));
    }

    private static string GetConnectionString()
    {
        var environmentValue = Environment.GetEnvironmentVariable("NBU_SQL_CONNECTION");
        if (!string.IsNullOrWhiteSpace(environmentValue)) return environmentValue;

        var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (File.Exists(filePath))
        {
            using var settings = JsonDocument.Parse(File.ReadAllText(filePath));
            var value = settings.RootElement.GetProperty("ConnectionStrings").GetProperty("NBU").GetString();
            if (!string.IsNullOrWhiteSpace(value)) return value;
        }

        return "Server=.\\SQLEXPRESS22;Database=NBU;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
    }
}
