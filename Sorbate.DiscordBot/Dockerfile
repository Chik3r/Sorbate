FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Sorbate.DiscordBot/Sorbate.DiscordBot.csproj", "Sorbate.DiscordBot/"]
RUN dotnet restore "Sorbate.DiscordBot/Sorbate.DiscordBot.csproj"
COPY . .
WORKDIR "/src/Sorbate.DiscordBot"
RUN dotnet build "Sorbate.DiscordBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Sorbate.DiscordBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sorbate.DiscordBot.dll"]
