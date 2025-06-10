namespace TgBot.Options;

/// <summary>
/// Конфигурационные параметры Telegram бота
/// </summary>
public class TelegramOptions
{
    /// <summary>
    /// Секция конфигурации (Telegram)
    /// </summary>
    public const string Telegram = nameof(Telegram);
    
    /// <summary>
    /// Токен доступа к API Telegram Bot
    /// </summary>
    public string Token { get; init; } = string.Empty;
}