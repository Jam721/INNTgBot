using InnTgBot.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace InnTgBot.Commands;

public class StartCommand : ICommandHandler
{
    public string CommandName => "/start";

    public async Task Execute(
        ITelegramBotClient botClient, 
        Message message, 
        CancellationToken cancellationToken)
    {
        var response = $"""
                        👋 *Добро пожаловать, {message.From?.FirstName}!*

                        Я — GiveINNBot, ваш помощник для получения информации о компаниях по ИНН.

                        🚀 *Основные возможности:*
                        - Мгновенный поиск данных по ИНН
                        - Простая работа через Telegram

                        🔎 *Как начать поиск:*
                        Просто отправьте команду:
                        `/inn [ваш_ИНН]`

                        Например:
                        `/inn 7735211265` или `/inn 7735211265 7735211272` для нескольких

                        📌 Для просмотра всех команд используйте /help

                        _Данные предоставляются на основе открытых источников_
                        """;

        await botClient.SendMessage(
            chatId: message.Chat.Id,
            text: response,
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);
    }
}