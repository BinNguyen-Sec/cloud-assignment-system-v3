# Phase 3 — Course Management Architecture

Phase 3 is delivered as one vertical slice. Course CRUD, role-scoped libraries, enrollment, Excel preview/confirm, audit logging, frontend pages, tests, migration tooling, and smoke tests are implemented together.

## Backend modules

- `Domain/Courses`: `Course`, `CourseMember`, enrollment source.
- `Domain/StudentImports`: preview batches and row-level outcomes.
- `Domain/Auditing`: append-only audit records.
- `Application/Features/Courses`: role-scoped course queries and lifecycle.
- `Application/Features/Enrollments`: manual enrollment and removal.
- `Application/Features/StudentImports`: upload validation, preview, confirm, history, and report.
- `Infrastructure/Importing`: ClosedXML adapter behind `IStudentWorkbookService`.
- `Api/Endpoints/CourseEndpoints`: thin HTTP mapping only.

## Authorization

- Teacher reads and manages only owned courses.
- Student reads only enrolled courses.
- Admin reads all courses and member lists.
- Excel upload/confirm is Teacher-only and requires ownership.
- Backend scope checks remain authoritative.

## Excel flow

1. Teacher downloads the `.xlsx` template.
2. Server validates extension, size, sheet, headers, formulas, rows, accounts, role, active state, duplicates, and existing membership.
3. Preview batch and row results are persisted for 30 minutes.
4. Teacher explicitly confirms.
5. One `SaveChangesAsync` call atomically writes memberships, final row statuses, batch totals, and audit event.
6. Result report is generated as `.xlsx`.

## Frontend routes

- Teacher: Course Library, create/edit/detail, students, import wizard, import history.
- Student: Course Library and course detail.
- Admin: Course Library, detail, students, and import history.

Search, sort, archive filter, and pagination are server-side and persisted in URL query parameters on Course Library.
