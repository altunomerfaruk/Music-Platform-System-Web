# CLAUDE.md — MusicProject Core Rules

## Project

This is an existing ASP.NET Core MVC music platform.

Main stack:
- ASP.NET Core MVC
- Razor `.cshtml`
- EF Core
- SQL Server
- Hangfire
- Existing CSS and JavaScript

Preserve the current architecture and application behavior.

Do not rewrite the project into another frontend or backend framework.

---

## Priority Order

When decisions conflict, use this priority:

1. Correctness
2. Security / authorization
3. Existing application behavior
4. Data integrity
5. Usability
6. Maintainability
7. Responsive design
8. Visual polish

Never sacrifice a higher-priority item for a lower-priority one.

---

## Architecture

Prefer this flow:

Controller
    ↓
Workflow / Service
    ↓
Repository
    ↓
Database

Controllers should stay thin.

Controllers must not access repositories directly.

Business rules belong in services/workflow services.

Database access belongs in repositories.

Do not create unnecessary abstractions.

---

## Existing Refactor

Preserve the current refactored structure, including:

- `Contracts/Requests`
- response DTOs
- partial controllers
- `ArtistSongWorkflowManager`
- `ArtistAlbumWorkflowManager`
- repository-side SQL filtering

Do not undo the recent architecture refactor unless explicitly requested.

---

## Admin Moderation

Admin moderation is stronger than publication status.

A song or album may be:

Published + Admin Hidden

and must still be inaccessible to normal users.

Rules:

- Artist cannot clear admin moderation.
- Artist may still see own hidden content in ArtistDashboard.
- Admin-hidden album hides its songs from normal users.
- A directly hidden song remains hidden independently of album visibility.
- Artist must not bypass an admin-hidden album by moving a song to another album or Single.

Do not weaken these rules.

---

## Publication

Publication statuses:

- Draft
- Scheduled
- Published
- Archived

Hangfire controls scheduled publication.

Do not break:
- scheduled song publication
- scheduled album publication
- cancellation/replacement of publication jobs
- album-controlled song publication behavior

---

## Audio

Uploaded MP3 files must remain outside `wwwroot`.

Current storage:

Storage/
└── Audio/

Do not expose physical audio file paths directly.

Normal user audio access must go through the protected streaming endpoint.

Streaming must respect:

- Published status
- song admin moderation
- album admin moderation
- existing authorization

Keep range processing enabled for audio streaming.

---

## Multi-Step Operations

For operations involving:

- database updates
- Hangfire jobs
- uploaded files

distinguish clearly between:

1. provisional resources before DB success
2. cleanup after DB success

If DB update fails:
- clean provisional new resources

If DB update succeeds:
- do not rollback successful DB state because cleanup of old resources failed
- log cleanup failures where appropriate

Avoid partial state whenever reasonably possible.

---

## Abandoned Features

Do not reintroduce abandoned features unless explicitly requested.

In particular:

User → Artist promotion

must not be added back.

---

## Frontend

Frontend-specific rules are located in:

.claude/rules/frontend.md

Follow them whenever modifying Razor, CSS, JavaScript, layouts, partials or UI behavior.

---

## Backend

Backend-specific rules are located in:

.claude/rules/backend.md

Follow them whenever modifying controllers, services, workflow services, repositories, models, migrations, Hangfire, authentication or business rules.

---

## Validation

Before declaring development work complete:

dotnet build

must succeed.

When frontend behavior changed, also perform browser validation according to the frontend rules.

---

## Git

Do not:

- commit
- push
- rewrite Git history

unless explicitly requested.

Do not include test MP3 files in Git.

---

## Documentation

Keep `ARCHITECTURE.md` consistent with the actual codebase.

Do not document:
- abandoned features
- audio files as public static files
- architecture that does not match the implementation

---

## General Rules

Before changing an existing feature:

1. Read the current implementation.
2. Find its callers and dependencies.
3. Preserve working behavior unless the task requires changing it.
4. Prefer focused changes over large unrelated refactors.
5. Remove dead references created by your own changes.
6. Do not hide backend bugs with frontend workarounds.

Do not endlessly refactor code that is already correct and maintainable.

@.claude/rules/frontend.md
@.claude/rules/backend.md