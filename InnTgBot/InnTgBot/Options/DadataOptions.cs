namespace InnTgBot.Options;

/// <summary>
/// Конфигурационные параметры для сервиса Dadata
/// </summary>
public class DadataOptions
{
    /// <summary>
    /// Секция конфигурации (Dadata)
    /// </summary>
    public const string Dadata = nameof(Dadata);
    
    /// <summary>
    /// API-токен для доступа к сервису Dadata
    /// </summary>
    public string Token { get; init; } = string.Empty;
}