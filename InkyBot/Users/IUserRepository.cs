using System.Net.NetworkInformation;

namespace InkyBot.Users;

public interface IUserRepository
{
    Task CreateUser(long telegramId, PhysicalAddress address);

    Task<User?> GetUserByPhysicalAddress(PhysicalAddress address);

    Task<User?> GetCurrentUser();

    Task UpdateUserDisplayAddress(User user, PhysicalAddress address);

    Task DeleteUser(Guid userId);
}