namespace InkyBot.Telegram.Internal;

internal sealed record CommandRegistration(string Command, Type ConversationType, string Description);