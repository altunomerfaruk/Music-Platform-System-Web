
# MusicProject Backend Rules

## Scope

Apply these rules when working with:

- Controllers
- Services
- Workflow Services
- Repositories
- EF Core
- Models
- Migrations
- Authentication
- Hangfire
- Audio storage
- Moderation
- Publication

---

## Architecture

Preferred flow:

Controller
    ↓
Workflow / Service
    ↓
Repository
    ↓
Database

Keep controllers thin.

Controllers should:

- validate request-level input
- map ViewModels to requests
- call services/workflows
- return views/results

Controllers must not access repositories directly.

---

## Services

Services contain business rules.

Workflow services are appropriate for operations involving multiple resources or steps, such as:

- Song create/update/delete
- Album create/update/delete
- MP3 files
- Hangfire jobs
- DB updates

Do not create a workflow service for trivial CRUD.

---

## Repositories

Repositories handle database access.

Prefer filtering in SQL using `IQueryable`.

Do not fetch large datasets and then filter in memory when SQL can perform the filtering.

Use `AsNoTracking()` for read-only queries where appropriate.

Use `AsSplitQuery()` where multiple collection includes would otherwise create problematic cartesian joins.

---

## Public Song Visibility

Normal user-facing song queries must enforce:

Song PublicationStatus == Published
AND
Song IsAdminHidden == false
AND
(
    AlbumId == null
    OR
    (
        Album PublicationStatus == Published
        AND
        Album IsAdminHidden == false
    )
)

Do not weaken this rule.

---

## Public Album Visibility

Normal user-facing album queries must enforce:

Album PublicationStatus == Published
AND
Album IsAdminHidden == false

Songs included in public album results must also respect song moderation/publication.

---

## Artist Visibility

ArtistDashboard may show the artist's own:

- Draft
- Scheduled
- Published
- Archived
- Admin-hidden

content.

Admin-hidden content must remain visually marked.

Artist must not be able to clear moderation.

---

## Admin Moderation

Moderation state:

IsAdminHidden
AdminHiddenReason
AdminHiddenAtUtc

is controlled by Admin.

If an album is admin-hidden, its songs are effectively hidden from users.

Do not automatically set every child song's `IsAdminHidden`.

A directly hidden child song must remain hidden after album hide/unhide cycles.

---

## Moderation Bypass Prevention

If a song belongs to an admin-hidden album:

existingSong.Album?.IsAdminHidden == true

the artist must not change its AlbumId to escape moderation.

Workflow validation should return the error against AlbumId where possible.

Service-level validation may remain as defense-in-depth.

---

## Publication Status

Statuses:

Draft
Scheduled
Published
Archived

Do not confuse publication with moderation.

Published does not imply publicly accessible if admin moderation blocks it.

---

## Hangfire

Scheduled publication uses Hangfire.

When replacing scheduled jobs:

- create the new provisional job
- update DB
- after DB success, clean up the old job

Do not treat failure to clean up the old job as failure of a DB update that already succeeded.

Log cleanup failure where appropriate.

---

## Multi-Step Atomicity

For update operations:

### Before DB success

New resources are provisional:

- new MP3
- new Hangfire job

If DB update fails:

- delete new MP3
- cancel new job
- preserve old resources

### After DB success

New resources are authoritative.

Cleanup old resources:

- old MP3
- old Hangfire job

If cleanup fails:

- do not delete the new resources
- do not report the successful DB update as rolled back
- log cleanup failure

---

## Song Creation Failure

Song creation involving:

MP3
DB
Hangfire

must not leave a misleading half-created state where reasonably avoidable.

If the workflow reports creation failure after creating resources, clean up:

- provisional job
- created song record according to current deletion strategy
- uploaded MP3

Preserve the original error if cleanup itself fails.

---

## Song Deletion

When deleting a song:

- apply the project's deletion strategy
- cancel relevant Hangfire job
- remove physical MP3 when appropriate

Cleanup errors should not corrupt unrelated DB state.

---

## Audio Storage

Audio files remain at:

Storage/Audio/

outside `wwwroot`.

Generate safe server-side filenames.

Do not trust the original upload filename as storage path.

Validate supported file type and size.

Do not expose storage paths to the browser.

---

## Streaming

User audio must be served through the protected stream action.

Before opening audio:

1. resolve the song through the public listening visibility rules
2. verify an audio file exists
3. open the stored file
4. stream with range processing

Keep:

enableRangeProcessing: true

Do not convert audio storage into public static files.

---

## Authentication

Do not weaken existing authentication.

Password hashes remain hashed.

Inactive users must not authenticate.

Role claims must stay consistent.

Do not remove authorization for easier development/testing.

---

## Admin Access

AdminDashboard must remain restricted to:

Admin

Hangfire Dashboard must remain Admin-only.

---

## User → Artist Promotion

Do not implement or restore:

User → Artist promotion

unless explicitly requested.

If old promotion symbols reappear, remove them.

---

## EF Core

Keep migrations focused.

Do not edit historical migrations unless there is a specific reason.

New schema changes should normally receive a new migration.

Do not introduce destructive DB changes casually.

---

## Error Mapping

Workflow failures should map to the relevant UI field where possible.

Examples:

- duplicate title → Title
- invalid MP3 → AudioFile
- invalid schedule → ScheduledPublishAt
- invalid album selection → AlbumId

Do not show an AlbumId error under Title.

---

## Validation

Server-side validation is authoritative.

Do not rely only on JavaScript for business rules.

Validate ownership server-side.

Examples:

- selected album belongs to artist
- requested song belongs to artist
- requested album belongs to artist
- selected genres are valid if required by the current design

---

## Code Quality

Prefer:

- focused methods
- clear names
- limited responsibilities
- explicit cleanup phases
- reusable helpers where they genuinely reduce duplication

Avoid:

- giant controller actions
- hidden side effects
- unnecessary abstractions
- unrelated refactors during bug fixes

---

## Final Backend Verification

After backend changes:

1. Search for obsolete references introduced/removed by the change.
2. Check interfaces and implementations match.
3. Run:

dotnet build

4. Verify no new build errors.
5. If applicable, test the affected workflow.

Do not commit or push unless explicitly requested.


============================================================
TARGET STRUCTURE
============================================================

OnlineKitap/
├── CLAUDE.md
└── .claude/
    └── rules/
        ├── frontend.md
        └── backend.md

Replace the old large CLAUDE.md with FILE 1.
Create frontend.md with FILE 2.
Create backend.md with FILE 3.