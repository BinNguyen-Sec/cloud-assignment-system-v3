# Cloud Assignment System V3 — Full Specification

## 1. Frozen decisions

### Product

- V3 is a clean rebuild that reuses proven business rules, not a line-by-line rewrite of V2.
- Roles remain `Admin`, `Teacher`, and `Student`.
- Teacher, Student, and Admin each receive separate overview and feature pages.
- Course libraries support search, sorting, filtering, pagination, and URL-persisted state.
- Teacher can enroll one existing Student manually or import existing Student accounts through `.xlsx`.
- Missing users in Excel are reported; import does not silently create accounts.
- Excel import requires preview and explicit confirmation.
- One current submission exists per student per assignment. Resubmission replaces the file, increments attempt number, and resets grade/feedback.
- Assignment states are `Draft`, `Published`, and `Closed`.
- Late submissions are controlled by `AllowLateSubmission` and are marked late when accepted.
- Backend enforces all role, ownership, and membership rules.
- Frontend uses an original bright “Modern Magical Academy” visual system and does not copy a franchise.
- Google Cloud is the preferred deployment vendor, while cloud dependencies remain behind interfaces.

### Engineering

- Backend projects: Domain, Application, Infrastructure, API, UnitTests, IntegrationTests.
- Frontend is organized by feature.
- Controllers are thin and never return EF entities.
- Application use cases own validation, authorization scope checks, and transactions.
- API errors use RFC 7807 Problem Details with stable error codes.
- IDs use GUID/UUID; timestamps use UTC.
- PostgreSQL is the relational database.
- File metadata is centralized in `FileAsset`.
- Search/filter/sort is server-side and allow-listed.
- Audit logs are append-only.
- Feature delivery is vertical-slice complete to avoid patch-driven spaghetti code.

## 2. Product scope

### Authentication and identity

- Email/password login.
- JWT access tokens.
- Active/disabled accounts.
- Role-based route and API protection.
- Current-user profile.
- Seeded demo accounts.
- Password hashing and non-revealing login failures.

### Courses

- Teacher-owned course creation, editing, archive/restore, and inspection.
- Course code, name, description, semester, academic year, theme preset.
- Student enrollment/removal.
- Manual enrollment.
- Excel template, preview, confirm, history, and result report.
- Teacher and Student search/sort/filter/pagination.
- Dedicated course detail pages and member list.

### Assignments

- Create, edit, publish, close, reopen, delete.
- Deadline, maximum score, late policy.
- Optional instruction file lifecycle.
- Course-specific and role-specific lists.
- Search, sort, filter, pagination.

### Submissions and grading

- Upload, resubmit, download current submission.
- File replacement and safe cleanup.
- Late detection.
- Teacher grading queue.
- Grade and feedback.
- Student result view.

### Admin

- System overview.
- User listing and status management.
- Course, assignment, submission, import, and audit explorers.

### Retained advanced roadmap

- Malware scanning before permanent storage.
- AI-assisted summary and feedback suggestions requiring teacher approval.
- Monitoring, backup/restore, and teardown scripts.

### Out of initial scope

- Live chat, video meetings, payment, plagiarism detection, native mobile apps, teacher-created passwords, and silent account creation from Excel.

## 3. Roles and permissions

### Admin

Can view system-wide data, manage user active status, inspect resources, imports, audit logs, and health summaries. Cannot view raw passwords or secrets.

### Teacher

Can manage only owned courses, enroll existing Students, import class lists, manage assignments and instruction files, inspect/download submissions, and grade. Cannot manage another teacher’s course, change roles, or create missing accounts through import.

### Student

Can view enrolled courses, search/sort them, view published/closed assignments, download instructions, submit/resubmit while policy permits, download their own submission, and view grade/feedback. Cannot see Draft assignments or other students’ work.

### Authorization standard

A role is necessary but not sufficient. Every protected use case also verifies ownership, membership, account status, and resource state.

## 4. Sitemap

### Public

```text
/login
/forbidden
/not-found
```

### Teacher

