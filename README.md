# Cloud Assignment System V3

Clean, modular, cloud-native assignment platform with a bright Modern Magical Academy interface.

## Current checkpoint

**Phase 3 — Course Management complete**

- .NET 10 modular backend and PostgreSQL 16.
- JWT access token + rotating HttpOnly refresh token.
- Role-scoped Teacher, Student, and Admin navigation.
- Course CRUD, archive/restore, search, sort, filter, and pagination.
- Manual student enrollment and removal.
- Excel `.xlsx` template, preview, confirm, history, and result report.
- Append-only audit events for Course and enrollment workflows.
- Unit tests, integration tests, frontend tests, and runtime smoke test.

## First run after applying Phase 3

```powershell
cd C:\Users\PC\Downloads\cloud-assignment-system-v3
Set-ExecutionPolicy -Scope Process Bypass -Force
.\scripts\setup-phase3-database.ps1
.\scripts\verify.ps1
```

## Run locally

Terminal 1:

```powershell
dotnet run --project .\backend\src\CloudAssignment.Api
```

Terminal 2:

```powershell
cd .\frontend\cloud-assignment-web
npm.cmd run dev
```

Open `http://localhost:5173`.

With the API running:

```powershell
.\scripts\smoke-test-auth.ps1
.\scripts\smoke-test-phase3.ps1
```

## Local demo credentials

```text
admin@arcana.local    / Arcana@2026!
teacher@arcana.local  / Arcana@2026!
student@arcana.local  / Arcana@2026!
student2@arcana.local / Arcana@2026!
student3@arcana.local / Arcana@2026!
```

## Important

- Commit the generated `CourseManagement` migration after database setup.
- Never reuse development JWT keys or demo passwords in production.
- V2 remains the rollback/demo backup until V3 passes complete regression.
