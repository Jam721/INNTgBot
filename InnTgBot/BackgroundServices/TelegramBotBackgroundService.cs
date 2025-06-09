using InnTgBot.Options;
using InnTgBot.Services;
using InnTgBot.Services.Interfaces;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace InnTgBot.BackgroundServices;

/// <summary>
/// Фоновый сервис для обработки входящих сообщений Telegram бота
/// </summary>
/// <remarks>
/// Основные функции:
/// - Инициализация и поддержание соединения с Telegram API
/// - Маршрутизация входящих команд
/// - Обработка текстовых сообщений
/// - Глобальная обработка ошибок
/// - Интеграция с системой хранения истории сообщений
/// </remarks>
public class TelegramBotBackgroundService(
    IOptions<TelegramOptions> telegramOptions,
    CommandRouter router,
    ILastMessageService lastMessageService)
    : BackgroundService
{
    private readonly TelegramOptions _telegramOptions = telegramOptions.Value;
    
    /// <summary>
    /// Основной цикл работы бота
    /// </summary>
    /// <param name="stoppingToken">Токен отмены для остановки сервиса</param>
    /// <remarks>
    /// Логика работы:
    /// 1. Создает клиент Telegram Bot API
    /// 2. Запускает бесконечный цикл приема сообщений
    /// 3. Останавливается при получении сигнала отмены
    /// </remarks>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine($"Telegram token: {_telegramOptions.Token}");
        Console.WriteLine($"Token length: {_telegramOptions.Token?.Length}");
        
        if (string.IsNullOrWhiteSpace(_telegramOptions.Token) || 
            !_telegramOptions.Token.Contains(':') || 
            _telegramOptions.Token.Length < 46)
        {
            throw new ArgumentException("Invalid Telegram token format");
        }
        
        var botClient = new TelegramBotClient(_telegramOptions.Token);

        while (!stoppingToken.IsCancellationRequested)
        {
            await botClient.ReceiveAsync(
                updateHandler: HandleUpdateAsync, 
                errorHandler: HandleErrorAsync, 
                cancellationToken: stoppingToken);
        }
    }

    /// <summary>
    /// Обработчик входящих сообщений
    /// </summary>
    /// <param name="botClient">API клиент Telegram</param>
    /// <param name="update">Входящее обновление</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <remarks>
    /// Алгоритм обработки:
    /// 1. Фильтрация только текстовых сообщений
    /// 2. Логирование входящих сообщений
    /// 3. Определение типа сообщения:
    ///    - Команда (начинается с /): передача в CommandRouter
    ///    - Обычный текст: отправка help-подсказки
    /// 4. Сохранение ответа в истории сообщений
    /// </remarks>
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not {} message) return;
        if (message.Text is not {} messageText) return;
    
        var chatId = message.Chat.Id;
        Console.WriteLine($"Сообщение: {messageText} отправлено в чат: {chatId} пользователем: {message.From?.Username}");

        if (messageText.StartsWith($"/"))
        {
            await router.HandleCommandAsync(botClient, message, cancellationToken);
            return;
        }
        
        const string text = "Я не понимаю твое сообщение, чтобы узнать как пользоваться моим функционалом напиши /help";
        
        await botClient.SendMessage(
            chatId: chatId,
            text: text,
            cancellationToken: cancellationToken);
        
        await lastMessageService.StoreLastMessage(message.Chat.Id, text);
    }

    /// <summary>
    /// Глобальный обработчик ошибок
    /// </summary>
    /// <param name="botClient">API клиент Telegram</param>
    /// <param name="exception">Возникшее исключение</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <remarks>
    /// Особенности обработки:
    /// - ApiRequestException: специальная обработка ошибок Telegram API
    /// - Все остальные исключения: логирование полного стека
    /// - Гарантированное возвращение управления (никогда не падает)
    /// </remarks>
    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException => "Ошибка",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}