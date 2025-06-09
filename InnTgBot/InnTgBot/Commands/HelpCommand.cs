using InnTgBot.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace InnTgBot.Commands;

public class HelpCommand(ILastMessageService lastMessageService) : ICommandHandler
{
    public string CommandName => "/help";

    public async Task Execute(
        ITelegramBotClient botClient, 
        Message message, 
        CancellationToken cancellationToken)
    {
        const string response = """
                                🌟 *Доступные команды:*

                                • /start — Начало работы с ботом
                                • /help — Показать справку по командам
                                • /hello — Информация о разработчике
                                • /inn — Поиск компаний по ИНН
                                • /last — Повторить последнее действие бота

                                🔍 *Функционал поиска по ИНН:*
                                - Поиск по любому ИНН российской организации
                                - Отображение полного наименования компании
                                - Юридический адрес организации

                                💡 *Пример использования:*
                                Отправьте команду в формате:
                                `/inn 7735211265` или `/inn 7735211265 7735211272` для нескольких


                                _Бот работает на основе открытых данных ФНС России_
                                """;

        await botClient.SendMessage(
            chatId: message.Chat.Id,
            text: response,
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);
        
        await lastMessageService.StoreLastMessage(message.Chat.Id, response);
    }
}