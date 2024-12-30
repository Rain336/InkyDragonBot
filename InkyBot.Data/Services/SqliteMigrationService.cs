using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting;

namespace InkyBot.Data.Services;

internal sealed class SqliteMigrationService(string connectionString) : IHostedService
{
    private const string CreateMigrationsTable = @"
CREATE TABLE IF NOT EXISTS Users (
    Id TEXT PRIMARY KEY NOT NULL,
    TelegramId INTEGER NOT NULL,
    DisplayAddress BLOB
);
CREATE TABLE Migrations (
    Version INTEGER PRIMARY KEY NOT NULL,
);
INSERT INTO Migrations VALUES (0)";

    private const string MigrateV0ToV1 = @"
ALTER TABLE Users ADD COLUMN ProfilePicture BLOB;
ALTER TABLE Users ADD COLUMN ContentImage BLOB;
INSERT INTO Migrations VALUES (1)
";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        var table = await connection.QueryFirstOrDefaultAsync<string>(
            "SELECT name FROM sqlite_master WHERE type='table' AND name='Migrations'",
            cancellationToken
        );

        if (string.IsNullOrWhiteSpace(table))
        {
            await connection.ExecuteAsync(CreateMigrationsTable, cancellationToken);
        }

        var version = await connection.QuerySingleAsync<int>(
            "SELECT MAX(Version) FROM Migrations",
            cancellationToken
        );

        switch (version)
        {
            case 0:
                await connection.ExecuteAsync(MigrateV0ToV1, cancellationToken);
                break;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}