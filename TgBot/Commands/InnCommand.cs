using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgBot.Info;
using TgBot.Services.Interfaces;

namespace TgBot.Commands;

/// <summary>
/// Обработчик команды /inn для поиска информации о компаниях по ИНН
/// </summary>
/// <remarks>
/// Основные функции:
/// - Валидация и парсинг ИНН из сообщения
/// - Поиск данных через внешний сервис (Dadata)
/// - Форматирование и отправка результатов
/// - Обработка ошибок и невалидных входных данных
/// </remarks>
public class InnCommand(
    ICompanyInfoService companyInfoService,
    ILastMessageService lastMessageService)
    : ICommandHandler
{
    /// <summary>
    /// Системное имя команды: /inn
    /// </summary>
    public string CommandName => "/inn";

    /// <summary>
    /// Главный метод обработки команды
    /// </summary>
    /// <param name="botClient">API клиент Telegram</param>
    /// <param name="message">Входящее сообщение</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public async Task Execute(
        ITelegramBotClient botClient, 
        Message message, 
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message.Text))
            {
                await SendUsage(botClient, message.Chat.Id, cancellationToken);
                return;
            }
            
            var (validInns, invalidInns) = ValidateAndExtractInns(message.Text);
            
            if (invalidInns.Count != 0)
            {
                await SendInvalidInnWarning(
                    botClient, 
                    message.Chat.Id, 
                    invalidInns, 
                    cancellationToken);
            }
            
            if (validInns.Count == 0)
            {
                await SendUsage(botClient, message.Chat.Id, cancellationToken);
                return;
            }
            
            var companies = await GetCompanyInfo(validInns);
            
            await SendResponse(botClient, message.Chat.Id, companies, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            await SendErrorMessage(botClient, message.Chat.Id, cancellationToken);
        }
    }

    /// <summary>
    /// Валидация и извлечение ИНН из текста сообщения
    /// </summary>
    /// <param name="messageText">Текст сообщения пользователя</param>
    /// <returns>
    /// Кортеж: 
    ///   - validInns: Валидные ИНН (10/12 цифр)
    ///   - invalidInns: Невалидные значения
    /// </returns>
    /// <remarks>
    /// - Автоматически обрезает список до 15 ИНН
    /// - Игнорирует пустые значения
    /// </remarks>
    private (List<string> validInns, List<string> invalidInns) ValidateAndExtractInns(string messageText)
    {
        var validInns = new List<string>();
        var invalidInns = new List<string>();
        
        var innParts = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .Select(inn => inn.Trim())
            .Where(inn => !string.IsNullOrWhiteSpace(inn))
            .ToList();
        
        foreach (var inn in innParts)
        {
            if (inn.Length is not (10 or 12) || !inn.All(char.IsDigit))
            {
                invalidInns.Add(inn);
            }
            else
            {
                validInns.Add(inn);
            }
        }
        
        return (validInns.Take(15).ToList(), invalidInns);
    }

    /// <summary>
    /// Отправка предупреждения о невалидных ИНН
    /// </summary>
    /// <param name="botClient">API клиент Telegram</param>
    /// <param name="chatId">ID чата</param>
    /// <param name="invalidInns">Список невалидных ИНН</param>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task SendInvalidInnWarning(
        ITelegramBotClient botClient,
        long chatId,
        List<string> invalidInns,
        CancellationToken cancellationToken)
    {
        try
        {
            var warning = new StringBuilder();
            warning.AppendLine("⚠️ *Обнаружены невалидные ИНН:*");
            
            foreach (var inn in invalidInns)
            {
                warning.AppendLine($"- {inn}");
            }
            
            warning.AppendLine("\nℹ️ ИНН должен содержать 10 или 12 цифр");

            await botClient.SendMessage(
                chatId: chatId,
                text: warning.ToString(),
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Отправка инструкции по использованию команды
    /// </summary>
    /// <param name="botClient">API клиент Telegram</param>
    /// <param name="chatId">ID чата</param>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task SendUsage(
        ITelegramBotClient botClient,
        long chatId,
        CancellationToken cancellationToken)
    {
        try
        {
            const string usageText = """
                                     *Использование команды /inn:*
                                     Укажите один или несколько ИНН через пробел после команды.

                                     _Примеры:_
                                     `/inn 7707083893` - информация по одному ИНН
                                     `/inn 7707083893 7719408167` - информация по нескольким ИНН

                                     *Требования к ИНН:*
                                     • Должен содержать только цифры
                                     • Длина 10 знаков (для юр. лиц) или 12 знаков (для ИП)
                                     """;

            await botClient.SendMessage(
                chatId: chatId,
                text: usageText,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);
            
            await lastMessageService.StoreLastMessage(chatId, usageText);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Получение информации о компаниях по ИНН
    /// </summary>
    /// <param name="inns">Список валидных ИНН</param>
    /// <returns>Список объектов CompanyInfo</returns>
    /// <remarks>
    /// Особенности обработки ошибок:
    /// - ApiRequestException: сохраняет сообщение об ошибке от внешнего API
    /// - Общие исключения: возвращает заглушку с ошибкой
    /// </remarks>
    private async Task<List<CompanyInfo>> GetCompanyInfo(List<string> inns)
    {
        var results = new List<CompanyInfo>();
        
        foreach (var inn in inns)
        {
            try
            {
                var company = await companyInfoService.GetCompanyByInnAsync(inn);
                results.Add(company);
            }
            catch (ApiRequestException apiEx)
            {
                Console.WriteLine(apiEx.Message);
                results.Add(new CompanyInfo
                {
                    Inn = inn,
                    Name = "Ошибка при запросе к внешнему сервису",
                    Address = apiEx.Message,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                results.Add(new CompanyInfo
                {
                    Inn = inn,
                    Name = "Внутренняя ошибка сервера",
                });
            }
        }
        
        return results;
    }

    /// <summary>
    /// Форматирование и отправка результатов пользователю
    /// </summary>
    /// <param name="botClient">API клиент Telegram</param>
    /// <param name="chatId">ID чата</param>
    /// <param name="companies">Список компаний</param>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task SendResponse(
        ITelegramBotClient botClient,
        long chatId,
        List<CompanyInfo> companies,
        CancellationToken cancellationToken)
    {
        try
        {
            if (companies.Count == 0 || companies.All(c => c.IsError))
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Не удалось получить информацию по указанным ИНН",
                    parseMode: ParseMode.MarkdownV2,
                    cancellationToken: cancellationToken);
                
                await lastMessageService.StoreLastMessage(chatId, "Не удалось получить информацию по указанным ИНН");
                return;
            }

            var response = new StringBuilder();
            var validCompanies = companies
                .Where(c => !c.IsError)
                .OrderBy(c => c.Name)
                .ToList();
            
            var erroredCompanies = companies
                .Where(c => c.IsError)
                .ToList();

            if (validCompanies.Count != 0)
            {
                response.AppendLine("🏢 *Найденные компании:*");
                response.AppendLine();
                
                foreach (var company in validCompanies)
                {
                    response.AppendLine($"*{company.Name}*");
                    response.AppendLine($"*ИНН:* `{company.Inn}`");
                    
                    response.AppendLine($"*Статус:* {company.StatusDetail ?? "нет данных"}");
                    
                    if (!string.IsNullOrEmpty(company.Address))
                        response.AppendLine($"*Адрес:* {company.Address}");
                    
                    if (!string.IsNullOrEmpty(company.Management))
                        response.AppendLine($"*Руководитель:* {company.Management}");
                    
                    if (!string.IsNullOrEmpty(company.RegistrationDate))
                        response.AppendLine($"*Дата регистрации:* {company.RegistrationDate}");
                    
                    if (!string.IsNullOrEmpty(company.Okved))
                        response.AppendLine($"*ОКВЭД:* {company.Okved}");
                    
                    if (!string.IsNullOrEmpty(company.Capital))
                        response.AppendLine($"*Уставный капитал:* {company.Capital}");
                    
                    response.AppendLine();
                }
            }

            if (erroredCompanies.Count != 0)
            {
                response.AppendLine("⚠️ *Проблемы с обработкой:*");
                response.AppendLine();
                
                foreach (var company in erroredCompanies)
                {
                    response.AppendLine($"*ИНН {company.Inn}:* {company.Name}");
                    if (!string.IsNullOrEmpty(company.Address))
                    {
                        response.AppendLine($"_Причина:_ {company.Address}");
                    }
                    response.AppendLine();
                }
            }

            await SendMessageInParts(
                botClient, 
                chatId, 
                response.ToString(), 
                ParseMode.Markdown, 
                cancellationToken);
            
            await lastMessageService.StoreLastMessage(chatId, response.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            await SendErrorMessage(botClient, chatId, cancellationToken);
        }
    }

    /// <summary>
    /// Отправка длинных сообщений частями (ограничение Telegram: 4096 символов)
    /// </summary>
    /// <param name="botClient">API клиент Telegram</param>
    /// <param name="chatId">ID чата</param>
    /// <param name="text">Текст сообщения</param>
    /// <param name="parseMode">Режим форматирования</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <remarks>
    /// - Автоматически разбивает текст по строкам
    /// - Добавляет задержку 300мс между сообщениями
    /// </remarks>
    private async Task SendMessageInParts(
        ITelegramBotClient botClient,
        long chatId,
        string text,
        ParseMode parseMode,
        CancellationToken cancellationToken)
    {
        const int maxMessageLength = 4096;
        var messages = SplitMessage(text, maxMessageLength);

        foreach (var messagePart in messages)
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: messagePart,
                parseMode: parseMode,
                cancellationToken: cancellationToken);
            
            await lastMessageService.StoreLastMessage(chatId, messagePart);
            
            await Task.Delay(300, cancellationToken);
        }
    }

    /// <summary>
    /// Соединение сообщений
    /// </summary>
    private List<string> SplitMessage(string text, int maxLength)
    {
        var messages = new List<string>();
        var currentPart = new StringBuilder();

        foreach (var line in text.Split('\n'))
        {
            if (currentPart.Length + line.Length + 1 > maxLength)
            {
                messages.Add(currentPart.ToString());
                currentPart.Clear();
            }

            currentPart.AppendLine(line);
        }

        if (currentPart.Length > 0)
        {
            messages.Add(currentPart.ToString());
        }

        return messages;
    }

    /// <summary>
    /// Случай ошибки
    /// </summary>
    private async Task SendErrorMessage(
        ITelegramBotClient botClient,
        long chatId,
        CancellationToken cancellationToken)
    {
        try
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: "⚠️ Произошла непредвиденная ошибка. Пожалуйста, попробуйте позже.",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);

            await lastMessageService.StoreLastMessage(chatId,
                "⚠️ Произошла непредвиденная ошибка. Пожалуйста, попробуйте позже.");
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}