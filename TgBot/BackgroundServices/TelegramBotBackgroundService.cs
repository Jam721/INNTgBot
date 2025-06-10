using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using TgBot.Options;
using TgBot.Services;
using TgBot.Services.Interfaces;

namespace TgBot.BackgroundServices;

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
        if (string.IsNullOrWhiteSpace(_telegramOptions.Token) || 
            !_telegramOptions.Token.Contains(':') || 
            _telegramOptions.Token.Length < 46)
        {
            throw new ArgumentException("Invalid Telegram token format");
        }
        
        var botClient = new TelegramBotClient(_telegramOptions.Token);
        var me = await botClient.GetMe(cancellationToken: stoppingToken);
        Console.WriteLine($"Bot started: @{me.Username}");
    
        var lastUpdateId = 0;
        var isProcessing = false;
    
        while (!stoppingToken.IsCancellationRequested)
        {
            if (isProcessing)
            {
                await Task.Delay(1000, stoppingToken);
                continue;
            }
        
            try
            {
                isProcessing = true;
                var updates = await botClient.GetUpdates(
                    offset: lastUpdateId + 1,
                    timeout: 30,
                    cancellationToken: stoppingToken
                );

                foreach (var update in updates)
                {
                    await HandleUpdateAsync(botClient, update, stoppingToken);
                    lastUpdateId = update.Id;
                }
            }
            catch (ApiRequestException ex) when (ex.Message.Contains("terminated by other getUpdates"))
            {
                Console.WriteLine("GetUpdates conflict, increasing delay...");
                await Task.Delay(10000, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled error: {ex.Message}");
                await Task.Delay(5000, stoppingToken);
            }
            finally
            {
                isProcessing = false;
            }
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
        try
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
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обработки сообщения: {ex}");
            // Попытка отправить сообщение об ошибке пользователю
            try
            {
                await botClient.SendMessage(
                    chatId: update.Message!.Chat.Id,
                    text: "⚠️ Произошла внутренняя ошибка. Попробуйте позже.",
                    cancellationToken: cancellationToken
                );
            }
            catch { /* Игнорируем вторичные ошибки */ }
        }
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