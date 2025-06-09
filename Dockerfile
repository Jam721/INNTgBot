FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем только файл проекта сначала
COPY ["InnTgBot.csproj", "."]
RUN dotnet restore "InnTgBot.csproj"

# Копируем остальные файлы
COPY . .

RUN dotnet build "InnTgBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InnTgBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InnTgBot.dll"]