# MusicProject Frontend & Visual QA Rules

## Scope

Apply these rules when working with:

- Razor `.cshtml`
- layouts
- partial views
- CSS
- JavaScript
- responsive behavior
- forms
- user-facing UI

---

## Always Do First

Before frontend changes:

1. Invoke the `frontend-design` skill if available.
2. Read the relevant:
   - Razor view
   - layout
   - partials
   - CSS
   - JavaScript
3. Understand the current design before editing.

Do not evaluate a page only from source code if it can be rendered.

---

## Existing Design

The existing MusicProject UI is the primary reference.

Preserve:

- dark theme
- green primary color
- sidebar structure
- card style
- current typography hierarchy
- dashboard visual language

Improve consistency and usability.

Do not redesign the entire application.

Do not introduce:

- Tailwind
- Bootstrap
- React
- another frontend framework

unless explicitly requested.

---

## Backend Safety

Frontend work must not break:

- `asp-controller`
- `asp-action`
- `asp-for`
- model binding
- antiforgery
- validation
- routes
- authorization
- publication
- admin moderation
- MP3 upload
- MP3 streaming
- Hangfire behavior

Do not move backend business logic into JavaScript.

Server-side rules remain authoritative.

---

## Local Rendering

Always test through the real ASP.NET application.

Never screenshot:

file:///

Use:

dotnet run

or the existing launch configuration.

Detect the actual localhost URL.

Do not assume a fixed port.

Reuse an existing running instance if appropriate.

---

## Authentication

Do not:

- remove `[Authorize]`
- bypass login
- weaken role checks

for frontend testing.

Use legitimate development/test accounts where available.

Relevant roles:

- User
- Artist
- Admin

---

## Screenshot Directory

Store screenshots under:

docs/ui-review/

Examples:

user-index-before.png
user-index-after.png
artist-my-songs-before.png
artist-my-songs-after.png
admin-songs-before.png
admin-songs-after.png

Do not place screenshots in source folders.

Remove redundant screenshots at the end.

---

## Visual QA Loop

For meaningful frontend changes:

1. Run the app.
2. Open the real page.
3. Take a screenshot.
4. Inspect the screenshot.
5. Identify concrete visible problems.
6. Make focused improvements.
7. Reload.
8. Take another screenshot.
9. Compare before and after.
10. Repeat only if meaningful issues remain.

Maximum:

3 rounds per page

Stop earlier when:

- layout is clean
- hierarchy is clear
- spacing is consistent
- responsive behavior works
- no obvious usability issue remains
- remaining changes would mostly be personal taste

If a change makes the result worse, revert it.

Do not endlessly tweak tiny cosmetic values.

---

## Review Criteria

Check:

### Layout
- alignment
- width
- overflow
- whitespace
- grid consistency

### Spacing
- margins
- padding
- card gaps
- form spacing
- table spacing

### Typography
- hierarchy
- readability
- line height
- long text handling

### Components
- buttons
- forms
- cards
- tables
- badges
- alerts
- empty states
- upload controls
- filters

### States

Where relevant, interactive elements should have:

- hover
- focus-visible
- active
- disabled

Avoid:

transition: all;

---

## Responsive Review

Check at least:

### Desktop

~1440px

### Tablet

~768–1024px

### Mobile

~375–430px

Check:

- horizontal overflow
- sidebar behavior
- table overflow
- button wrapping
- form widths
- long titles
- filters
- player positioning

---

## User Dashboard

Review where applicable:

- Index
- AllSongs
- SongDetails
- ArtistDetails
- AlbumDetails
- SearchResults
- LikedSongs
- ListeningHistory
- UserSettings

Pay special attention to:

- play/pause
- like buttons
- filters
- search
- global player
- empty states

---

## Audio Player

Test:

- first play
- pause
- resume
- switch song
- ended song
- song without audio
- admin-hidden song
- song inside admin-hidden album

Do not expose direct MP3 paths.

Do not bypass backend stream visibility rules.

If a global visual player exists, keep it consistent across UserDashboard pages.

---

## Artist Dashboard

Review:

- Index
- MySongs
- CreateSong
- EditSong
- MyAlbums
- AlbumDetails
- CreateAlbum
- EditAlbum
- ProfileSettings

Check:

- publication badges
- scheduled controls
- MP3 upload
- current audio state
- admin moderation warnings
- album/song forms

Artist must never receive a control that removes admin moderation.

---

## Admin Dashboard

Review:

- Index
- Users
- Artists
- Songs
- Albums

Check:

- search
- filters
- tables
- status badges
- moderation controls
- moderation reason fields
- responsive tables

Do not reintroduce User → Artist promotion.

---

## Moderation UI

Direct song moderation and album-derived moderation must be distinguishable.

Examples:

Admin tarafından gizlendi

and

Albüm nedeniyle gizli

Publication status must not imply access if admin moderation blocks the content.

---

## Forms

Prefer existing Razor helpers:

asp-for
asp-action
asp-controller
asp-validation-for
asp-validation-summary

File upload forms must keep:

enctype="multipart/form-data"

Show validation near the relevant field.

---

## CSS

Use current project conventions.

Avoid excessive:

- gradients
- shadows
- glow
- animation
- blur
- decorative texture

Do not add global CSS rules that unexpectedly affect unrelated pages.

Scope page-specific CSS carefully.

---

## JavaScript

Use JS only where interaction needs it.

Avoid:

- duplicate handlers
- unnecessary globals
- duplicated backend rules
- unnecessary inline JS

Do not introduce new browser console errors.

---

## Content Stress Tests

Where useful, test:

- long song title
- long album title
- long artist name
- long moderation reason
- many genres
- long email

UI must not break.

---

## Empty States

Where possible review:

- no songs
- no albums
- no results
- no liked songs
- no followed artists
- no listening history

Empty pages should still feel intentional.

---

## Final Frontend Verification

After frontend work:

1. Run:

dotnet build

2. Check browser console.
3. Check obvious 404s.
4. Check obvious 500s.
5. Verify key navigation.
6. Verify important forms.
7. Verify responsive layout.
8. Verify audio playback if player-related code changed.

Report briefly:

- pages reviewed
- frontend files changed
- visible issues fixed
- screenshot rounds
- screenshot directory
- responsive result
- browser console result
- build result
- remaining UI issues
Do not commit or push unless explicitly requested.
