# Windows Setup — Phase 1 Foundation

## 1. Required tools

- Git for Windows
- .NET 10 SDK
- Node.js 24 LTS
- Docker Desktop with WSL 2 backend
- Visual Studio Code

Restart PowerShell and VS Code after installing tools.

## 2. Verify versions

```powershell
git --version
dotnet --version
node --version
npm --version
docker --version
docker compose version
```

Expected baseline:

```text
Git: available
.NET SDK: 10.x
Node.js: 24.x LTS
Docker Compose: v2
```

## 3. Bootstrap

From the repository root:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\scripts\bootstrap.ps1
```

The script:

1. Validates installed tools.
2. Creates local `.env` when missing.
3. Starts PostgreSQL 16.
4. Restores .NET dependencies.
5. Installs frontend dependencies.
6. Runs backend/frontend quality gates.

## 4. Run the system

PowerShell 1:

```powershell
dotnet run --project .\backend\src\CloudAssignment.Api
```

PowerShell 2:

```powershell
cd .\frontend\cloud-assignment-web
npm run dev
```

## 5. Verify

```text
http://localhost:5173
http://localhost:8080/api/v1/system/info
http://localhost:8080/health/live
http://localhost:8080/health/ready
```

The readiness endpoint requires PostgreSQL to be healthy.

## 6. Commit the generated lockfile

After the first successful `npm install`, commit `package-lock.json`:

```powershell
git add .
git commit -m "Establish V3 foundation"
git push origin main
```

## Common issue: port 5432 is occupied

Check:

```powershell
Get-NetTCPConnection -LocalPort 5432 -ErrorAction SilentlyContinue
```

Change `POSTGRES_PORT` in `.env` only if necessary, then also update the local API connection string through an environment variable:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5433;Database=cloud_assignment_v3;Username=cloud_assignment;Password=change-me-local-only"
```

Never commit the real `.env` file.
