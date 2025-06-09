using InnTgBot.BackgroundServices;
using InnTgBot.Commands;
using InnTgBot.Options;
using InnTgBot.Services;
using InnTgBot.Services.Interfaces;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

Console.WriteLine($"Telegram token: {configuration["Telegram:Token"]}");
Console.WriteLine($"Dadata token: {configuration["Dadata:Token"]}");

if (string.IsNullOrEmpty(configuration["Telegram:Token"]))
{
    throw new Exception("Telegram token is missing!");
}

// Сервис на фоне
services.AddHostedService<TelegramBotBackgroundService>();

// Роутер команд
services.AddSingleton<CommandRouter>();

// Сервисы
services.AddSingleton<ICompanyInfoService, CompanyService>();
services.AddSingleton<ILastMessageService, LastMessageService>();

// Наши команды
services.AddSingleton<ICommandHandler, StartCommand>();
services.AddSingleton<ICommandHandler, HelpCommand>();
services.AddSingleton<ICommandHandler, HelloCommand>();
services.AddSingleton<ICommandHandler, InnCommand>();
services.AddSingleton<ICommandHandler, LastCommand>();

// Конфиги
services.Configure<TelegramOptions>(configuration.GetSection(TelegramOptions.Telegram));
services.Configure<DadataOptions>(configuration.GetSection(DadataOptions.Dadata));

var host = builder.Build();
host.Run();