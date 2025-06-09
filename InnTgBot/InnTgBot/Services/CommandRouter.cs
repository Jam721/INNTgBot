using InnTgBot.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace InnTgBot.Services;

public class CommandRouter
{
    private readonly Dictionary<string, ICommandHandler> _handlers;


    public CommandRouter(IEnumerable<ICommandHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.CommandName);
    }
    
    public async Task HandleCommandAsync(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken)
    {
        var command = message.Text?.Split(' ')[0].ToLower();
        var chatId = message.Chat.Id;
        
        if (command != null && _handlers.TryGetValue(command, out var handler))
        {
            await handler.Execute(botClient, message, cancellationToken);
        }
        else
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: "Неизвестная команда",
                cancellationToken: cancellationToken);
        }
    }
}