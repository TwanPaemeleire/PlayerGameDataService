# Just Dodge + Player Game Data Service

A small Unity bullet-dodging game with its own backend: an ASP.NET Core API and a PostgreSQL database that power a live global leaderboard.

I built this project to learn backend development end to end: API design, databases, auth, Docker and deployment.

**Play it:** [choclified.itch.io/just-dodge](https://choclified.itch.io/just-dodge)

## How it works

```mermaid
flowchart LR
    Game["Unity WebGL game<br/>(itch.io)"] -- "HTTPS + JSON" --> API["ASP.NET Core API<br/>(Docker on Render)"]
    API -- "EF Core" --> DB[("PostgreSQL<br/>(Neon)")]
    Pinger["Uptime monitor"] -- "GET /health" --> API
```

1. After a run, the player types a name (3 to 5 characters) and submits their score.
2. On the first submit, the game registers the name and receives a secret token, which it stores locally.
3. Every score submit sends that token as a `Bearer` header, so nobody can post scores under someone else's name.
4. The main menu shows the global top 10, with each player's best score only, and refreshes it automatically.

## Tech stack

| Part | Tech |
|---|---|
| Game | Unity 6, C#, UnityWebRequest, async/await |
| API | ASP.NET Core minimal APIs on .NET 10 |
| Database | PostgreSQL with Entity Framework Core (Npgsql) and migrations |
| Packaging | Docker (multi-stage build), Docker Compose |
| Hosting | Render (API), Neon (database), itch.io (game) |

## Features

- **Password-free identity:** players only pick a name. The server hands out a random 256-bit token and stores only its SHA-256 hash.
- **Leaderboards:** all-time top lists show one entry per player, with their best score.
- **Rate limiting:** register and submit are limited to 10 requests per minute per IP, using the real client IP behind the host's proxy.
- **Production ready:** migrations run on startup, secrets come from environment variables, `/health` endpoint, CORS limited to itch.io.

## API

| Method | Route | Description |
|---|---|---|
| `POST` | `/players/register` | Register a name, returns a token |
| `POST` | `/scores` | Submit a score (`Authorization: Bearer <token>`) |
| `GET` | `/scores/top?count=10` | All-time top scores, one per player |
| `GET` | `/scores/top/weekly?count=10` | Top scores of the last 7 days |
| `GET` | `/scores/{id}` | A single score |
| `GET` | `/scores` | All scores |
| `GET` | `/players/{name}/bestScore` | A player's best score |
| `GET` | `/players/{name}/stats` | Games played, best and average score, rank |
| `GET` | `/health` | Health check |

## Run it locally

You only need [Docker Desktop](https://www.docker.com/products/docker-desktop/).

```bash
git clone https://github.com/TwanPaemeleire/PlayerGameDataService.git
cd PlayerGameDataService
docker compose up --build
```

The API is now running on `http://localhost:5269`, with its own PostgreSQL database. Try `http://localhost:5269/scores/top`.

To play against it, open `game/JustDodge` in Unity 6 and assign the `ApiConfig_Local` asset to the `LeaderboardService` in the MainMenu scene.
