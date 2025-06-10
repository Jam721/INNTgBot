using System.Globalization;
using Dadata;
using Dadata.Model;
using InnTgBot.Info;
using InnTgBot.Options;
using InnTgBot.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace InnTgBot.Services;

/// <summary>
/// Сервис для получения информации о компаниях через API Dadata
/// </summary>
public class CompanyService : ICompanyInfoService
{
    private readonly DadataOptions _dadataOptions;

    /// <summary>
    /// Инициализирует новый экземпляр сервиса работы с компаниями
    /// </summary>
    /// <param name="dadataOptions">Настройки доступа к API Dadata</param>
    public CompanyService(
        IOptions<DadataOptions> dadataOptions)
    {
        _dadataOptions = dadataOptions.Value;
    }

    /// <summary>
    /// Получает информацию о компании по ИНН
    /// </summary>
    /// <param name="inn">Идентификационный номер налогоплательщика (10 или 12 цифр)</param>
    /// <returns>
    /// Объект <see cref="CompanyInfo"/> с данными компании.
    /// В случае ошибки свойство <see cref="CompanyInfo.IsError"/> будет true.
    /// </returns>
    public async Task<CompanyInfo> GetCompanyByInnAsync(string inn)
    {
        if (string.IsNullOrWhiteSpace(inn) || inn.Length is not (10 or 12) || !inn.All(char.IsDigit))
        {
            return CreateInvalidInnResponse(inn);
        }

        try
        {
            var api = new SuggestClientAsync(_dadataOptions.Token);
            var result = await api.FindParty(inn);
            
            if (result.suggestions == null || result.suggestions.Count == 0)
            {
                return CreateNotFoundResponse(inn);
            }

            var suggestion = result.suggestions[0];
            var data = suggestion.data;
            
            var companyName = LimitLength(suggestion.value, 500);
            var address = LimitLength(data.address?.value, 300);
            var management = LimitLength(data.management?.name, 100);
            var okved = LimitLength(data.okved, 50);
            
            return new CompanyInfo
            {
                Inn = inn,
                Name = companyName!,
                Address = address,
                Management = management,
                RegistrationDate = data.state?.registration_date?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                StatusDetail = GetStatusDetail(data.state),
                Okved = okved,
                Capital = FormatCapital(data.capital?.value)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return CreateErrorResponse(inn);
        }
    }

    private static string? FormatCapital(decimal? value)
    {
        if (value == null) return null;
        
        try
        {
            return value.Value.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")) + " ₽";
        }
        catch
        {
            return value.Value.ToString("N2") + " ₽";
        }
    }

    private static string? GetStatusDetail(PartyState? state)
    {
        if (state == null) return null;
        
        return state.status switch
        {
            PartyStatus.ACTIVE => "Действующая",
            PartyStatus.LIQUIDATING => "Ликвидируется",
            PartyStatus.LIQUIDATED => "Ликвидирована",
            PartyStatus.BANKRUPT => "Банкротство",
            PartyStatus.REORGANIZING => "Реорганизация",
            _ => "Неизвестный статус"
        };
    }

    private static string? LimitLength(string? input, int maxLength)
    {
        return input?.Length > maxLength ? input[..maxLength] : input;
    }

    private static CompanyInfo CreateInvalidInnResponse(string inn) => new()
    {
        Inn = inn,
        Name = "Неверный формат ИНН"
    };

    private static CompanyInfo CreateNotFoundResponse(string inn) => new()
    {
        Inn = inn,
        Name = "Компания не найдена"
    };

    private static CompanyInfo CreateErrorResponse(string inn) => new()
    {
        Inn = inn,
        Name = "Ошибка при получении данных"
    };
}