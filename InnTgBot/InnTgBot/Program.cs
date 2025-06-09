using InnTgBot.BackgroundServices;
using InnTgBot.Commands;
using InnTgBot.Options;
using InnTgBot.Services;
using InnTgBot.Services.Interfaces;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// Сервис на фоне
services.AddHostedService<TelegramBotBackgroundService>();

// Роутер команд
services.AddSingleton<CommandRouter>();

// Наши команды
services.AddSingleton<ICommandHandler, StartCommand>();

// Конфиги
services.Configure<TelegramOptions>(configuration.GetSection(TelegramOptions.Telegram));

var host = builder.Build();
host.Run();