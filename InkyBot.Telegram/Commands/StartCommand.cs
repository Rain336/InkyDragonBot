using System.Net.NetworkInformation;
using InkyBot.Conversation;
using InkyBot.EInk;
using InkyBot.Telegram.Extensions;
using InkyBot.Users;
using Telegram.Bot.Types.ReplyMarkups;

namespace InkyBot.Telegram.Commands;

[RegisterCommand("start", Description = "Starts the bot and registers your display")]
public class StartCommand(
    IUserRepository userRepository,
    ITagDatabaseService tagDatabaseService,
    IBadgeGenerator badgeGenerator
) : IConversation
{
    public async Task ExecuteAsync(IConversationContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(context.Args))
        {
            await context.ReplyAsync(
                "To start Inky Dragon Bot, please call /start with the display's mac address",
                cancellationToken
            );
            return;
        }

        if (!PhysicalAddress.TryParse(context.Args, out var address))
        {
            await context.ReplyAsync(
                "Invalid mac address, please call /start again with a valid mac address",
                cancellationToken
            );
            return;
        }

        var current = await userRepository.GetCurrentUser();

        if (await userRepository.GetUserByPhysicalAddress(address) is { } existing)
        {
            if (existing.Id == current?.Id)
            {
                await context.ReplyAsync(
                    "You are already registered as the wearer of that display",
                    cancellationToken
                );
            }
            else
            {
                await context.ReplyAsync(
                    "That display is already registered or does not exist",
                    cancellationToken
                );
            }

            return;
        }

        if (!tagDatabaseService.TryGetTag(address, out var tag) || tag.Address is null)
        {
            await context.ReplyAsync(
                "That display is already registered or does not exist",
                cancellationToken
            );
            return;
        }

        if (current is not null)
        {
            if (current.DisplayAddress is not null)
            {
                var reply = await context.ReplyWithOptionsAsync(
                    @"You are already registered as the wearer of a display
Do you want to keep it or change to the new display?

Old Display Address: " + current.DisplayAddress,
                    new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithCallbackData("Overwrite", "confirm"),
                        InlineKeyboardButton.WithCallbackData("Keep", "deny")
                    ),
                    cancellationToken
                );
                if (reply.Data != "confirm")
                {
                    return;
                }
            }

            await userRepository.UpdateUserDisplayAddress(current, address);
        }
        else if (context.Sender is not null)
        {
            await userRepository.CreateUser(context.Sender.Id, address);
            await badgeGenerator.GenerateBadgesAsync(tag, context.Sender, null, cancellationToken);
        }

        await context.ReplyAsync(
            "The display was assigned to your account",
            cancellationToken
        );
    }
}