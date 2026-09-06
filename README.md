# Hustle Through — Offline Single-Player Build (v2 architecture)

**This is a pivot from the earlier server-backed version.** Since the game is
launching as single-player with no player-vs-player competition, the entire
backend (Fly.io, Postgres, hosting cost, "server's down" risk) is gone. The
game now runs entirely on-device — Windows, macOS, Android, iOS, all from one
Unity project with zero ongoing hosting cost.

## What changed and why

The story/career-progression loop never actually needed a server — that
architecture only earned its keep for the real-time PvP "rob/kill/kidnap
other players" idea from earlier in this project, which isn't in scope right
now. Removing it removes:

- Fly.io hosting bills
- Postgres/Redis to manage
- The single point of failure of "is the server up right now"
- Login/account friction before a player can even start playing

The trade-off, stated plainly: progress lives on the device. No cross-device
sync, no cloud backup, unless you add one later (see "Later" section below).
For a single-player game where value comes from story depth rather than a
live economy, that's the right trade.

## What's in this repo now

- **`/client-unity/Assets/Scripts`** — the full offline game logic:
  - `Data/` — loads bundled JSON (`Resources/Data/jobs.json`,
    `Resources/Data/ranks.json`) that ships inside the app itself
  - `Local/` — `LocalSaveManager` (JSON save file via
    `Application.persistentDataPath`, works on every platform automatically)
    and `GameBootstrap` (wires everything up on launch)
  - `Progression/` — `PlayerProgress`, the only code path allowed to change
    rank/level, exactly mirroring the old server-side rule but running
    on-device
  - `JobRack/` — `JobRackManager`, reads the local job pool instead of
    calling an API
  - `Economy/` — currency display, reads local state directly
  - `IAP/` — `IAPManager` + `StoreCatalog`: real-money purchases still work
    (this is your monetization, unchanged in intent), using Unity IAP's
    on-device receipt obfuscation instead of server-side verification (see
    the honesty note in `IAPManager.cs` about that trade-off)
- **`/future-online-backend`** — the old Node/Postgres backend, kept as
  reference. Nothing in the current game touches it. If you ever add
  leaderboards, cloud save, or real multiplayer later, this is a running
  head start rather than starting over.
- **`/docs`** — design docs + logo, unchanged.

## Building and playing this with only a phone + GitHub — no desktop needed

This is a real path, confirmed against Unity's current CI activation method
(no manual license file upload needed anymore — just account credentials as
secrets). It gets you an actual installable, playable **Android APK**
without ever touching Unity Editor's GUI. Windows/macOS/iOS can also be
*built* this way, but can't be *played* without those respective devices —
that part is a hard platform limitation, not something GitHub can fix.

**Step-by-step (corrected — Personal license needs a one-time manual
activation exchange, not just email/password):**

1. **Create a free Unity ID** at `id.unity.com` if you don't already have
   one — if you signed up via Google, set a real password too (Unity's
   login screen → "Reset your password" with your email works even for
   Google-linked accounts).
2. **Add two secrets first**: repo → Settings → Secrets and variables →
   Actions → add `UNITY_EMAIL` and `UNITY_PASSWORD`.
3. **Run workflow "1. Request Unity Activation File"** (Actions tab → Run
   workflow). When it finishes, download the `unity-activation-file`
   artifact — it's a small `.alf` file.
4. **Exchange it on Unity's site**: go to `license.unity3d.com/manual` in
   your phone browser, log in, upload the `.alf` file, download the `.ulf`
   file it gives you back.
