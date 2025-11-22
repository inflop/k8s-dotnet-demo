using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Kube.Database.Api;

public class DatabaseInitializer
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly string _sqlPath;

    public DatabaseInitializer(string connectionString, ILogger<DatabaseInitializer> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
        _sqlPath = Path.Combine(AppContext.BaseDirectory, "sql");
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing database...");

        await CreateDatabaseAsync();
        await CreateTablesAsync();

        _logger.LogInformation("Database initialization completed.");
    }

    private async Task CreateDatabaseAsync()
    {
        var builder = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = "master"
        };

        var script = await File.ReadAllTextAsync(Path.Combine(_sqlPath, "01_create_database.sql"));
        using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        using var command = new SqlCommand(script, connection);
        await command.ExecuteNonQueryAsync();
    }

    private async Task CreateTablesAsync()
    {
        var script = await File.ReadAllTextAsync(Path.Combine(_sqlPath, "02_create_tables.sql"));
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var command = new SqlCommand(script, connection);
        await command.ExecuteNonQueryAsync();
    }
}
