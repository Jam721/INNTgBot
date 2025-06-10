# Используем официальный образ .NET 9.0
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TgBot/TgBot.csproj", "TgBot/"]
RUN dotnet restore "TgBot/TgBot.csproj"
COPY . .
WORKDIR "/src/TgBot"
RUN dotnet build "TgBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TgBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Установка переменных среды для Render
ENV ASPNETCORE_URLS=http://*:$PORT
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TgBot.dll"]