5. **Add a third secret**: open that `.ulf` file (it's plain text), copy
   its entire contents, add as a new GitHub secret named `UNITY_LICENSE`.
6. **Run "2. Bootstrap Unity Project"** (Actions tab → Run workflow) — now
   that all three secrets exist, this generates the missing
   `ProjectSettings/` folder and commits it back automatically.
7. **Run "3. Unity Multi-Platform Build"** — builds Android, Windows, macOS.
8. **Download the Android build artifact**, unzip on your phone, enable
   "install unknown apps" for your file manager/browser, tap the `.apk` to
   install and play — a real build of the actual game.

**Being honest about risk here**: this is now cross-checked against
GameCI's own documentation and a real confirmed bug report matching your
exact first error, so it should be right — but I still can't run it myself
to guarantee it end-to-end. If step 3, 6, or 7 fails, the Actions log will
say why — screenshot it and we'll fix it from there.



1. Install Unity Hub + Unity Editor (2022 LTS or newer), 2D or 2.5D URP
   template.
2. Copy `client-unity/Assets/Scripts` and `client-unity/Assets/Resources`
   into your new Unity project's `Assets` folder.
3. Install the **Unity IAP** package via Window → Package Manager →
   `com.unity.purchasing`.
4. Create a `Bootstrap` scene with an empty GameObject holding
   `GameBootstrap.cs`, set `firstSceneName` to whatever your main
   menu/gameplay scene is called, and make it the first scene in
   Build Settings.
5. Build target: File → Build Settings → pick Windows, macOS, Android, or
   iOS. Each just works once the project builds — no server config needed.

## Content status: 150 launch levels + 100 reserve (corrected)

**Important correction from an earlier pass:** levels 1–70 are now the
*actual* hand-authored story from `docs/Hustle_Through_Level_Design.md` —
every named job, the DeadmanXXXII/V00D00 arc, both hacking missions, the
Dev Mission/Deploy-a-Defender four-parters, the Ayr family thread, and the
finale ("The Rack Has No More Cards") at exactly level 70 as designed. An
earlier version of this content wrongly overwrote levels 6–70 with generic
placeholder text — that's fixed now. The rank ladder also now correctly
matches the real design: Head Hustler is earned at **level 70**, not
stretched out to 150.

**`jobs.json` now contains 263 entries total:**
- **Levels 1–70**: the real story spine, hand-authored, matching your doc exactly.
- **13 bonus-pool jobs** (repeatable side content): the original 5 (Window
  Wangle, Bank Holiday Bouncer, etc.) plus 8 new ones split by theme and
  placed at the tier they actually fit tonally:
  - **Graffiti** (2 jobs) — Rookie/Younger tier, alongside the existing
    "Colours on the Wall" story level.
  - **Heists** (3 jobs) — Ganger through Trapper tier, alongside the
    existing story heists (Warehouse Word, House Always Loses).
  - **Political assassination contracts** (3 jobs, fictional public
    figures only) — Gangster through Second-in-Command tier, matching the
    established absurd-escalation tone (space station sabotage, black site
    break-ins). Framed as crew business, not real-world content.
- **Levels 71–250 — "Head Hustler Empire Era"**: since the real story
  closes definitively at level 70 ("no further cash payout — rank and story
  closure are the reward," per your own design), levels beyond that are
  framed as a distinct post-finale epilogue arc rather than more rank
  progression — the player has already earned Head Hustler, and this is
  what running the empire looks like afterward. Content mix: heists,
  political-assassination contracts, empire-branding callbacks to the
  graffiti thread, and crew-management jobs, roughly in that proportion.
  Levels 71–150 ship at launch; 151–250 sit in reserve
  (`releaseBatch: "reserve"`) until you flip
  `GameDataLoader.ReserveContentUnlocked = true`.

**Honesty on quality**: levels 1–70 and all 13 bonus jobs are genuinely
hand-authored. The 180 Empire Era levels (71–250) are still templated —
structurally sound, correctly categorised and placed, but written from
templates rather than by hand. That's an enormous amount of content to
author individually; the practical move is treating a handful of Empire Era
levels as milestone set-pieces worth a real writing pass (e.g. every 10th
level) while leaving the rest as functional filler between them, same
pattern as most open-world games handle side content at this scale.

**Rank ladder correction**: `ranks.json` now unlocks Head Hustler at level
70 (matching the real finale) instead of 150. Empire Era content (71–250)
sits under the already-earned Head Hustler rank — no new ranks invented,
no re-earning anything.

## Monetization, still intact

The "no IAP touches rank" rule is preserved exactly: `IAPManager` only ever
calls `PlayerProgress.CreditNotes()`, and nothing else in the codebase can
credit Notes. Rank/level only ever change through `PlayerProgress.CompleteJob()`.
Extend `StoreCatalog.cs` with more cosmetics as the tier-based art direction
(desaturated Rookie → neon Ganger → cold Gangster-plus) comes together.

## Full monetization design (free download + freemium, built to fund expansion)

The catalog below is designed around one rule: **every revenue lever here is
something a player can also just... not do, and lose nothing gameplay-wise.**
No ads or purchases are required to finish the story or reach Head Hustler —
they're all speed/cosmetic/support options layered on top of a genuinely
complete free game (150 launch levels + 13 bonus jobs, for £0).

### 1. Notes packs (real money → hard currency)
Five tiers instead of three, so both impulse buyers and bigger spenders have
a natural next step:

| Product | Notes | Suggested price |
|---|---|---|
| `notes_pack_small` | 100 | ~£0.99 |
| `notes_pack_medium` | 550 | ~£4.49 |
| `notes_pack_large` | 1200 | ~£8.99 — flagged "Best Value" in UI |
| `notes_pack_mega` | 2600 | ~£17.99 |
| `notes_pack_whale` | 7000 | ~£39.99 |

No pack is a "bad deal" — bigger packs just have a modestly better
Notes-per-£ ratio, which is honest anchoring rather than manipulative
(compare to games that make the small pack deliberately terrible value to
pressure bigger spend — we don't do that here).

### 2. Cosmetic/convenience catalog (Notes → items)
12 items across three visual tiers matching the Game Bible's art direction
(Gold/Rookie-Player, Platinum/Ganger-Hustler, Diamond-Enhanced/Gangster-Head
Hustler) — rack skins, mask variants, car cosmetics, crew colours, plus
universal convenience items (retry tokens, rack refreshes, sold individually
or in discounted 5-packs). Full list and prices in `StoreCatalog.cs`. Every
item is priced so a single small Notes pack buys at least one thing outright.

### 3. Founder's Pack (one-time, new-player-only)
`founders_pack`, ~£3.99: 400 Notes + an exclusive mask + ads removed,
bundled well below buying the equivalent separately. Only offered before
Rank 3 (`IAPManager.IsFoundersPackEligible()`) — a genuine new-player
welcome offer, not something available to farm repeatedly or something a
returning player gets pushed into buying twice.

### 4. Remove Ads (one-time)
`remove_ads`, ~£2.99. Standard, well-liked model — removes interstitial/
banner-style ads if you add them later. Does **not** remove the option to
watch a rewarded ad for Notes (see below) — that stays available to
everyone since it's opt-in and benefits the player either way.

### 5. Rewarded video ads (the free path — likely your biggest single revenue line)
`RewardedAdManager.cs` — player opts in, watches an ad, gets Notes (15 by
default, on a 60-second cooldown to prevent spam-farming). This is a stub
ready to wire into a real ad network (Unity LevelPlay, AdMob, etc.) — see
the TODO comment in the file. For a free-download game, rewarded ads
typically outperform direct IAP revenue by volume, precisely because
they're free to the player and don't require a card. This is also the
fairest possible monetization lever: the player actively chooses to trade
30 seconds for a small reward, gets it every time, and it costs them
nothing.

### Why this combination, not just "more IAP"
A free download's revenue realistically comes from a mix, not one lever:
rewarded ads convert almost everyone a little; the Founder's Pack converts
new players once at a good rate; Notes packs convert a smaller % of
engaged players repeatedly; Remove Ads converts players who'd rather pay
once than watch anything. Together, this is a realistic shot at funding
the reserve 100 levels and beyond without ever touching pay-to-win — which
also protects the thing that makes the game worth returning to in the
first place.

## Payments — what's real vs what's left to configure

The `IAPManager` code already talks to Unity IAP, which routes to the real
Apple/Google purchase flow — this is not a simulation, it's the actual
mechanism every paid mobile app uses. What's still needed on your end,
outside of code:

1. **App Store Connect** (Apple) and **Google Play Console** (Google)
   developer accounts — this is where you create the actual sellable
   products (`notes_pack_small`, `notes_pack_medium`, `notes_pack_large` —
   IDs must match exactly what's in `IAPManager.cs`).
2. Set real prices per product in each console.
3. Test with **sandbox accounts** (Apple) and **licence testers** (Google)
   before going live — both platforms give you fake-money test purchases so
   you can confirm the flow end-to-end before real money is involved.
4. One important nuance: a purchase briefly needs the device to be online to
   contact Apple/Google's payment servers — this is universal across every
   app on both platforms and is unrelated to whether *your* game needs a
   backend. The game itself keeps working fully offline; only the moment of
   payment needs connectivity, exactly like buying anything through the App
   Store or Play Store.
5. **Stronger fraud protection later**: right now, purchases are validated
   on-device (Unity IAP's receipt obfuscation). The stronger version —
   server-side receipt verification against Apple/Google directly — is
   already built and sitting in `/future-online-backend/src/services/billingService.ts`
   and `receiptValidation.ts`. Once the game is making real money, reconnecting
   `IAPManager` to that backend (rather than validating locally) is the
   natural next step — see "Later" below.

## Later, if/when it makes sense

- **Cloud save** (not full multiplayer): Google Play Games Services and
  Apple Game Center both offer free cloud save hooks that sync the local
  JSON save across a player's devices — no backend of your own required.
- **Leaderboards or light social features**: could plug into the same
  platform services above before ever needing a custom server again.
- **Real-time PvP**: if this ever comes back into scope, `/future-online-backend`
  plus the earlier architecture doc (`Hustle_Through_Tech_Architecture.md`)
  is where that conversation picks back up — dedicated authoritative servers,
  session-based matchmaking, the whole thing. Not needed for where the game
  is now.
