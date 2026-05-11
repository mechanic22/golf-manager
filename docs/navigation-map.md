# GolfManager Web Navigation Map

## Router and Layouts

- Router entry: `src/GolfManager.Web/App.razor`
- Default layout from router: `MainLayout`
- `MainLayout` renders `NavMenu` and page body
- Most authenticated app pages explicitly set `@layout AppLayout`, which replaces `NavMenu` with context-aware app navigation

## Layout Roles

### Public/marketing layout (`MainLayout` + `NavMenu`)
Used by pages without `@layout AppLayout`.

Typical top nav links:
- `/`
- `/icons`
- `/login` or `/dashboard` (depending on auth)
- `/register`

### App layout (`AppLayout`)
Used by dashboard, leagues, seasons, profile, admin pages.

Context-aware nav sections:
- Default context: Dashboard, Events, Players
- League context (`/league/{LeagueKey}`): Dashboard, Seasons, Members
- Season context (`/league/{LeagueKey}/season/{SeasonKey}`): Back to League, Events, Standings, Players
- Admin context (`/admin*`): Dashboard, Users, Leagues

## Route Inventory

### Public/Auth
- `/`
- `/login`
- `/register`
- `/icons`
- `/not-found`

### Dashboard and general
- `/dashboard`
- `/events`
- `/events/create`
- `/event/{EventKey}`
- `/event/{EventKey}/manage`
- `/leagues`
- `/leagues/create`

### League
- `/league/{LeagueKey}`
- `/league/{LeagueKey}/dashboard`
- `/league/{LeagueKey}/members`
- `/league/{LeagueKey}/seasons`
- `/league/{LeagueKey}/settings`
- `/league/{LeagueKey}/player/{PlayerId}`

### Season
- `/league/{LeagueKey}/season/{SeasonKey}`
- `/league/{LeagueKey}/season/{SeasonKey}/overview`
- `/league/{LeagueKey}/season/{SeasonKey}/events`
- `/league/{LeagueKey}/season/{SeasonKey}/standings`
- `/league/{LeagueKey}/season/{SeasonKey}/players`
- `/league/{leagueKey}/season/{seasonKey}/settings`
- `/league/{LeagueKey}/season/{SeasonKey}/event/{EventKey}/scores`

### Profile
- `/profile`
- `/profile/stats`
- `/profile/rounds`
- `/profile/handicap`

### Admin
- `/admin`
- `/admin/users`

## Auth and Redirect Behavior

- Most app pages gate on `AuthService.IsAuthenticated` and navigate to `/login` if not authenticated
- `App.razor` initializes auth before rendering routes
- `NavMenu` attempts custom-domain league context bootstrapping after first render

## AppState Navigation Context

`AppState` tracks:
- current league key/id/name
- league admin flag
- custom-domain to league-key map

League key normalization is lowercase/trimmed in `SetCurrentLeague`.

## Known Navigation Gaps

These links currently exist in `AppLayout` but matching routes are not present:
- `/players`
- `/admin/leagues`

If desired, these should either be implemented as pages or removed/redirected.
