using InnTgBot.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace InnTgBot.Commands;

/// <summary>
/// Стартовая команда при запуске бота
/// </summary>
public class StartCommand(ILastMessageService lastMessageService) : ICommandHandler
{
    /// <summary>
    /// Системное имя команды: /start
    /// </summary>
    public string CommandName => "/start";

    /// <summary>
    /// Отправляет приветственное сообщение и инструкции
    /// </summary>
    /// <param name="botClient">API клиент Telegram</param>
    /// <param name="message">Входящее сообщение</param>
    /// <param name="cancellationToken">Токен отмены</param>
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
        
        await lastMessageService.StoreLastMessage(message.Chat.Id, response);
    }
}