using Telegram.Bot;
using Telegram.Bot.Types;

namespace InnTgBot.Services.Interfaces;

/// <summary>
/// Интерфейс для обработчиков команд бота
/// </summary>
public interface ICommandHandler
{
    /// <summary>
    /// Название команды (например: "/start")
    /// </summary>
    string CommandName { get; }
    
    /// <summary>
    /// Выполняет обработку команды
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot API</param>
    /// <param name="message">Входящее сообщение с командой</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Task, представляющий асинхронную операцию</returns>
    Task Execute(
        ITelegramBotClient botClient, 
        Message message, 
        CancellationToken cancellationToken);
}