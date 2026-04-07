# YTMusic Backend API

<p>
  <img src="https://img.shields.io/badge/Engine-.Net%208-purple" alt=".Net Version">
  <img src="https://img.shields.io/badge/Version-v1.1-blue" alt="Api Version">
  <img src="https://img.shields.io/badge/License-MIT-green" alt="License">
</p>

`YTMusic Backend API` is an ASP.NET Core Web API service designed for working with YouTube Music playlists. The project implements a full user lifecycle (registration, verification, JWT-authentication), playlist import, storage, and, most importantly, optimization via a dedicated microservice. It provides a ready, extensible, and secure foundation for music applications, bots, or microservices.

---

## Table of contents

* [Project structure](#project-structure)
* [Getting Started (Docker)](#getting-started-docker)
* [JWT](#jwt)
* [Rate limiter](#rate-limiter)
* [Swagger](#swagger)
* [Endpoints](#endpoints)
* [Optimization Algorithms](#optimization-algorithms)
* [Testing strategy](#testing-strategy)
* [License](#license)
* [Contacts](#contacts)

---

## Project structure

The project is built on the principles of **Clean Architecture** with elements of a **microservice** approach. It is logically divided into layers: Presentation (API), Orchestrator (business logic), Data (data access), Model (contracts), and Platform (infrastructure implementations).


```
YTMusic-api/                 # Main Web API (controllers, Program.cs, Startup.cs) 
  ├─ Controllers/            # API endpoints (Playlists, Tracks, User, PlaylistTrack, UserPlaylist)
  ├─ Extensions/             # Service registrations, RateLimiter extensions, Swagger setup 
  ├─ Middleware/             # Custom middleware (ExceptionHandling, RateLimitResetMiddleware) 
  ├─ Dockerfile              # Dockerfile for the API 
  └─ appsettings.json

YTMusicApi.Model/            # DTOs, contracts/interfaces (I*Repository, I*Orchestrator, IJwtProvider, IPasswordHasher)

YTMusicApi.Orchestrator/     # Business logic layer (services)
  └─ implementations of I*Orchestrator

YTMusicApi.Data/             # Persistence layer (EF Core) 
  ├─ SqlDbContext.cs 
  ├─ Migrations/             # EF Core migrations 
  ├─ Playlist/               # PlaylistDao, PlaylistRepository, mappings
  ├─ Track/                  # TrackDao, TrackRepository
  ├─ User/                   # UserDao, UserRepository
  ├─ PlaylistTrack/          # PlaylistTrackDao, repository
  ├─ UserPlaylist/           # UserPlaylistDao, repository
  └─ YouTube/                # YouTubeRepository, YouTubeSettings (YouTube Data API v3 integration)

YTMusicApi.Platform/         # Infrastructure / technical service implementations 
  ├─ Jwt/                    # JWT generation 
  ├─ Email/                  # Email sending via SMTP (MailKit) 
  └─ PasswordHasher.cs       # BCrypt password hashing

YTMusicApi.Optimizer/        # Microservice for playlist optimization 
  ├─ Optimization/           # Algorithms (Greedy, Ant Colony) 
  └─ Dockerfile              # Dockerfile for the microservice

YTMusicApi.Shared/           # Shared DTOs for communication between API and Optimizer
```

Built with:

*   **.NET 8 / ASP.NET Core** — primary framework for the API
*   **Entity Framework Core** — ORM for **PostgreSQL**
*   **JWT (JSON Web Tokens)** — user authentication
*   **MailKit** — email sending for user verification
*   **Swagger (Swashbuckle)** — automatic API documentation
*   **Rate Limiting Middleware** — request throttling
*   **Docker / Docker Compose** — containerization and deployment
*   **YouTube Data API v3** — integration for retrieving track metadata

---

## Getting Started (Docker)
 
The project is fully containerized, making it easy to get up and running.
 
### 1. Configuration
 
Create a `.env` file in the project's root directory. You can copy the contents from `.env.example` and fill in your values.
 
```dotenv
 # .env
 
 # ASP.NET Core environment (Development or Production)
 ASPNETCORE_ENVIRONMENT=Development
 
 # PostgreSQL Connection String
 DB_CONNECTION_STRING=
 
 # YouTube Data API v3 Key
 # Get it from Google Cloud Console: https://console.cloud.google.com/apis/credentials
 YOUTUBE_API_KEY=
 
 # JWT Secret Key (a long, random, and secret string)
 JWT_SECRET_KEY=
 
 # Base URL of the client application (e.g., React, Vue) for CORS and email links
 CLIENT_BASE_URL=http://localhost:5173
 
 # SMTP Settings for sending verification emails
 SMTP_SENDER_EMAIL=
 SMTP_USERNAME=
 SMTP_PASSWORD=
```
> **Note:** For Gmail, you'll need to generate an "App Password" for the `SMTP_PASSWORD`.

### 2. EF Core migrations

Before starting the application for the first time, you need to apply the database migrations to create the necessary tables in your PostgreSQL database.

Migrations are stored in `YTMusicApi.Data/Migrations/`.
Apply migrations with:

```bash
dotnet ef database update --project ..\YTMusicApi.Data\YTMusicApi.Data.csproj --startup-project YTMusic-api
```

### 3. Run
 
Open a terminal in the root directory and run:
 
```sh
 docker-compose up --build
```

This command will build and start the main API and the optimizer microservice. The API will be available at `http://localhost:8080`.

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

Swagger UI is enabled for API documentation and is available at `http://localhost:8080/swagger/index.html` when the app is running.

---

## Endpoints

**User (Public):**

*   `POST /api/v1/user/register` — Register a new user.
*   `POST /api/v1/user/login` — Authenticate and receive a JWT.
*   `POST /api/v1/user/verify-email` — Verify email using a token from the verification link.
*   `POST /api/v1/user/resend-verification` — Resend the email verification link.
*   `POST /api/v1/user/logout` — Clear the authentication cookie.

**Playlists (Auth required):**

*   `GET /api/v1/playlists` — Get the current user's playlists.
*   `POST /api/v1/playlists` — Add a playlist from YouTube by its ID.
*   `GET /api/v1/playlists/{playlistId}` — Get a specific playlist by its ID.
*   `PUT /api/v1/playlists/{playlistId}` — Sync a playlist with YouTube to update its details and tracks.
*   `DELETE /api/v1/playlists/{playlistId}` — Remove a playlist from the user's profile.
*   `GET /api/v1/playlists/{playlistId}/export` — Export a playlist to a `.csv` file.
*   `POST /api/v1/playlists/{playlistId}/optimize` — Get an optimized list of tracks without saving it.
*   `POST /api/v1/playlists/optimized` — Save an optimized playlist to the user's profile.

**Tracks (Auth required):**

*   `GET /api/v1/playlists/{playlistId}/tracks` — Get all tracks for a specific playlist.
*   `PUT /api/v1/playlists/{playlistId}/tracks` — Sync all tracks in a playlist with YouTube.
*   `GET /api/v1/tracks/{trackId}` — Get a specific track by its ID.

---

## Optimization Algorithms

A core feature of this API is the ability to generate an optimized sub-playlist from a larger source playlist. This complex task is handled by the dedicated `YTMusicApi.Optimizer` microservice.

The goal is to select a sequence of tracks that fits within a specified `timeLimit` while maximizing a "score". This score is a weighted combination of two factors:

1.  **Genre Similarity**: How similar a track's genre (`TopicCategories`) is to the previous track's genre.
2.  **Recency**: The release year of the track (`PublishedAt`).

The API supports two different algorithms for this task:

### 1. Greedy Algorithm

*   **How it works**: A fast and straightforward approach. At each step, it selects the next track that provides the highest immediate score (based on genre and year weights) without considering the long-term impact on the total score.
*   **Best for**: Quick results where "good enough" is sufficient.

### 2. Ant Colony Optimization (ACO)

*   **How it works**: A more sophisticated, metaheuristic algorithm inspired by the foraging behavior of ants. It simulates multiple "ants" that build different playlist solutions. Paths that lead to higher total scores are reinforced, guiding subsequent ants toward better solutions over several iterations.
*   **Best for**: Finding a potentially better (closer to optimal) solution than the Greedy algorithm, at the cost of longer computation time.

### Configurable Parameters

The optimization process can be fine-tuned via the API using these parameters:

*   `timeLimit`: The maximum total duration of the new playlist.
*   `maxTracks`: The maximum number of tracks in the new playlist.
*   `algorithm`: The algorithm to use (`Greedy` or `AntColony`).
*   `genreWeight`: A value from 0.0 to 1.0 that determines the importance of genre similarity in the score calculation (the weight for the year will be `1.0 - genreWeight`).
*   `startTrackId`: An optional track ID to force the playlist to start with a specific track.

---

## Testing Strategy

The project has a comprehensive testing strategy to ensure code quality and reliability, divided into two main parts:

### Unit Tests (`YTMusicApi.Orchestrator.Tests`)

*   **Purpose**: To test the business logic within the `Orchestrator` layer in complete isolation.
*   **Technology**:
    *   **xUnit**: The testing framework used to define and run tests.
    *   **Moq**: Used to create mock objects for dependencies (like repositories and platform services), ensuring that only the orchestrator's logic is being tested.
    *   **FluentAssertions**: Provides a more readable and natural language for writing test assertions.

### Integration Tests (`YTMusicApi.IntegrationTest`)

*   **Purpose**: To test the entire application pipeline, from the HTTP request to the response, ensuring all layers work together correctly.
*   **Technology & Approach**:
    *   **WebApplicationFactory**: Hosts the API in-memory for testing, simulating a real-world environment without needing to deploy it.
    *   **In-Memory Database**: Uses `Microsoft.EntityFrameworkCore.InMemory` to provide a clean, isolated database for each test run, ensuring tests are independent and fast.
    *   **Mocked External Services**: All external dependencies, such as the YouTube Data API, the Optimizer microservice, and the Email Sender, are replaced with mocks. This makes tests deterministic and independent of external factors.
    *   **TestAuthHandler**: A custom authentication handler is used to simulate requests from authenticated users by passing a test user ID in the request headers, allowing for easy testing of protected endpoints.

---

## License

Project `YTMusic Backend API` is distributed under the **MIT License**.

---

## Contacts

Author: Vadym Fil

* GitHub: [Vdaymi](https://github.com/Vdaymi/)
* Email: [vadim2004fil@gmail.com](mailto:vadim2004fil@gmail.com)
* Telegram: [@Vadym_fil](https://t.me/Vadym_fil)
