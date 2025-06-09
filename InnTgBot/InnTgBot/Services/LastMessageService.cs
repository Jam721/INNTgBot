using System.Collections.Concurrent;
using InnTgBot.Services.Interfaces;

namespace InnTgBot.Services;

public class LastMessageService : ILastMessageService
{
    private readonly ConcurrentDictionary<long, string> _lastMessages = new();
    
    public Task StoreLastMessage(long chatId, string message)
    {
        _lastMessages[chatId] = message;
        
        return Task.CompletedTask;
    }

    public string? GetLastMessage(long chatId)
    {
        return _lastMessages.GetValueOrDefault(chatId);
    }
}