```text
/teacher/overview
/teacher/courses
/teacher/courses/new
/teacher/courses/:courseId
/teacher/courses/:courseId/edit
/teacher/courses/:courseId/students
/teacher/courses/:courseId/students/import
/teacher/courses/:courseId/assignments
/teacher/assignments/new?courseId=:courseId
/teacher/assignments/:assignmentId
/teacher/assignments/:assignmentId/edit
/teacher/assignments/:assignmentId/submissions
/teacher/submissions/:submissionId
/teacher/import-history
```

Teacher overview contains only high-level counts, upcoming deadlines, ungraded work, recent activity, and shortcuts.

### Student

```text
/student/overview
/student/courses
/student/courses/:courseId
/student/assignments
/student/assignments/:assignmentId
/student/submissions
/student/submissions/:submissionId
```

### Admin

```text
/admin/overview
/admin/users
/admin/users/:userId
/admin/courses
/admin/courses/:courseId
/admin/assignments
/admin/submissions
/admin/imports
/admin/audit-logs
/admin/system
```

### Navigation behavior

- Desktop left navigation rail; mobile drawer.
- Breadcrumbs on detail/edit pages.
- Search/filter/sort/page are query parameters.
- Course detail tabs: Overview, Assignments, Students (Teacher/Admin), Activity.

## 5. Domain and database schema

All IDs are UUID/GUID and timestamps are UTC.

### User

- `Id`
- `StudentCode` nullable, unique when present
- `FullName`
- `Email` normalized unique
- `PasswordHash`
- `Role`: Admin, Teacher, Student
- `IsActive`
- `MustChangePassword`
- `CreatedAt`, `UpdatedAt`

### Course

- `Id`, `Code` unique, `Name`, `Description`
- `Semester`, `AcademicYear`
- `TeacherId`
- `IsArchived`
- `ThemeKey`
- `CreatedAt`, `UpdatedAt`

Indexes: unique Code; TeacherId+IsArchived.

### CourseMember

- `Id`, `CourseId`, `StudentId`
- `EnrollmentSource`: Manual, Excel
- `ImportBatchId` nullable
- `EnrolledAt`

Constraint: unique `(CourseId, StudentId)`.

### Assignment

- `Id`, `CourseId`, `Title`, `Description`
- `Status`: Draft, Published, Closed
- `DueAt`, `AllowLateSubmission`, `MaxScore`
- `InstructionFileId` nullable
- `CreatedById`, `PublishedAt`, `CreatedAt`, `UpdatedAt`

### Submission

- `Id`, `AssignmentId`, `StudentId`, `FileAssetId`
- `AttemptNumber`
- `SubmittedAt`, `IsLate`
- `Status`: Submitted, Graded
- `Grade`, `Feedback`, `GradedById`, `GradedAt`, `UpdatedAt`

Constraint: unique `(AssignmentId, StudentId)`.

### FileAsset

- `Id`, `Provider`, `ContainerName`, `ObjectKey`
- `OriginalFileName`, `ContentType`, `SizeBytes`, `Sha256`
- `UploadedById`, `CreatedAt`, `DeletedAt`

Constraint: unique `(Provider, ContainerName, ObjectKey)`.

### StudentImportBatch

- `Id`, `CourseId`, `UploadedById`, `OriginalFileName`
- `Status`: Previewed, Completed, Failed, Expired
- `TotalRows`, `ValidRows`, `InvalidRows`, `ImportedRows`, `SkippedRows`
- `CreatedAt`, `CompletedAt`, `ExpiresAt`

### StudentImportRow

- `Id`, `BatchId`, `RowNumber`
- `StudentCode`, `FullName`, `Email`
- `ResolvedUserId`
- `Status`: Valid, Invalid, DuplicateInFile, AlreadyEnrolled, UserNotFound, InactiveUser, WrongRole
- `Message`

Constraint: unique `(BatchId, RowNumber)`.

### AuditLog

- `Id`, `ActorUserId`, `Action`, `EntityType`, `EntityId`
- `Metadata` JSONB, `IpAddress`, `UserAgent`, `CreatedAt`

## 6. API contract

Base path: `/api/v1`.

