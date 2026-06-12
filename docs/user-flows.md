# User Flows

Key journeys through the app by persona. Each flow lists the pages visited and the role required.

---

## Flow A — New league admin sets up a league

**Role required:** Any authenticated user (becomes Owner on league creation)

1. `/register` — Create account
2. `/dashboard` — See empty state; click "Create League"
3. `CreateLeagueDialog` — Enter league name/key; submits → `/league/{key}`
4. `/league/{key}` → **Seasons tab** → "Add Season" → create season
5. `/league/{key}/season/{key}` → **Players tab** → "Add Player" for each golfer
6. `/league/{key}/season/{key}` → **Teams tab** → create teams, assign players
7. `/league/{key}/season/{key}` → **Events tab** → create event, set date/course/tee

---

## Flow B — Season player checks standings and upcoming events

**Role required:** Authenticated league member (Member or above)

1. `/login`
2. `/dashboard` — League cards show active season and next event date
3. Click league card → `/league/{key}` → **Dashboard tab** (overview, recent activity)
4. Click season → `/league/{key}/season/{key}` → **Players tab** (standings + roster)
5. Switch to **Events tab** → see upcoming and past events
6. Click an event → `/event/{key}` → view results, teams, description

---

## Flow C — League admin enters scores after a round

**Role required:** League Admin or Owner

1. `/league/{key}/season/{key}` → **Events tab**
2. Click the event for that week
3. Navigate to **Score Entry**: `/league/{key}/season/{key}/event/{key}/scores`
4. Enter raw scores per player via `ScoreEntryRow` components
5. Save — `EventScoringService` rebuilds the scoreboard and updates team standings
6. Navigate to **Scorecards** to print blank cards for the next round (all members can access this)

---

## Flow D — Guest views league standings (no account)

**Role required:** None — guest password only

1. Admin shares link: `/league/{key}/guest`
2. Guest enters the league password → receives a guest session token
3. Redirected to `/league/{key}` → view-only standings page
4. No access to any other pages; nav shows only Home / Events / Standings
5. "Sign In" button available to upgrade to a full account

---

## Flow E — Global admin manages users and leagues

**Role required:** Global Admin (`IsGlobalAdmin` flag)

1. `/dashboard` → click "Admin Dashboard" in user menu
2. `/admin` → overview
3. `/admin/users` → search, edit roles, set/remove global admin flag
4. `/admin/leagues` → view all leagues, manage league-level settings
