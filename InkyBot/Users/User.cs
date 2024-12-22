using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;

namespace InkyBot.Users;

public class User
{
    public Guid Id { get; set; }
    public long TelegramId { get; set; }
    public PhysicalAddress? DisplayAddress { get; set; }
}