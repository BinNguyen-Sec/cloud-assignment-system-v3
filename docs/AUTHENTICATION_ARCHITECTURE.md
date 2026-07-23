# Phase 2 — Authentication Architecture

## Complete vertical slice

- User and RefreshToken domain entities.
- PostgreSQL mappings and generated EF Core migration workflow.
- BCrypt password hashing.
- JWT access tokens with role claims.
- Rotating refresh tokens stored as SHA-256 hashes.
- Refresh token delivered only through an HttpOnly cookie.
- Active-account validation on every access-token authentication.
- Role policies for Admin, Teacher, and Student.
- Login, refresh, logout, current user, and change-password endpoints.
- React authentication context with access token kept in memory.
- Session recovery through the HttpOnly refresh cookie after page reload.
- Public-only and role-protected routes.
- Dedicated role overview pages; no all-in-one dashboard.
- SQLite-backed API integration tests and domain/application unit tests.

## Token lifecycle

```text
Login
  → verify BCrypt password
  → issue 15-minute JWT access token
  → generate random refresh token
  → store only SHA-256 refresh-token hash
  → send raw refresh token in HttpOnly cookie

Page reload
  → frontend POST /auth/refresh with credentials
  → backend rotates refresh token
  → frontend receives a new in-memory access token

Logout or password change
  → revoke refresh token(s)
  → delete cookie
  → clear frontend session
```

## Local demo accounts

| Role | Email | Password |
|---|---|---|
| Admin | admin@arcana.local | Arcana@2026! |
| Teacher | teacher@arcana.local | Arcana@2026! |
| Student | student@arcana.local | Arcana@2026! |

These credentials exist only in `appsettings.Development.json`. Production seeding is disabled and all secrets must be supplied through environment variables or Secret Manager.

## Security decisions

- Passwords are never logged or returned.
- Invalid login errors do not reveal whether an email exists.
- Refresh tokens are never stored in plaintext in PostgreSQL.
- Access tokens are not persisted in localStorage or sessionStorage.
- Disabled accounts are rejected even when an old JWT has not expired.
- Password change revokes all refresh tokens and requires a new login.
- Backend role authorization remains authoritative; route guards only improve UX.
