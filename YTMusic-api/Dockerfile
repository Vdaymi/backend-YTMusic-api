FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["YTMusic-api/YTMusicApi.csproj", "YTMusic-api/"]
COPY ["YTMusicApi.Data/YTMusicApi.Data.csproj", "YTMusicApi.Data/"]
COPY ["YTMusicApi.Model/YTMusicApi.Model.csproj", "YTMusicApi.Model/"]
COPY ["YTMusicApi.Orchestrator/YTMusicApi.Orchestrator.csproj", "YTMusicApi.Orchestrator/"]
COPY ["YTMusicApi.Platform/YTMusicApi.Platform.csproj", "YTMusicApi.Platform/"]

RUN dotnet restore "./YTMusic-api/YTMusicApi.csproj"

COPY . .
WORKDIR "/src/YTMusic-api"
RUN dotnet build "./YTMusicApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./YTMusicApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

COPY YTMusic-api/certs/https.pfx certs/https.pfx

ENV ASPNETCORE_URLS="http://+:8080;https://+:8081"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/app/certs/https.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=Vadim123

ENTRYPOINT ["dotnet", "YTMusicApi.dll"]
