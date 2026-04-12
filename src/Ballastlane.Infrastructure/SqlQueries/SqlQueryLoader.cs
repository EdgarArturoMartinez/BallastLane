using System.IO;

namespace Ballastlane.Infrastructure.SqlQueries;

public static class SqlQueryLoader
{
    private static string BasePath => Path.Combine(AppContext.BaseDirectory, "SqlQueries");

    public static string Get(string name)
    {
        var file = Path.Combine(BasePath, name + ".sql");
        if (File.Exists(file))
            return File.ReadAllText(file);

        // Fallback to development path (when running from source)
        var devPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "src", "Ballastlane.Infrastructure", "SqlQueries", name + ".sql");
        if (File.Exists(devPath))
            return File.ReadAllText(devPath);

        throw new FileNotFoundException($"SQL query not found: {name}", file);
    }
}
