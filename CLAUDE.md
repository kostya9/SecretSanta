# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build the project (also runs npm install and webpack for CSS)
dotnet build SecretSanta/SecretSanta.csproj

# Run in development mode
dotnet run --project SecretSanta/SecretSanta.csproj

# Publish for production (Linux)
dotnet publish SecretSanta/SecretSanta.csproj --configuration Release -o publish -r linux-x64 --self-contained true
```

## EF Core Migrations

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> --project SecretSanta/SecretSanta.csproj

# Update database
dotnet ef database update --project SecretSanta/SecretSanta.csproj
```

## Architecture

This is a Blazor Server application for organizing Secret Santa gift exchanges with Telegram integration.

### Key Components

- **Telegram Bot Integration** (`Domain/BotWrapper.cs`): Handles Telegram bot commands (`/start`, `/whoamisantafor`) and validates Telegram login authentication via HMAC-SHA256
- **Persistence Layer** (`Domain/Data/`): SQLite database with EF Core. `SqliteDbContext` defines two tables: `santa_event` and `santa_event_membership`
- **User State Management** (`Domain/State/`): Scoped services tracking `TelegramAuth` and `UserSantaEvents` per user session

### Frontend Assets

CSS is built using TailwindCSS via Webpack. The build is integrated into MSBuild via `Assets/StaticAssets.targets`:
- Source files: `SecretSanta/Assets/`
- Output: `SecretSanta/wwwroot/css/main.css`
- Build triggers automatically on `dotnet build`

### Configuration

Environment variables (loaded from `.env` via dotenv.net):
- `DOTNET_BOT_KEY`: Telegram bot API token (required)
- `DOTNET_SQLITE_DB_PATH`: Database storage path (production only)

### Localization

Supports English and Ukrainian (`en`, `uk`). Resources in `Resources/SecretSanta.uk.resx`.
