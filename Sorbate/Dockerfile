FROM sonroyaalmerol/steamcmd-arm64:root-bookworm AS base

# Install dotnet onto the image
RUN apt-get update && apt-get install -y --no-install-recommends \
        curl ca-certificates libicu72 libssl3 \
    && curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh \
    && chmod +x /tmp/dotnet-install.sh \
    && /tmp/dotnet-install.sh --channel 10.0 --runtime aspnetcore --install-dir /usr/share/dotnet \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm -rf /var/lib/apt/lists/* /tmp/dotnet-install.sh

ENV DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_NOLOGO=true \
    ASPNETCORE_HTTP_PORTS=8080


# Build program
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Sorbate/Sorbate.csproj", "Sorbate/"]
RUN dotnet restore "Sorbate/Sorbate.csproj"
COPY . .
WORKDIR "/src/Sorbate"
RUN dotnet build "Sorbate.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Sorbate.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish --chown=steam:steam /app/publish .
USER steam
ENTRYPOINT ["dotnet", "Sorbate.dll"]