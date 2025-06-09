using InnTgBot.Options;
using InnTgBot.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace InnTgBot.BackgroundServices;

public class TelegramBotBackgroundService(
    IOptions<TelegramOptions> telegramOptions,
    CommandRouter router)
    : BackgroundService
{
    private readonly TelegramOptions _telegramOptions = telegramOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var botClient = new TelegramBotClient(_telegramOptions.Token);

        while (!stoppingToken.IsCancellationRequested)
        {
            await botClient.ReceiveAsync(
                updateHandler: HandleUpdateAsync, 
                errorHandler: HandleErrorAsync, 
                cancellationToken: stoppingToken);
        }
    }

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

        await botClient.SendMessage(
            chatId: chatId,
            text: "Я не понимаю твое сообщение, чтобы узнать как пользоваться моим функционалом напиши /help",
            cancellationToken: cancellationToken);
    }

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