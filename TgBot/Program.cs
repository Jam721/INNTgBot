using TgBot.BackgroundServices;
using TgBot.Commands;
using TgBot.Options;
using TgBot.Services;
using TgBot.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

configuration.AddEnvironmentVariables();

// Регистрация сервисов
builder.Services.AddSingleton<CommandRouter>();
builder.Services.AddSingleton<ICompanyInfoService, CompanyService>();
builder.Services.AddSingleton<ILastMessageService, LastMessageService>();

// Регистрация команд
builder.Services.AddSingleton<ICommandHandler, StartCommand>();
builder.Services.AddSingleton<ICommandHandler, HelpCommand>();
builder.Services.AddSingleton<ICommandHandler, HelloCommand>();
builder.Services.AddSingleton<ICommandHandler, InnCommand>();
builder.Services.AddSingleton<ICommandHandler, LastCommand>();

// Конфигурация
builder.Services.Configure<TelegramOptions>(configuration.GetSection(TelegramOptions.Telegram));
builder.Services.Configure<DadataOptions>(configuration.GetSection(DadataOptions.Dadata));

// Фоновый сервис
builder.Services.AddHostedService<TelegramBotBackgroundService>();

var app = builder.Build();

// Health check эндпоинты
app.MapGet("/", () => {
    Console.WriteLine($"[{DateTime.UtcNow}] Root health check");
    return "Bot is running";
});

app.MapGet("/health", () => 
{
    Console.WriteLine($"[{DateTime.UtcNow}] Health check passed");
    return Results.Ok("Bot is alive");
});

// Ключевое добавление: keep-alive механизм
app.Lifetime.ApplicationStarted.Register(() => 
{
    _ = Task.Run(async () =>
    {
        while (!app.Lifetime.ApplicationStopping.IsCancellationRequested)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] Keep-alive heartbeat");
            await Task.Delay(TimeSpan.FromMinutes(7), app.Lifetime.ApplicationStopping);
        }
    });
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"Application listening on port: {port}");

app.Run();