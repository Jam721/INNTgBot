using InnTgBot.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace InnTgBot.Commands;

public class LastCommand(ILastMessageService lastMessageService) : ICommandHandler
{
    public string CommandName => "/last";
    
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