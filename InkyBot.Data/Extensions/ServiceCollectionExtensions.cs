using System.Data;
using Dapper;
using InkyBot.Data.Services;
using InkyBot.Data.TypeHandlers;
using InkyBot.Data.Users;
using InkyBot.Users;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace InkyBot.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInkyBotData(this IServiceCollection services, string connectionString)
    {
        SqlMapper.AddTypeHandler(new GuidHandler());
        SqlMapper.AddTypeHandler(new PhysicalAddressHandler());

        return services
            .AddHostedService(_ => new SqliteMigrationService(connectionString))
            .AddScoped<IDbConnection>(_ => new SqliteConnection(connectionString))
            .AddTransient<IUserRepository, UserRepository>();
    }
}