### Conventions

- camelCase JSON.
- ISO 8601 UTC timestamps.
- Paged collections.
- RFC 7807 Problem Details.
- File endpoints return short-lived signed URLs or controlled streams.

### Auth

```text
POST /auth/login
GET  /auth/me
POST /auth/change-password
```

### Overview

```text
GET /teacher/overview
GET /student/overview
GET /admin/overview
```

### Courses

```text
GET    /courses
POST   /courses
GET    /courses/{courseId}
PUT    /courses/{courseId}
DELETE /courses/{courseId}
POST   /courses/{courseId}/archive
POST   /courses/{courseId}/restore
```

`GET /courses` is role-scoped. Query fields: `q`, `sort`, `direction`, `page`, `pageSize`, `semester`, `academicYear`, `archived`, `hasPendingWork`.

### Members

```text
GET    /courses/{courseId}/students
POST   /courses/{courseId}/students
DELETE /courses/{courseId}/students/{studentId}
```

### Excel import

```text
GET  /courses/{courseId}/students/import-template
POST /courses/{courseId}/students/import-preview
POST /courses/{courseId}/students/imports/{batchId}/confirm
GET  /courses/{courseId}/students/imports
GET  /courses/{courseId}/students/imports/{batchId}
GET  /courses/{courseId}/students/imports/{batchId}/error-report
```

### Assignments

```text
GET    /courses/{courseId}/assignments
POST   /courses/{courseId}/assignments
GET    /assignments/{assignmentId}
PUT    /assignments/{assignmentId}
DELETE /assignments/{assignmentId}
POST   /assignments/{assignmentId}/publish
POST   /assignments/{assignmentId}/close
POST   /assignments/{assignmentId}/reopen
```

Instruction files:

```text
POST   /assignments/{assignmentId}/instruction-file
GET    /assignments/{assignmentId}/instruction-file/download
DELETE /assignments/{assignmentId}/instruction-file
```

### Submission and grading

```text
GET  /assignments/{assignmentId}/my-submission
POST /assignments/{assignmentId}/my-submission
GET  /submissions/{submissionId}/download
GET  /student/submissions
GET  /assignments/{assignmentId}/submissions
GET  /submissions/{submissionId}
PUT  /submissions/{submissionId}/grade
GET  /teacher/submissions
```

### Admin

```text
GET /admin/users
GET /admin/users/{userId}
PUT /admin/users/{userId}/status
GET /admin/courses
GET /admin/assignments
GET /admin/submissions
GET /admin/imports
GET /admin/audit-logs
GET /admin/system/health-summary
```

### Paged response

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalItems": 0,
  "totalPages": 0,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

### Problem response

```json
{
  "type": "https://cloud-assignment/errors/course-not-found",
  "title": "Course not found",
  "status": 404,
  "detail": "The requested course does not exist or is outside your access scope.",
  "instance": "/api/v1/courses/...",
  "traceId": "...",
  "errorCode": "COURSE_NOT_FOUND",
  "errors": {}
}
```

## 7. Excel import specification

### Template

Worksheet: `Students`.

| StudentCode | FullName | Email |
|---|---|---|
| 23DH111550 | Nguyễn Ngọc Khải | student@example.com |

Email is required and is the primary identity lookup.

### File rules

- `.xlsx` only.
- Initial max size 5 MB.
- Initial max 1,000 data rows.
- Header comparison is trimmed and case-insensitive.
- Formula cells are rejected for identity fields.
- Empty trailing rows are ignored.
- Uploaded source workbook is not permanently stored by default.

### Preview algorithm

1. Verify Teacher owns the course.
2. Validate file signature, size, worksheet, and headers.
3. Normalize email/student code.
4. Detect duplicates within the file.
5. Resolve all users in batched queries.
6. Require active Student role.
7. Detect existing enrollments.
8. Persist preview batch and row outcomes.
9. Return counts and paged preview rows.

### Confirm

- Explicit Teacher confirmation.
- Batch must not be expired or completed.
- Re-check users and enrollments for concurrent changes.
- Insert still-valid memberships in one transaction.
- Idempotent confirmation.
- Audit event plus result workbook.

