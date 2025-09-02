# YTMusic Backend API

<p>
  <img src="https://img.shields.io/badge/Engine-.Net%208-purple" alt=".Net Version">
  <img src="https://img.shields.io/badge/Version-v1.0-blue" alt="Api Version">
  <img src="https://img.shields.io/badge/License-MIT-green" alt="License">
</p>

`YTMusic Backend API` — an **ASP.NET Core Web API** for applications that work with playlists and tracks based on YouTube metadata. The project encapsulates core concerns such as JWT authentication, database persistence, and integration with the YouTube Data API. It provides a convenient, extensible and secure foundation for music apps, bots, or microservices.

---

## Table of contents

* [Project structure](#project-structure)
* [Quick start (local)](#quick-start-local)
* [JWT](#jwt)
* [Rate limiter](#rate-limiter)
* [Swagger](#swagger)
* [Endpoints](#endpoints)
* [Docker](#docker)
* [EF Core migrations](#ef-core-migrations)
* [Frontend](#frontend)
* [License](#license)
* [Contacts](#contacts)

---

## Project structure

The project follows an **Onion (layered / clean)** architecture. It is split into logical layers: Presentation (API), Orchestrator (service layer), Data (EF Core / DAOs / repositories), Model (DTOs / interfaces) and Platform (JWT, password hashing implementations).

```
YTMusic-api/                 # Web API (controllers, Program.cs, Startup.cs)
  ├─ Controllers/            # controllers (Playlists, Tracks, User, PlaylistTrack, UserPlaylist)
  ├─ Extensions/             # service registrations, RateLimiter extensions, Swagger setup
  ├─ Middleware/             # ExceptionHandling, RateLimitResetMiddleware
  ├─ Properties/             # launchSettings.json
  ├─ Dockerfile              # Dockerfile for the API
  └─ appsettings.json

YTMusicApi.Model/            # DTOs, contracts/interfaces (IJwtProvider, IPasswordHasher, I*Repository, I*Orchestrator)

YTMusicApi.Orchestrator/     # Business logic: PlaylistOrchestrator, TrackOrchestrator, UserOrchestrator, ...
  └─ implementations of I*Orchestrator

YTMusicApi.Data/             # Persistence / EF Core
  ├─ SqlDbContext.cs
  ├─ Migrations/             # EF Core migrations (InitialCreate)
  ├─ Playlist/               # PlaylistDao, PlaylistRepository, mappings
  ├─ Track/                  # TrackDao, TrackRepository
  ├─ User/                   # UserDao, UserRepository
  ├─ PlaylistTrack/          # PlaylistTrackDao, repository
  ├─ UserPlaylist/           # UserPlaylistDao, repository
  └─ YouTube/                # YouTubeRepository, YouTubeSettings (API integration)

YTMusicApi.Platform/         # Infrastructure / technical services
  ├─ Jwt/                    # JwtOptions.cs, JwtProvider.cs
  └─ PasswordHasher.cs       # password hashing implementation
```

Built with:

* **.NET 8 / ASP.NET Core** — primary framework for the API
* **Entity Framework Core** — ORM for SQL Server
* **JWT (JSON Web Tokens)** — user authentication
* **Swagger (Swashbuckle)** — automatic API documentation
* **Rate Limiting Middleware** — request throttling
* **Docker** — containerization and deployment
* **YouTube Data API v3** — integration for retrieving track metadata

Key controllers:

* `YTMusic-api/Playlist/PlaylistsController.cs`
* `YTMusic-api/Track/TracksController.cs`
* `YTMusic-api/PlaylistTrack/PlaylistTrackController.cs`
* `YTMusic-api/User/UserController.cs`
* `YTMusic-api/UserPlaylist/UserPlaylistController.cs`

---

## Quick start (local)

1. Clone the repository:

```bash
git clone https://github.com/Vdaymi/backend-YTMusic-api.git
cd backend-YTMusic-api
```

2. Restore/build dependencies:

```bash
dotnet restore
dotnet build
```

3. Add a configuration file:

`appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=<DB_SERVER>;Database=<DB_NAME>;User Id=<USER>;Password=<PASSWORD>;"
  },
  "YouTube": {
    "ApiKey": "<YOUR_YT_API_KEY>",
    "ApplicationName": "YourAppName"
  },
  "JwtOptions": {
    "SecretKey": "<REPLACE_WITH_A_STRONG_SECRET>",
    "ExpiresHours": "12"
  }
}
```

4. Apply migrations:

```bash
# if dotnet-ef is not installed
dotnet tool install --global dotnet-ef

dotnet ef database update --project YTMusicApi.Data --startup-project YTMusic-api
```

5. Run the API:

```bash
dotnet run --project YTMusic-api
```

The API will be available on the ports specified in `launchSettings.json` or Kestrel settings.

---

## JWT

* Interface: `YTMusicApi.Model/Auth/IJwtProvider.cs`
* Config: `YTMusicApi.Platform/Jwt/JwtOptions.cs`
* Implementation: `YTMusicApi.Platform/Jwt/JwtProvider.cs` — reads `JwtOptions` (SecretKey, ExpiresHours) and generates signed JWTs.
* Flow: `UserController` → `UserOrchestrator` → `IJwtProvider.GenerateToken()` → token returned to client.
* JWT Bearer authentication is configured in `Program.cs` / `Startup.cs` for validating incoming tokens.

---

## Rate limiter

Rate limiting is implemented in `YTMusic-api/Extensions/RateLimitingExtensions.cs`, `YTMusic-api/Extensions/RateLimitResetStore.cs` and middleware: `YTMusic-api/Middleware/RateLimitResetMiddleware.cs`.
The middleware restricts the number of requests; limits can be configured according to your needs.

---

## Swagger

Swagger UI is enabled for API documentation and is available at `http://localhost:<port>/swagger/index.html` when the app is running.

---

## Endpoints

**Authorized endpoints:**

* `GET /api/v1/playlists` — get playlists list
* `POST /api/v1/playlists` — create a playlist
* `GET /api/v1/playlists/{playlistId}` — get playlist by id
* `PUT /api/v1/playlists/{playlistId}` — update playlist
* `DELETE /api/v1/playlists/{playlistId}` — delete playlist
* `GET /api/v1/playlists/{playlistId}/tracks` — get playlist tracks
* `PUT /api/v1/playlists/{playlistId}/tracks` — update playlist tracks
* `GET /api/v1/tracks/{trackId}` — get track

**Public (no auth) endpoints:**

* `POST /api/v1/user/register` — register user
* `POST /api/v1/user/login` — authenticate user

---

## Docker

1. Create an `https.pfx` certificate and place it into `YTMusic-api/certs`.
2. If your database is deployed in the cloud you can remove the `sqlserver` service from `docker-compose.yml`.

**Run**

```bash
docker-compose up --build -d
```

---

## EF Core migrations

Migrations are stored in `YTMusicApi.Data/Migrations/`.
Apply migrations with:

```bash
dotnet ef database update --project YTMusicApi.Data --startup-project YTMusic-api
```

---

## Frontend

The frontend for this project is deployed at: [https://ytmusicplaylists.vercel.app/](https://ytmusicplaylists.vercel.app/)

---

## License

Project `backend-YTMusic-api` is distributed under the **MIT License**.

---

## Contacts

Author: Vadym Fil

* GitHub: [Vdaymi](https://github.com/Vdaymi/)
* Email: [vadim2004fil@gmail.com](mailto:vadim2004fil@gmail.com)
* Telegram: [@Vadym_fil](https://t.me/Vadym_fil)
