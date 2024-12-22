using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting;

namespace InkyBot.Data.Services;

internal sealed class SqliteMigrationService(string connectionString) : IHostedService
{
    private const string MigrationScript = @"
CREATE TABLE IF NOT EXISTS Users (
    Id TEXT PRIMARY KEY NOT NULL,
    TelegramId INTEGER NOT NULL,
    DisplayAddress BLOB
)";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.ExecuteAsync(MigrationScript);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}