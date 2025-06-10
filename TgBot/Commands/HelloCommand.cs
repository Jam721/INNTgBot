using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgBot.Services.Interfaces;

namespace TgBot.Commands;

/// <summary>
/// Команда вывода контактной информации разработчика
/// </summary>
public class HelloCommand(ILastMessageService lastMessageService) : ICommandHandler
{
    /// <summary>
    /// Системное имя команды: /hello
    /// </summary>
    public string CommandName => "/hello";

    /// <summary>
    /// Отправляет в чат контактные данные разработчика
    /// </summary>
    /// <param name="botClient">API клиент Telegram</param>
    /// <param name="message">Входящее сообщение</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public async Task Execute(
        ITelegramBotClient botClient, 
        Message message, 
        CancellationToken cancellationToken)
    {
        const string response = """
                                    👨‍💻 *Моя контактная информация:*
                                
                                    • *Имя:* Марянян Артур
                                    • *Email:* [arturqweasd@yandex.ru](mailto:arturqweasd@yandex.ru)
                                    • *GitHub:* [Jam721](https://github.com/Jam721)
                                    • *Резюме:* [hh.ru](https://hh.ru/resume/604046e7ff0ee241040039ed1f6b6f685a4669)
                                
                                    _Всегда открыт для сотрудничества!_
                                """;

        await botClient.SendMessage(
            chatId: message.Chat.Id,
            text: response,
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);
        
        await lastMessageService.StoreLastMessage(message.Chat.Id, response);
    }
}