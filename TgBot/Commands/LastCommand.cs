using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgBot.Services.Interfaces;

namespace TgBot.Commands;

/// <summary>
/// Команда повторения последнего действия
/// </summary>
public class LastCommand(ILastMessageService lastMessageService) : ICommandHandler
{
    /// <summary>
    /// Системное имя команды: /last
    /// </summary>
    public string CommandName => "/last";
    
    /// <summary>
    /// Повторяет последнее сообщение, отправленное ботом в чат
    /// </summary>
    /// <param name="botClient">API клиент Telegram</param>
    /// <param name="message">Входящее сообщение</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public async Task Execute(
        ITelegramBotClient botClient, 
        Message message, 
        CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var lastMessage = lastMessageService.GetLastMessage(chatId);

        if (lastMessage == null)
        {
            await botClient.SendMessage(
                chatId,
                "⚠️ Нет сохраненных сообщений для повторения",
                cancellationToken: cancellationToken);
            return;
        }
        
        await botClient.SendMessage(
            chatId,
            lastMessage,
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);
        
        await lastMessageService.StoreLastMessage(chatId, lastMessage);
    }
}