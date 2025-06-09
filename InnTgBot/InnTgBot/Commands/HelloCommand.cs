using InnTgBot.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace InnTgBot.Commands;

public class HelloCommand(ILastMessageService lastMessageService) : ICommandHandler
{
    public string CommandName => "/hello";

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