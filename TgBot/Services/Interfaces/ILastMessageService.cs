namespace TgBot.Services.Interfaces;

/// <summary>
/// Сервис для хранения и получения последних сообщений бота
/// </summary>
public interface ILastMessageService
{
    /// <summary>
    /// Сохраняет последнее сообщение для указанного чата
    /// </summary>
    /// <param name="chatId">Идентификатор чата Telegram</param>
    /// <param name="message">Текст сообщения для сохранения</param>
    Task StoreLastMessage(long chatId, string message);
    
    /// <summary>
    /// Получает последнее сохраненное сообщение для чата
    /// </summary>
    /// <param name="chatId">Идентификатор чата Telegram</param>
    /// <returns>
    /// Сохраненное сообщение или null, если сообщений нет
    /// </returns>
    string? GetLastMessage(long chatId);
}