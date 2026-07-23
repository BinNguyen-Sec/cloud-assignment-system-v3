# Cloud Assignment System V3

A clean rebuild of the cloud assignment platform using a pragmatic modular-monolith architecture and a bright **Modern Magical Academy** frontend.

## Technology baseline

- Backend: ASP.NET Core on .NET 10 LTS
- Frontend: React 19 + TypeScript + Vite
- Database: PostgreSQL 16
- Local orchestration: Docker Compose
- Tests: xUnit and Vitest
- CI: GitHub Actions

## Repository layout

```text
backend/
  src/
    CloudAssignment.Domain/
    CloudAssignment.Application/
    CloudAssignment.Infrastructure/
    CloudAssignment.Api/
  tests/
    CloudAssignment.UnitTests/
    CloudAssignment.IntegrationTests/
frontend/
  cloud-assignment-web/
docker/
docs/
scripts/
.github/workflows/
```

## Quick start on Windows

1. Install Git, .NET 10 SDK, Node.js 24 LTS, Docker Desktop, and VS Code.
2. Open PowerShell in the repository.
3. Run:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\scripts\bootstrap.ps1
```

4. Start the API:

```powershell
dotnet run --project .\backend\src\CloudAssignment.Api
```

5. In another PowerShell window, start the frontend:

```powershell
cd .\frontend\cloud-assignment-web
npm run dev
```

- Frontend: `http://localhost:5173`
- API: `http://localhost:8080`
- Live health: `http://localhost:8080/health/live`
- Ready health: `http://localhost:8080/health/ready`
- OpenAPI JSON in Development: `http://localhost:8080/openapi/v1.json`

See `docs/SETUP_WINDOWS.md` for full instructions.
