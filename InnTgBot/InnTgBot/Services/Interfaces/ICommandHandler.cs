using Telegram.Bot;
using Telegram.Bot.Types;

namespace InnTgBot.Services.Interfaces;

public interface ICommandHandler
{
    string CommandName { get; }
    Task Execute(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);
}