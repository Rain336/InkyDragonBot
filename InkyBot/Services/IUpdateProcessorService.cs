using Telegram.Bot.Types;

namespace InkyBot.Services;

public interface IUpdateProcessorService
{
    ValueTask ProcessUpdate(Update update, CancellationToken cancellationToken = default);

    ValueTask Finish(long chatId);
}