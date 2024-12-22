using InkyBot.Conversation;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace InkyBot.Telegram.Extensions;

public static class ConversationContextExtensions
{
    public static Task ReplyAsync(
        this IConversationContext context,
        string text,
        CancellationToken cancellationToken = default)
    {
        return context.BotClient.SendMessage(
            context.InitialMessage.Chat.Id,
            text,
            ParseMode.MarkdownV2,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<CallbackQuery> ReplyWithOptionsAsync(
        this IConversationContext context,
        string text,
        InlineKeyboardMarkup replyMarkup,
        CancellationToken cancellationToken = default)
    {
        var message = await context.BotClient.SendMessage(
            context.Chat.Id,
            text,
            ParseMode.MarkdownV2,
            replyMarkup: replyMarkup,
            cancellationToken: cancellationToken
        );

        var result = await context.WaitForCallbackQueryAsync(cancellationToken);
        await context.BotClient.EditMessageReplyMarkup(
            context.Chat.Id,
            message.Id,
            InlineKeyboardMarkup.Empty(),
            null,
            cancellationToken
        );

        return result;
    }
}