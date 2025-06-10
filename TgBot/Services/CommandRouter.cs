using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot.Services.Interfaces;

namespace TgBot.Services;

/// <summary>
/// Маршрутизатор команд для обработки входящих сообщений
/// </summary>
public class CommandRouter
{
    private readonly ILastMessageService _lastMessageService;
    private readonly Dictionary<string, ICommandHandler> _handlers;

    /// <summary>
    /// Инициализирует новый экземпляр маршрутизатора команд
    /// </summary>
    /// <param name="handlers">Коллекция обработчиков команд</param>
    /// <param name="lastMessageService">Сервис для работы с последними сообщениями</param>
    public CommandRouter(
        IEnumerable<ICommandHandler> handlers, 
        ILastMessageService lastMessageService)
    {
        _lastMessageService = lastMessageService;
        _handlers = handlers.ToDictionary(h => h.CommandName);
    }
    
    /// <summary>
    /// Обрабатывает входящую команду
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot API</param>
    /// <param name="message">Входящее сообщение</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Задача, представляющая асинхронную операцию обработки</returns>
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
            const string errorMessage = "Неизвестная команда";
            
            await botClient.SendMessage(
                chatId: chatId,
                text: errorMessage,
                cancellationToken: cancellationToken);
            
            await _lastMessageService.StoreLastMessage(message.Chat.Id, errorMessage);
        }
    }
}