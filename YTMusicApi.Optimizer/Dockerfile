FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["YTMusicApi.Optimizer/YTMusicApi.Optimizer.csproj", "YTMusicApi.Optimizer/"]
COPY ["YTMusicApi.Shared/YTMusicApi.Shared.csproj", "YTMusicApi.Shared/"]
RUN dotnet restore "./YTMusicApi.Optimizer/YTMusicApi.Optimizer.csproj"

COPY . .
WORKDIR "/src/YTMusicApi.Optimizer"
RUN dotnet build "./YTMusicApi.Optimizer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./YTMusicApi.Optimizer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YTMusicApi.Optimizer.dll"]