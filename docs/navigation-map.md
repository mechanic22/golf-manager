# GolfManager Web Navigation Map

## Router and Layouts

- Router entry: `src/GolfManager.Web/App.razor`
- Default layout from router: `MainLayout`
- `MainLayout` renders `NavMenu` and page body
- Most authenticated app pages explicitly set `@layout AppLayout`, which replaces `NavMenu` with context-aware app navigation

## Layout Roles

### Public/marketing layout (`MainLayout` + `NavMenu`)
Used by pages without `@layout AppLayout`.

Top nav links:
- `/`
- `/icons`
- `/login` or `/dashboard` (depending on auth)
- `/register`

### App layout (`AppLayout`)
Used by dashboard, leagues, seasons, profile, admin pages.

Context-aware nav — see `context-nav.md` for the full detection logic and per-context link sets.

## Route Inventory

### Public/Auth
- `/` — Home/marketing (anonymous) or redirect to `/dashboard` (authenticated)
- `/login`
- `/register`
- `/league/{LeagueKey}/guest` — Guest password entry for a specific league
- `/icons` — Icon reference page
- `/not-found`
- `/access-denied`

### Dashboard and general
- `/dashboard`
- `/events`
- `/events/create` — Global admin only
- `/event/{EventKey}` — Event detail (results, teams, description)
- `/event/{EventKey}/manage` — Event management (organizer only)
- `/leagues`
- `/leagues/create`
- `/organizer-dashboard`
- `/guest-standings` — Alias for guest standings (redirects to league guest view)

### League
- `/league/{LeagueKey}` — League overview (Dashboard tab)
- `/league/{LeagueKey}/dashboard` — Alias for above
- `/league/{LeagueKey}/members`
- `/league/{LeagueKey}/seasons`
- `/league/{LeagueKey}/settings` — League admin only
- `/league/{LeagueKey}/player/{PlayerId}` — Individual player profile within league

### Season
All season routes share the `SeasonLayout`. The `/{Tab}` suffix drives the active tab.

- `/league/{LeagueKey}/season/{SeasonKey}` — Defaults to overview tab
- `/league/{LeagueKey}/season/{SeasonKey}/overview`
- `/league/{LeagueKey}/season/{SeasonKey}/events`
- `/league/{LeagueKey}/season/{SeasonKey}/standings`
- `/league/{LeagueKey}/season/{SeasonKey}/players`
- `/league/{LeagueKey}/season/{SeasonKey}/teams`
- `/league/{LeagueKey}/season/{SeasonKey}/settings` — League admin only

### Season event sub-pages (admin-only unless noted)
- `/league/{LeagueKey}/season/{SeasonKey}/event/{EventKey}/scores` — Score entry (admin only)
- `/league/{LeagueKey}/season/{SeasonKey}/event/{EventKey}/golfers` — Golfer management (admin only)
- `/league/{LeagueKey}/season/{SeasonKey}/event/{EventKey}/matchups` — Matchup/pairing setup (admin only)
- `/league/{LeagueKey}/season/{SeasonKey}/event/{EventKey}/scorecards` — Printable scorecards (currently admin only; see `page-content-by-role.md` for recommended change)

### Profile
- `/profile`
- `/profile/stats`
- `/profile/rounds`
- `/profile/handicap`

### Admin (global admin only)
- `/admin`
- `/admin/users`
- `/admin/leagues`

## Auth and Redirect Behavior

- Most app pages gate on `AuthService.IsAuthenticated` and navigate to `/login` if not authenticated
- `App.razor` initializes auth before rendering routes
- `NavMenu` attempts custom-domain league context bootstrapping after first render
- Guest users (`AuthService.IsGuest`) can only access their specific league's guest standings; any other league URL redirects them to that league's `/guest` page

## AppState Navigation Context

`AppState` tracks:
- current league key / id / name
- league admin flag (`IsCurrentLeagueAdmin`)
- custom-domain → league-key map

League key normalization: lowercase + trimmed in `SetCurrentLeague`.

## Known Issues

- **`/players`** — referenced in an older nav block but no page exists. Should be removed from any remaining nav references.
- **Header nav duplicates in-page tabs** at league and season context — see improvement item #1 in the plan.
