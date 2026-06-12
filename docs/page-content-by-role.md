# Page Content by Role

What each role can see and do on each page. Role hierarchy from lowest to highest:

| Role | How acquired |
|---|---|
| **Anonymous** | Not logged in |
| **Guest** | Guest password login via `/league/{key}/guest` |
| **Viewer** | `LeagueMemberRole.Viewer` — league member, view-only (⚠ not yet distinctly gated in UI — treated same as Member) |
| **Member** | `LeagueMemberRole.Member` — standard league golfer |
| **Admin / Owner** | `LeagueMemberRole.Admin` or `.Owner` — both treated as "league admin" throughout the codebase |
| **Organizer** | Any authenticated user who created a specific event (`event.OrganizerId == currentUserId`) |
| **Global Admin** | `IsGlobalAdmin` flag — system-wide, set via `/admin/users` |

"League admin" in code means `Owner || Admin`. `CanManageLeague` = Global Admin OR League Admin.

---

## Public pages

### `/` (Home)
| Role | Sees |
|---|---|
| Anonymous | Marketing hero, features, "Get Started" + "Sign In" CTAs |
| Authenticated | "Go to Dashboard" CTA only (same page, auth-aware buttons) |

### `/login`, `/register`
Open to everyone. Authenticated users are immediately redirected to `/dashboard`.

### `/league/{key}/guest`
Open to everyone. Authenticated non-guest users are redirected to `/league/{key}` directly.

---

## Dashboard

### `/dashboard`
| Role | Sees |
|---|---|
| Member | League cards with active season info and next event date |
| League Admin | Same + "Admin" badge on their league cards |
| Global Admin | Same + "Admin Dashboard" link in user menu |

---

## Events

### `/events`
| Role | Sees |
|---|---|
| Any authenticated | Full event list |
| Global Admin | + "Create Event" button |

### `/event/{key}`
| Role | Sees |
|---|---|
| Any authenticated | Event details, results, teams, description |
| Organizer | + "Manage Event" button linking to `/event/{key}/manage` |

### `/event/{key}/manage`
Organizer only. Other authenticated users can navigate here but have no edit controls.

---

## League pages

### `/league/{key}`
| Role | Sees |
|---|---|
| Member / Viewer | Dashboard, Seasons, Members tabs |
| Admin / Owner / Global Admin | + Settings tab |

### `/league/{key}/members`
| Role | Sees |
|---|---|
| Any league member | Full member list with roles |
| Admin / Owner / Global Admin (`CanManageLeague`) | + Edit role, remove member controls |
| Owner / Global Admin only | + Promote/demote Owner role control |

### `/league/{key}/seasons`
All league members. No role differences in content.

### `/league/{key}/settings`
Admin / Owner / Global Admin only. Others are redirected to `/access-denied`.

### `/league/{key}/player/{PlayerId}`
Any authenticated league member.

---

## Season pages

### `/league/{key}/season/{key}` (tabs)

| Tab | Who can see | Admin extras |
|---|---|---|
| Overview | All members | — |
| Events | All members | — |
| Players | All members | + Add Player, Remove, Mark Paid/Unpaid controls |
| Teams | All members (read) | + Add Team, Edit, Delete, Assign Players |
| Settings | Admin / Owner / Global Admin only | Full settings form |

### `/league/{key}/season/{key}/event/{key}/scores`
League Admin / Owner / Global Admin only. Other authenticated users see an "Access Denied" card.

### `/league/{key}/season/{key}/event/{key}/golfers`
League Admin / Owner / Global Admin only.

### `/league/{key}/season/{key}/event/{key}/matchups`
League Admin / Owner / Global Admin only.

### `/league/{key}/season/{key}/event/{key}/scorecards`
Currently: League Admin / Owner / Global Admin only — shows "Access Denied" for members.
**Recommended change:** Open to all league members (`CanViewSeason`). The content (blank paper scorecards with matchup assignments) is not sensitive and is exactly what a golfer needs before their round.

---

## Profile

### `/profile`, `/profile/stats`, `/profile/rounds`, `/profile/handicap`
Any authenticated user. Always shows the current user's own data — no cross-user viewing here.

---

## Guest standings

### `/guest-standings`, `/league/{key}/guest` (standings view)
Guest session or any authenticated user. View-only standings for one league. No access to any other pages; nav shows only Home / Events / Standings.

---

## Admin (global admin only)

### `/admin`, `/admin/users`, `/admin/leagues`
Global Admin only. All other authenticated users are redirected to `/access-denied`.

---

## Known gaps / future roles

- **`Viewer` role** exists in `LeagueMemberRole` enum but is not distinctly gated anywhere in the UI — currently treated identically to `Member`. Should be defined and enforced if read-only league access is needed.
- **`Season Admin`** and **`Moderator`** roles are referenced in `IAuthorizationService` comments (`CanManageSeason`, `CanEnterScores`) but not yet implemented.
- **Anonymous access to public league data** — currently no league/season/event content is readable without at least a guest session. See `context-nav.md` and the flow improvement notes for options.
