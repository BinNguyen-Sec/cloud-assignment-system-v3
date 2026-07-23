# Cloud Assignment System V3

Clean, modular, cloud-native assignment platform with a bright Modern Magical Academy interface.

## Current checkpoint

**Phase 2 — Authentication complete**

- .NET 10 modular backend
- React 19 + TypeScript frontend
- PostgreSQL 16 local database
- JWT access tokens
- rotating HttpOnly refresh tokens
- BCrypt password hashing
- Admin / Teacher / Student role policies
- dedicated role overview routes
- unit and integration tests

## First run after extracting the Authentication Pack

```powershell
cd C:\Users\PC\Downloads\cloud-assignment-system-v3
Set-ExecutionPolicy -Scope Process Bypass -Force
.\scripts\bootstrap.ps1
```

The script starts PostgreSQL, creates the local EF Core tool manifest, generates the `InitialIdentity` migration once, applies the schema, installs frontend packages, and executes all quality gates.

## Run locally

Terminal 1:

```powershell
dotnet run --project .\backend\src\CloudAssignment.Api
```

Terminal 2:

```powershell
cd .\frontend\cloud-assignment-web
npm run dev
```

Open `http://localhost:5173`.

With the API still running, verify the complete authentication flow:

```powershell
.\scripts\smoke-test-auth.ps1
```

## Local demo credentials

```text
admin@arcana.local   / Arcana@2026!
teacher@arcana.local / Arcana@2026!
student@arcana.local / Arcana@2026!
```

## Important

- Commit the generated `Persistence/Migrations` files after bootstrap.
- Never reuse the development JWT key or demo password in production.
- V2 remains the rollback/demo backup until V3 passes complete regression.
