# Phase 3 Verification Checklist

- [ ] `scripts/setup-phase3-database.ps1` creates and applies `CourseManagement` migration.
- [ ] NuGet vulnerability audit reports no vulnerable packages.
- [ ] Backend solution builds with zero warnings/errors.
- [ ] Unit and integration tests pass.
- [ ] Frontend type-check, tests, and production build pass.
- [ ] Teacher creates, searches, sorts, edits, archives, and restores a course.
- [ ] Student sees only enrolled courses.
- [ ] Admin can inspect all courses and student lists.
- [ ] Teacher manually enrolls an existing Student account.
- [ ] Excel template downloads.
- [ ] Preview identifies valid and invalid rows.
- [ ] Confirm imports only currently valid rows and is idempotent.
- [ ] Result report downloads.
- [ ] Import history is visible.
- [ ] `scripts/smoke-test-phase3.ps1` passes against a running API.
- [ ] No fake controls are present.