## 8. Search, sort, filter, pagination

General query contract:

```text
q
sort
direction=asc|desc
page=1
pageSize=10|20|50
```

Invalid sort fields return `400 INVALID_SORT_FIELD`.

Course sort keys: `updatedAt`, `createdAt`, `name`, `code`, `studentCount`, `assignmentCount`.

Student course search also includes teacher name. Teacher course filters include archive, semester, academic year, and pending work.

Frontend query state is stored in the URL. Search uses debounce. Search/filter changes reset page to 1. Empty search results differ visually from an empty collection.

## 9. Storage, audit, and errors

### Storage abstraction

```csharp
public interface IFileStorageService
{
    Task<StoredFileResult> UploadAsync(
        Stream content,
        FileUploadDescriptor descriptor,
        CancellationToken cancellationToken);

    Task<Uri> CreateDownloadUriAsync(
        string objectKey,
        TimeSpan lifetime,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string objectKey,
        CancellationToken cancellationToken);
}
```

Application code never imports a provider SDK.

Object paths:

```text
courses/{courseId}/assignments/{assignmentId}/instructions/{fileId}/{safeName}
courses/{courseId}/assignments/{assignmentId}/submissions/{studentId}/{fileId}/{safeName}
```

Replacement rule: upload new object → commit DB state → delete old object. Failed cleanup is logged and retried later.

Audit events include authentication, user status, course changes, enrollment/import, assignment lifecycle, instruction file lifecycle, submission creation/replacement/download, and grading. Secrets and tokens are never logged.

## 10. Modern Magical Academy design system

### Experience

A bright, refined digital academy where courses feel like magical disciplines, while remaining appropriate for university work.

### Tokens

```text
Canvas ivory       #F7F3EA
Surface            #FFFDFC
Midnight indigo    #20213A
Academy indigo     #5964D8
Arcane violet      #8759C7
Soft gold          #D8B45B
Cyan glow          #5FC8D7
Success            #3A9B78
Warning            #C88A3D
Danger             #C85D67
```

### Visual language

- Soft parchment gradients.
- Very subtle constellation/rune patterns.
- Restrained glass panels.
- Thin gold/indigo borders.
- Course cards inspired by refined magical study volumes.
- Progress seals and constellation indicators.
- Short fade/lift/shimmer/line-drawing motion.
- No heavy particles or distracting glow.

### Core components

```text
AcademyShell
MagicSidebar
TopCommandBar
CourseGrimoireCard
SpellbookTabs
RuneStatCard
EnchantedDeadline
MagicSearchField
ImportCauldronDropzone
ImportPreviewTable
StatusSigil
ArcaneEmptyState
RuneSkeleton
ConfirmRitualDialog
```

User-facing Vietnamese remains professional; whimsical names are internal component names.

### Accessibility

- Strong contrast and visible focus.
- Keyboard support.
- Reduced-motion support.
- Semantic headings and labeled forms.
- Errors use icon + text, not color alone.

## 11. Source architecture

```text
cloud-assignment-system-v3/
├── backend/
│   ├── CloudAssignment.sln
│   ├── src/
│   │   ├── CloudAssignment.Domain/
│   │   ├── CloudAssignment.Application/
│   │   ├── CloudAssignment.Infrastructure/
│   │   └── CloudAssignment.Api/
│   └── tests/
│       ├── CloudAssignment.UnitTests/
│       └── CloudAssignment.IntegrationTests/
├── frontend/
│   └── cloud-assignment-web/
│       └── src/
│           ├── app/
│           ├── features/
│           ├── layouts/
│           ├── components/
│           ├── services/
│           ├── hooks/
│           ├── utils/
│           └── theme/
├── docker/
├── docs/
├── scripts/
└── .github/workflows/
```

Dependency direction:

```text
Domain ← Application ← Infrastructure
                 ↑          ↑
                 └──── API composition root
```

Frontend feature example:

```text
features/courses/
├── api/
├── components/
├── hooks/
├── pages/
├── schemas/
├── types/
└── utils/
```

