namespace InnTgBot.Info;

/// <summary>
/// Представляет информацию о юридическом лице или индивидуальном предпринимателе
/// </summary>
public class CompanyInfo
{
    /// <summary>
    /// Идентификационный номер налогоплательщика (ИНН)
    /// </summary>
    public string Inn { get; init; } = string.Empty;
    
    /// <summary>
    /// Полное наименование организации или ФИО индивидуального предпринимателя
    /// </summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>
    /// Юридический адрес организации
    /// </summary>
    public string? Address { get; init; }
    
    /// <summary>
    /// Информация о руководителе (для юр. лиц) или ИП
    /// </summary>
    public string? Management { get; init; }
    
    /// <summary>
    /// Дата регистрации в формате ДД.ММ.ГГГГ
    /// </summary>
    public string? RegistrationDate { get; init; }

    /// <summary>
    /// Человекочитаемое описание статуса компании на русском языке
    /// </summary>
    public string? StatusDetail { get; init; }
    
    /// <summary>
    /// Основной вид деятельности по ОКВЭД
    /// </summary>
    public string? Okved { get; init; }
    
    /// <summary>
    /// Размер уставного капитала с форматированием (10 000,00 ₽)
    /// </summary>
    public string? Capital { get; init; }
    
    /// <summary>
    /// Признак ошибки при получении данных
    /// </summary>
    /// <value>
    /// true - если в названии содержится "Ошибка" или "не найдена"
    /// false - данные получены успешно
    /// </value>
    public bool IsError => Name.Contains("Ошибка") || Name.Contains("не найдена");
}