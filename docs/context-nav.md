# Context-Aware Navigation

`AppLayout` (`src/GolfManager.Web/Layout/AppLayout.razor`) renders different nav links depending on the current URL. Context is detected from `currentUrl` (the base-relative path, updated on every `LocationChanged` event).

## Detection Priority (highest wins)

```
Event context  →  URL contains "/season/" AND "/event/"
Season context →  URL contains "/league/" AND "/season/"  (and not event)
League context →  URL starts with "league/"              (and not season)
Admin context  →  URL starts with "admin"
Guest context  →  AuthService.IsGuest == true
Default        →  everything else
```

Detection properties in code (`AppLayout.razor`):
```csharp
IsInEventContext  = currentUrl.Contains("/season/") && currentUrl.Contains("/event/");
IsInSeasonContext = currentUrl.Contains("/league/") && currentUrl.Contains("/season/") && !IsInEventContext;
IsInLeagueContext = currentUrl.StartsWith("league/") && !currentUrl.Contains("/season/");
IsInAdminContext  = currentUrl.StartsWith("admin");
```

## Nav Links per Context

| Context | Trigger URL pattern | Nav links shown |
|---|---|---|
| **Guest** | `IsGuest == true` | Home · Events · Standings (league-scoped) |
| **Event** | `.../season/.../event/...` | ← Events · Score Entry* · Golfers* · Matchups* · Scorecards |
| **Season** | `.../league/.../season/...` | ← [League Name] · Events · Standings · Players · Teams |
| **League** | `league/...` (no season) | ← Dashboard · Overview · Seasons · Members · Settings* |
| **Admin** | `admin/...` | Dashboard · Users · Leagues |
| **Default** | anything else | Dashboard · Events · Profile |

\* Admin/Owner only

> **Known issue:** League and season context nav links duplicate the in-page tabs exactly. The recommended fix is to reduce league and season context to a single back/breadcrumb link and let the tabs own within-section navigation. See improvement item #1 in the plan.

## URL Builders

The nav uses helper methods to extract the league/season keys from the current URL by splitting on `/`:

- `GetLeagueUrl()` — `/league/{parts[1]}`
- `GetSeasonUrl()` — `/league/{parts[1]}/season/{parts[3]}`
- `GetSeasonEventsUrl()`, `GetSeasonStandingsUrl()`, etc. — appends the tab suffix

These rely on the URL having a consistent segment structure. Custom-domain URLs are normalized by `NavMenu` bootstrapping before the app context is set.

## AppState and League Selector

When the user has multiple league memberships, a league selector dropdown appears in the header. Changing it calls `AppState.SetCurrentLeague(leagueKey)`, which:
1. Updates `CurrentLeagueKey`, `CurrentLeagueName`, `IsCurrentLeagueAdmin`
2. Fires `OnChange` → `AppLayout` re-renders with the new league context

League keys are normalized (lowercase, trimmed) in `SetCurrentLeague`.

## Custom Domain Bootstrapping

On first render, `NavMenu` checks if the current host matches a known custom domain (e.g. `digikeygolf.com`) and sets the league context automatically via `AppState`. This means users on a custom domain land directly in league context without navigating through `/dashboard`.
