# Phase 1 Foundation Architecture

## Backend boundaries

```text
Domain <- Application <- Infrastructure
                    ^
                    |
                   API
```

- Domain: business objects and invariant exceptions.
- Application: use-case contracts, paging, errors, storage/time interfaces.
- Infrastructure: EF Core, PostgreSQL, provider adapters.
- API: HTTP composition root, CORS, Problem Details, health checks.

## Frontend boundaries

```text
app -> feature pages -> feature components/hooks/api
                  -> shared layouts/components/services/theme
```

Visual components do not call `fetch` directly. All HTTP requests go through `apiClient.ts`.

## Foundation quality gates

- .NET restore/build/test.
- TypeScript type-check.
- Vitest.
- Vite production build.
- GitHub Actions repeats the same checks.

## Deliberately not implemented in Phase 1

- Authentication.
- Database entities for users/courses.
- Role authorization.
- Course/assignment/submission controls.
- Cloud deployment.

This avoids fake controls and keeps Phase 1 focused on stable infrastructure. Phase 2 will add Authentication as one complete vertical slice.
