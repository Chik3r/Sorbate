FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Sorbate.Storage/Sorbate.Storage.csproj", "Sorbate.Storage/"]
RUN dotnet restore "Sorbate.Storage/Sorbate.Storage.csproj"
COPY . .
WORKDIR "/src/Sorbate.Storage"
RUN dotnet build "Sorbate.Storage.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Sorbate.Storage.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sorbate.Storage.dll"]
