namespace InnTgBot.Services.Interfaces;

public interface ILastMessageService
{
    Task StoreLastMessage(long chatId, string message);
    string? GetLastMessage(long chatId);
}