Anti-spaghetti rules:

- Page components should not become god components.
- One application handler/service represents one use case.
- No controller-level EF queries.
- No direct `fetch` inside visual components.
- No scattered raw role strings.
- Shared components are extracted only for real reuse.
- Dead code is deleted, not commented out.

## 12. Tests and Definition of Done

### Automated

- Domain/validator unit tests.
- Search/sort mapping tests.
- Late-submission and grade-bound tests.
- Excel classification tests.
- Integration tests for auth, ownership, enrollment, import, assignment states, submission/resubmission, grading, admin, and Problem Details.
- Frontend tests for query state, forms, import wizard, route guards, and all UI states.

### Manual end-to-end

1. Admin sees users and health.
2. Teacher creates a course.
3. Teacher searches/sorts courses.
4. Teacher manually enrolls one Student.
5. Teacher previews an Excel file containing valid, duplicate, already-enrolled, and missing users.
6. Teacher confirms valid rows.
7. Teacher creates/publishes an assignment with instruction file.
8. Student finds the course, downloads instructions, and submits.
9. Teacher finds and grades the submission.
10. Student sees result and resubmits when permitted; grade resets.
11. Audit log contains expected events.
12. Unauthorized cross-role and cross-course access is rejected.
13. Nested-route refresh and mobile layout work.

A feature is Done only when database, backend, authorization, validation, API contract, frontend, all UI states, audit, tests, builds, manual flow, and documentation are complete. No fake controls.

## 13. Cloud architecture and cost guardrails

Preferred Google-centered topology:

```text
GitHub Actions ─────► Firebase Hosting (React)
       │
       └────────────► Cloud Run (ASP.NET Core)
                            │
                    ┌───────┴────────┐
                    ▼                ▼
                PostgreSQL      Cloud Storage
                    │
              Secret Manager
                    │
               Cloud Logging
```

Cloud-agnostic boundaries include `IFileStorageService`, `IClock`, `ICurrentUser`, and EF Core application context interfaces.

No cloud resource is created before a current pricing/free-trial review. Guardrails:

- Dedicated project and resource labels.
- Budget alerts before deployment.
- Cloud Run min instances 0, max instances 1 for demo.
- Conservative CPU/memory/timeouts.
- Upload limits and storage lifecycle.
- No versioning, external load balancer, VM, or HA database unless explicitly approved.
- Written teardown date after report.
- Daily cost check during demo.

Budget alerts are notifications, not a hard spending cap. Managed PostgreSQL selection is deferred to a separate cost review. Local Docker PostgreSQL is the development default.

## 14. Delivery roadmap

### Phase 0 — Specification
Freeze this pack and create the repository/project board.

### Phase 1 — Foundation
Solution skeleton, React skeleton, PostgreSQL Docker, configuration, logging, error middleware, API client, routing, theme tokens, baseline CI/tests.

### Phase 2 — Authentication
User migration, seeds, login, current user, role guards, role layouts, security tests.

### Phase 3 — Course Management (one integrated delivery)
Course CRUD, Teacher/Student libraries, search/sort/filter/pagination, details, members, manual enrollment/removal, Excel template/preview/confirm/result/history, audit, tests.

### Phase 4 — Assignments (one integrated delivery)
CRUD, states, instruction file lifecycle, course pages, visibility/policy rules, search/filter/sort, tests.

### Phase 5 — Submission and Grading
Submit/resubmit/download, late policy, Teacher queue, grade/feedback, Student results, tests.

### Phase 6 — Administration
Overview, users/status, resource explorers, imports, audit, tests.

### Phase 7 — Magical Academy polish
Responsive QA, accessibility, motion, performance, visual consistency, screenshots.

### Phase 8 — Docker and cloud
Full-stack Docker, health checks, approved Google deployment, secrets, logging, teardown.

### Phase 9 — CI/CD and security
Keyless Actions, automated test/deploy, rate limiting, headers, file hardening, malware scanning.

### Phase 10 — AI and deliverables
AI-assisted summary/feedback, final report, diagrams, demo/video flow, cleanup documentation.
