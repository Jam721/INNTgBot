using System.Collections.Concurrent;
using TgBot.Services.Interfaces;

namespace TgBot.Services;

/// <summary>
/// Сервис для хранения последнего сообщения в чате
/// </summary>
public class LastMessageService : ILastMessageService
{
    private readonly ConcurrentDictionary<long, string> _lastMessages = new();
    
    /// <summary>
    /// Сохраняет последнее сообщение для указанного чата
    /// </summary>
    /// <param name="chatId">Идентификатор чата в Telegram</param>
    /// <param name="message">Текст сообщения для сохранения</param>
    /// <returns>Задача, представляющая асинхронную операцию</returns>
    public Task StoreLastMessage(long chatId, string message)
    {
        _lastMessages[chatId] = message;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Получает последнее сохраненное сообщение для чата
    /// </summary>
    /// <param name="chatId">Идентификатор чата в Telegram</param>
    /// <returns>
    /// Последнее сохраненное сообщение или null, если сообщения нет
    /// </returns>
    public string? GetLastMessage(long chatId)
    {
        return _lastMessages.GetValueOrDefault(chatId);
    }
}