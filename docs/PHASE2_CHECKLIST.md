# Phase 2 Acceptance Checklist

- [ ] `scripts/bootstrap.ps1` completes without an error.
- [ ] EF Core creates and applies `InitialIdentity` migration.
- [ ] Backend build succeeds with warnings treated as errors.
- [ ] Unit and integration tests pass.
- [ ] Frontend type-check, tests, and production build pass.
- [ ] `http://localhost:8080/health/live` returns Healthy.
- [ ] `http://localhost:8080/health/ready` returns Healthy.
- [ ] Teacher login opens `/teacher/overview`.
- [ ] Admin login opens `/admin/overview`.
- [ ] Student login opens `/student/overview`.
- [ ] A Teacher token cannot open the Admin endpoint.
- [ ] Browser reload restores the session through the HttpOnly refresh cookie.
- [ ] Logout clears the session.
- [ ] `scripts/smoke-test-auth.ps1` passes.
- [ ] Generated migration files are committed to Git.
