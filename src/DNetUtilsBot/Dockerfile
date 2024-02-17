FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
USER app
WORKDIR /app

LABEL org.opencontainers.image.source=https://github.com/Misha-133/DNetUtilsBot

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
RUN apk add icu-libs icu-data-full

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/DNetUtilsBot/DNetUtilsBot.csproj", "src/DNetUtilsBot/"]
RUN dotnet restore "./src/DNetUtilsBot/DNetUtilsBot.csproj"
COPY . .
WORKDIR "/src/src/DNetUtilsBot"
RUN dotnet build "./DNetUtilsBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DNetUtilsBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DNetUtilsBot.dll"]