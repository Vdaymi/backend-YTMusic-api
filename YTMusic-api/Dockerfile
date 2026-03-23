FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["YTMusic-api/YTMusicApi.csproj", "YTMusic-api/"]
COPY ["YTMusicApi.Data/YTMusicApi.Data.csproj", "YTMusicApi.Data/"]
COPY ["YTMusicApi.Model/YTMusicApi.Model.csproj", "YTMusicApi.Model/"]
COPY ["YTMusicApi.Orchestrator/YTMusicApi.Orchestrator.csproj", "YTMusicApi.Orchestrator/"]
COPY ["YTMusicApi.Platform/YTMusicApi.Platform.csproj", "YTMusicApi.Platform/"]
COPY ["YTMusicApi.Shared/YTMusicApi.Shared.csproj", "YTMusicApi.Shared/"]

RUN dotnet restore "./YTMusic-api/YTMusicApi.csproj"

COPY . .
WORKDIR "/src/YTMusic-api"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./YTMusicApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YTMusicApi.dll"]
