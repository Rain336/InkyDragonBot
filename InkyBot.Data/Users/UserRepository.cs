using System.Data;
using System.Net.NetworkInformation;
using Dapper;
using InkyBot.Services;
using InkyBot.Users;

namespace InkyBot.Data.Users;

internal sealed class UserRepository(
    IDbConnection connection,
    IConversationContextAccessor conversationAccessor
) : IUserRepository
{
    public Task CreateUser(long telegramId, PhysicalAddress address)
    {
        return connection.ExecuteAsync(
            "INSERT INTO Users (Id, TelegramId, DisplayAddress) VALUES (@Id, @TelegramId, @DisplayAddress)",
            new { Id = Guid.NewGuid(), TelegramId = telegramId, DisplayAddress = address }
        );
    }

    public Task<User?> GetUserByPhysicalAddress(PhysicalAddress address)
    {
        return connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE DisplayAddress = @Address",
            new { Address = address }
        );
    }

    public Task<User?> GetCurrentUser()
    {
        if (conversationAccessor.Context is { Sender: not null } conversationContext)
        {
            return connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE TelegramId = @Id",
                new { Id = conversationContext.Sender.Id }
            );
        }

        return Task.FromResult<User?>(null);
    }

    public Task UpdateUserDisplayAddress(User user, PhysicalAddress address)
    {
        return connection.ExecuteAsync(
            "UPDATE Users SET DisplayAddress = @Address WHERE Id = @Id",
            new { Id = user.Id, Address = address }
        );
    }

    public Task DeleteUser(Guid userId)
    {
        return connection.ExecuteAsync(
            "DELETE FROM Users WHERE Id = @Id",
            new { Id = userId }
        );
    }
}