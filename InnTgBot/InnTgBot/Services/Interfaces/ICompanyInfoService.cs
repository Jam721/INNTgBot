using InnTgBot.Info;

namespace InnTgBot.Services.Interfaces;

/// <summary>
/// Предоставляет методы для получения информации о компаниях и ИП по ИНН
/// </summary>
public interface ICompanyInfoService
{
    /// <summary>
    /// Получает информацию о компании/ИП по ИНН
    /// </summary>
    /// <param name="inn">Идентификационный номер налогоплательщика (10 или 12 цифр)</param>
    /// <returns>
    /// Объект <see cref="CompanyInfo"/> с данными компании.
    /// Свойство <see cref="CompanyInfo.IsError"/> указывает на наличие ошибки при запросе.
    /// </returns>
    /// <exception cref="ArgumentException">Выбрасывается если ИНН имеет недопустимый формат</exception>
    Task<CompanyInfo> GetCompanyByInnAsync(string inn);
}