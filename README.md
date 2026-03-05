# StihlLearningPlatform

Interaktive Lernplattform mit Angular (Frontend) und ASP.NET Core (Backend).

## Projektstruktur

- `src/` Angular Frontend
- `backend/StihlLearningPlatform.Api/` ASP.NET Core Web API
- `proxy.conf.json` Angular Proxy auf das Backend (`/api`)

## Voraussetzungen

- Node.js und npm
- .NET SDK 8.0

## Backend starten

```bash
cd backend/StihlLearningPlatform.Api
dotnet restore
dotnet run --launch-profile https
```

Backend-Endpunkte:

- `GET https://localhost:5173/api/health`
- `GET https://localhost:5173/api/modules/intro`
- `POST https://localhost:5173/api/modules/intro/submit`
- `GET https://localhost:5173/api/progress/{userId}`

## Frontend starten

```bash
npm run start -- --proxy-config proxy.conf.json
```

Frontend:

- `http://localhost:4200`

## Enthaltenes erstes Lernmodul

- 3 Fragen (Sicherheitsgrundlagen)
- Auswertung mit Punktezahl und bestanden/nicht bestanden
- Erklaerungen pro Frage
- In-Memory Fortschritt pro `userId`
