# Simon Duel — Unity Build Plan

**Status:** Draft 0.1 — planning only, nothing built  
**Date:** 25 October 2026  
**Reads with:** [GDD](GDD.md), [Art Bible](ART_BIBLE.md), [Market and Creative Strategy](MARKET_AND_CREATIVE_STRATEGY.md)  
**Owner picks (26 Oct 2026):** A1 one phone · B3 live and waiting in one match · C1 friends by link · D1 Rivalries, D2 Sparks and collection, D6 Badges and level · E1 everyone, kids included.

## Short version

- One Unity game, three ways to play: against the computer, two players on one phone, and online with friends.
- **Browser first** (owner, 3 Nov 2026): the game is built and tested as a web page that opens from a link on any phone or computer; the iPhone and Android apps follow from the same project after the checkpoint. Company: CuzGames.
- **Computer before phone** (owner, 4 Nov 2026): steps 1–6 are built and tried in a computer browser with mouse and keys. Phone browsers follow as their own step before the checkpoint, including re-checking speeds for thumbs.
- **Widescreen on computer** (owner, 4 Nov 2026, layout W1 "Face-off"): one shared board in the middle, a player panel on each side, laid out at 1920 × 1080 and filling the window. The Art Bible's sizes are for the upright phone layout, which is built in the Phone browser step.
- The rules are written once and shared by all three, so online can never play differently from the phone version.
- Each turn is played entirely on one phone; only the result travels. A weak connection never costs anyone a turn.
- Build order: the four pads → the plain duel → Twists → the computer → a checkpoint with real players → the look → all screens → meta → money and permissions → online → store.
- Art lives in the owner's Figma. Colors and sizes are copied into the game in one step, so Figma stays the source.
- Meta is small and cosmetic: rivalries, Sparks and a collection, badges and a level. Daily pattern, a computer tour and ranked seasons are not planned unless the numbers ask for them.
- Money: ad breaks after every second match, optional reward videos, a "No ads + starter looks" pack and look packs. About US$1,230 a month per 1,000 phones played daily in the US, about US$260 worldwide. Growth has to come from free sharing; bought installs don't pay back yet.
- Costs: Unity free under US$200K a year; online about US$0–60 a month at 10,000 monthly players; Apple US$99 a year, Google US$25 once; a web address for invite links, about US$10–20 a year.
- Still open: the public name (Simon is Hasbro's brand) and how hard the game sells (board choice F).

---

## 1. What we are building

| Mode (owner's word) | What the player does | Needs internet | Built in step |
|---|---|---|---|
| Single player | Duel against the computer (Easy, Medium, Hard). Practice: Classic, Endless, Twist practice, sight-only and sound-only. | No | 6 |
| PvP offline | Two players pass one phone. Later: Party, 3–8 players, elimination. | No | 3, then later |
| Multiplayer | Play a friend by link or code; live when both are in the game, waiting with a notification when not. No random players. | Yes | 11 |
| Meta | Rivalries, Sparks and collection, badges and level. | Partly | 9 |

"PvP offline" means one phone passed between players (owner pick A1). Two phones linked without internet is not planned.

## 2. Gaps and conflicts in the design docs

Each conflict becomes a named setting, so it is settled by playing, not by arguing. Defaults below are the starting values.

| Topic | GDD | Strategy doc | Start with | Settled by |
|---|---|---|---|---|
| Match length | Best of 5 | Best of 3 | Best of 3 (fits the 60–180 s target) | Playtest (setting) |
| Twists in the first playable | Reverse, Shift | Reverse, Shift, Echo | Reverse and Shift; Echo next | Setting |
| Using a Twist | Armed automatically after Add One | Player may choose to arm | Player chooses with one tap; skipping keeps it | Strategy Test C |
| Fastest playback | Floor 0.32 s light + 0.10 s gap | "Fast" 0.25 s + 0.10 s | GDD floor | Playtest (setting) |
| Charge between rallies | Not stated | Not stated | Resets each rally | Setting |
| Pads after a Shift turn | Not stated | Not stated | Slide home during the hand-over | Playtest |
| Start of a turn | Ownership banner, then playback | Same | Owner, 4 Nov 2026: no click before a turn; the banner and ring sweep run about a second, then the pattern plays. Re-check for passing a phone | Playtest |
| Computer opponent | Not in first playable | "Simple bot fallback" | In: the owner wants single player | Owner |
| Online | Later | Only after local proves out | After the checkpoint, at step 11 | Owner |
| Leaving the app | Pauses and hides the pattern | — | Same for offline. Online turns cannot pause: leaving during input counts as a timeout after a 10 s grace | Playtest |
| Blackout online | Unavailable if audio and haptics are both off | — | The server stores whether the receiving player can get Blackout | — |

Art references that no longer match the docs (redraw in Figma; the references are mood only):

- The motion sheet still shows **Shuffle** with pads crossing diagonally. The rule is now **Shift**: every pad moves one slot clockwise.
- The PNG references, `simon-duel-layout-spec.svg` and `simon-duel-palette.svg` still draw **white symbols on Coral and Electric**. The contrast audit made Dark Ink `#07142B` authoritative.
- The Twist icons in the references (lightning, shield, plus-dots, crossing arrows) do not map to the five Twists. Redraw Reverse, Shift, Echo, Rush, Blackout.
- The visual-language sheet shows the **classic round four-quadrant board**, which the strategy doc says to avoid for trade-dress reasons.

## 3. How the game is put together

**In plain words: one referee, several seats.** The rules live in a referee that knows nothing about screens, sounds or the internet. Each side of a match sits in a seat: a person tapping this phone, the computer, or a friend far away. The referee asks the seat for moves and announces what happened; the board, sounds and buzz react to the announcements.

What that buys:

1. Every mode plays by exactly the same rules.
2. The rules are tested automatically without opening the app.
3. The online server runs the same referee code to check each turn.
4. Every match is a seed plus a short list of moves, so it replays exactly: for "their turn, replayed", for bug reports, and for tuning.

```
Rules (plain C#, no Unity)   referee · match state · Twist bag · time budgets · computer brain · match record
      ▲ moves        ▼ announcements
Match runner (Unity)         turn phases and clocks · seats: Touch / Computer / Remote / Replay
      ▼ announcements
Presentation (Unity)         board · pads · ring · banners · effects · audio · haptics
Screens and meta (Unity)     navigation · setup · results · profile · rivalries · collection
Services (swappable)         save · online · store · analytics · notifications (offline stand-ins first)
```

### Rules core sketch

```csharp
public enum PadId { Circle, Triangle, Square, Star }        // identity, never screen position
public enum TwistId { None, Reverse, Shift, Echo, Rush, Blackout }

public sealed class Referee
{
    public Referee(RulesConfig rules, ulong seed, int players);
    public TurnPlan StartTurn();                  // expected entry (after Reverse/Echo), playback script,
                                                  // Blackout step, time budget, Shift layout
    public TapVerdict Tap(PadId pad, int msSinceInputOpened);   // Correct | Complete | Wrong(expected)
    public void TimeOut();
    public void AddStep(PadId pad);
    public void ArmTwist(bool arm);
    public MatchRecord Record { get; }            // seed + turn records; enough to replay the match
    public event Action<RefereeEvent> Announced;  // TurnStarted, StepCorrect, Slip, RallyWon, MatchWon, TwistEarned …
}
```

- **Same seed, same match.** One seeded random source per match drives the two-step rally start, the Twist bag, Echo's doubled step and the computer's choices. No `UnityEngine.Random` and no wall clock inside the rules; all times are whole milliseconds.
- **Identity, not position.** The referee only knows pad identities. Slots belong to the presentation; Shift arrives as a layout change inside the `TurnPlan`.
- **Twist mapping:** Reverse expects the sequence back to front, and the added step still goes on the end. Echo duplicates one seeded step in the expected entry. Rush shortens playback and the time budget. Blackout and Shift change only what is shown, never what is expected.
- **Everything tunable lives in settings assets**, one per feature, every limit a named number: `DuelRules`, `Timing`, `Feel`, `ComputerLevels`, `AudioMix`, `Economy`.

## 4. The modes in detail

### 4.1 Same phone (built first)

- Players type names or keep "Player 1" and "Player 2". Starting player is random, then alternates per rally; Rematch flips it.
- Between turns: ownership banner, then **"tap when ready"**, then playback (see gaps table).
- Sparks go to the phone owner's profile; typed names feed the rivalry records.
- Party (3–8 players, elimination, sequence resets after an elimination) reuses the same referee with more seats.

### 4.2 Against the computer

The computer must feel like a person, not a machine that never forgets.

- **Memory:** each step has a slip chance that rises sharply past the level's comfortable length (starting points: Easy 5, Medium 8, Hard 12). Slips cluster in the middle of the pattern, the way people remember starts and ends best. Reverse, Shift, Echo and Rush add to the chance.
- **Timing:** human-like gaps between taps (about 0.28–0.65 s), slower near its limit, with short hesitations.
- **Add One:** Easy picks at random. Medium avoids handing over easy runs. Hard builds confusing runs (A-B-A-B, doubles) and saves Twists for long patterns or the Twist the player fails most.
- **Honesty:** always labeled as the computer, including as a fallback while waiting for an online opponent.
- Every number per level sits in the `ComputerLevels` settings asset.

### 4.3 Practice

Classic (the game adds steps), Endless, Twist practice, plus sight-only and sound-only practice from the accessibility plan. Same referee, one seat.

### 4.4 Online

**Shape of a match**

1. A match lives on the server as its seed, the rules version and the list of turns.
2. Your phone plays your whole turn on its own (playback, input, timer), then sends one turn record: taps with times, the added step, the Twist armed.
3. The server runs the same referee over that record: right taps, inside the time budget, Twist allowed. Then it saves the turn and tells the other phone, instantly if they are in the game, by phone notification if not.
4. The other phone can show **their turn, replayed**, then starts its own turn.

- **Live and later are one thing.** The same match flows live while both players are present and waits while one is away (owner pick B3). Turn deadline: 24 hours, as a setting.
- **No lost turns.** A played turn stays on the phone until the server confirms it; each turn carries an ID, so sending it twice is harmless.
- **Friends:** invite link that opens the match in the app, plus a short code as a fallback. Past opponents become a friend list.
- **Invite links need a web address we own.** A tapped link opens the game if it is installed, or a small page pointing to the stores if not. That needs a domain (about US$10–20 a year) carrying the two small files Apple and Google check. Google's free link service for this closed in August 2025, so there is no free shortcut. The domain waits for the public name.
- **Accounts:** a guest account is created quietly on the first online match. Sign in with Apple or Google keeps progress across phones. Account deletion lives in Settings (both stores require it).
- **Friends only (owner pick C1).** No random players or quick match at launch. Notes kept in case the numbers ask for it later: your first turn is played now, and the next person searching picks the match up, so nobody has to be online at the same moment. The computer is offered openly while waiting: labelled, with no fake human profile, and kept out of ratings and rewards. Bots passed off as people get spotted and resented, and the EU now requires telling people when they face an AI.
- **Cheating, plainly:** nothing can stop a player writing the pattern down or filming the screen. The server rejects impossible input (for example long runs of taps under about 90 ms apart). With friends-only play there is nothing to win by it.
- **Fairness note:** the replay shows the pattern one extra time to the next player. Both players get it, so it stays even; it is a setting in case it proves too helpful.

**Service choice**

In plain words: because a turn is played whole on one phone, online needs no live-action networking, only a service that checks each finished turn, saves it and tells the other player. Pick the online services from Unity itself, plus Google's free notification service for "your turn" alerts.

| | Unity Gaming Services + Firebase notifications (pick) | Nakama, self-hosted (runner-up) |
|---|---|---|
| Turn check | Cloud Code C# modules: the same `SimonDuel.Rules` code runs on the server | Server code in Go, TypeScript or Lua: the referee would be written twice |
| Live play | Cloud Code push messages reach players who have the game open | Built-in authoritative and relayed matches |
| "Your turn" when away | Firebase Cloud Messaging (free), called from Cloud Code | Outside notification provider |
| Accounts | Guest sign-in linked later to Apple or Google; account deletion | Device sign-in linked to Apple or Google; account deletion |
| Saves, Sparks, items | Cloud Save "Protected" data only the server writes; Economy also checks Apple and Google receipts | Storage plus receipt checks with refund notices |
| Leaderboards, friends | Built in, server-written, scheduled resets; Friends free up to 50k monthly players | Built in |
| Cost at ~10k monthly players | About US$0–60 a month (free tiers: 1M Cloud Code calls, 1M Cloud Save reads and writes) | About US$30–100 a month on a small server, plus running it; Heroic Cloud hosting from about US$600 |
| Catch | High lock-in. Going over a free tier with no card on file blocks every Unity service, so a card goes on file before launch | We run, patch and back up the server ourselves |

- Not needed: Relay, Lobby or dedicated game servers. Unity's Multiplay hosting was wound down in 2026 and doesn't matter here.
- Ruled out: PlayFab (free tier capped at 1,000 players per game since March 2026), Photon (built for live action; no accounts, purchases or notifications), Epic Online Services (no server logic or phone notifications), Game Center (Apple only; Google's turn-based service closed in 2020).
- Everything sits behind the `SimonDuel.Online` interfaces, so moving to Nakama later means rewriting one adapter and porting the turn check, not the game.
- **Screen capture:** Android can block screenshots and recording during play; iOS can only detect recording. Not needed for friends-only play.

### 4.5 Age and safety

Owner pick E1 (26 Oct 2026): everyone including kids, built as a mixed-audience game and kept out of Apple's Kids section.

- **The age question comes at first launch**, because ads and analytics start with the first match (§6.4). Before 26 Oct it was planned for the first online match.
- **Under 13:** names picked from a list, preset emotes only, friends by invite link or code only, no personalised ads.
- **13 and up:** typed names through a filter, report and block.
- **Regional age checks** use Apple's and Google's own age tools where a region requires them (Texas since June 2026, Brazil).
- The COPPA amendments have applied since 22 April 2026. This is product-risk research, not legal advice: a lawyer should confirm that "8+" marketing does not make the game "directed to children".

## 5. Meta

All rewards change looks, never power (GDD §16). Owner picks (26 Oct 2026): **Rivalries**, **Sparks and collection**, **Badges and level**. Not planned unless the numbers ask: Daily pattern, Computer tour, Ranked seasons.

- **Rivalries:** your record against each person you've played (same-phone names and online friends): wins, losses, best pattern together, longest streak, and a Revenge button that starts a rematch.
- **Badges and level:** badges for moments such as "survived Reverse at length 10"; a level that grows with every finished match. Both show on the profile.

**Sparks, first numbers to tune**

| Earn | Sparks | Spend | Sparks |
|---|---:|---|---:|
| Finish a match | 20 | Crown or badge frame | 150–250 |
| Win bonus | 10 | Board look | 300–600 |
| First match of the day | 30 | Pad sound pack | 400 |
| New badge | 25 | Victory effect | 300 |

- Every look keeps the four symbols and the contrast rules; High Contrast overrides any look.
- Saved first as a local versioned file; moved to cloud save with online accounts.

## 6. Money and permissions

**What changed (26 Oct 2026):** the owner judged "free + one-time pass + optional videos" too weak to sell. This replaces the strategy doc's ad rules (§17), which allowed only optional videos and no ad before a rematch.

### 6.1 Where the money comes from

In plain words: in games like this the money comes from ads at natural breaks; purchases add a smaller share. The biggest two-player phone game (2 Player Games: The Challenge, 275M+ downloads) earns from ads plus a single US$6.99 "Remove Ads". Of the comparable games, only the ad-funded ones reached a large audience.

| Source | Rule | Starting setting |
|---|---|---|
| **Ad break** (full-screen video) | On the results screen after every second match | Never mid-rally, during a hand-over or in the first session; at most about 6 a day; never for anyone who paid |
| **Reward videos** (always optional) | Double a match's Sparks · try a locked look or sound pack for 24 hours · daily bonus · one second chance per run, vs computer and practice only | No reward videos in matches between people |
| **"No ads + starter looks"** | One-time purchase | US$4.99 |
| **Single looks and look packs** | Real-money prices. Sparks are earned by playing, never sold; no random packs | US$0.99–4.99 |
| **Season Pass** (choice F2) | Free track for everyone, paid track of new looks; monthly; 13 and up | US$4.99, from the second month |
| **VIP** (choice F3 only) | No ad breaks, double Sparks, a monthly look; 13 and up | US$4.99 a month; no weekly plans while children play |

Every number above is a remote setting, so ad gaps and prices can change without an app update.

### 6.2 What it could earn

Per 1,000 phones played each day (one phone can serve two players):

| | Mostly US players | Worldwide |
|---|---:|---:|
| Old plan (pass + optional videos) | ≈ US$21 a day · US$630 a month | ≈ US$5 a day · US$150 a month |
| F1: ad break every 2nd match + videos + no-ads pack | ≈ US$41 a day · US$1,230 a month | ≈ US$8–9 a day · US$260 a month |
| Each extra ad break per phone per day | + ≈ US$10 a day | + ≈ US$1.5 a day |
| Daily phones needed for US$10,000 a month | ≈ 8,000 | ≈ 39,000 |

- Arithmetic (US): 2 ad breaks × US$12.70 + 1 reward video × US$15.25, times 0.8 for unfilled ads and under-13 players ≈ US$32; purchases 1% × US$5 × 0.85 spread over a player's ~4.7 active days ≈ US$9.
- Ad prices: Appodeal (Q4 2024), Tenjin (2024 and 2026). Retention: GameAnalytics 2026 (median D1 22%, D7 under 4%, D30 under 1%). Most sources sell ad or tracking services; treat the numbers as ranges.
- **Each full-screen ad costs players:** 4–15% of players stop by the next day after a full-screen ad, against under 2% after a reward video. That is why the gap between ads is tuned from our own numbers.
- **Don't buy installs yet.** A bought US install earns about US$0.14–0.19 against a cost of US$1.41–6.78. Growth must come free: online invites (each one brings a new player), store featuring, short videos of real reactions, and word of mouth.

### 6.3 Who sees what, by age

One rule covers the EU and UK rules for minors, COPPA, Brazil and the US state bans on targeting teens: **ads aimed at a person's habits only for adults who agree.**

| Player | Ads | Privacy screen | Apple tracking question | Notifications |
|---|---|---|---|---|
| Under 13, or no answer | Family-certified ad companies only, no ad ID, G-rated, not personalised. Google's family rules: no full-screen ad at launch, every ad closable after 5 s | Never | Never | "Your turn" only |
| 13–17 | Not personalised | Where required, if above that country's consent age | Never | "Your turn" only |
| 18 and up | Personalised only if they agree | Where required | iPhone only, after the privacy screen, never after a refusal, never rewarded | "Your turn"; offers only if they opt in |

### 6.4 First launch, in this order

1. **Age question** before any ad or analytics code starts: month and year, nothing pre-filled, no hint of a cut-off, no instant retry. No answer counts as a child.
2. **The phone's own age signal first, where the law requires it** (Apple's Declared Age Range on iOS 26+, Google Play Age Signals): Texas since 4 Jun 2026, Brazil since 17 Mar 2026, Alabama and California from Jan 2027, Utah from May 2027, Louisiana from Jul 2027.
3. **Tag every ad request** with the player's tier (child, teen, adult). The consent tool does not do this; the game must.
4. **Privacy screen** (Google's certified consent tool, UMP, writing TCF v2.3 consent): only in the EEA, UK, Switzerland and 20 US states, and only above the country's age of digital consent (13 in the UK and Nordics, 14 in Spain and Italy, 15 in France, 16 in Germany, Ireland and the Netherlands). A "Privacy settings" button always stays in Settings.
5. **Apple's tracking question:** adults only, after the privacy screen.
6. **Start ads and analytics.** In the EEA, analytics that read device IDs start only after consent.

Notifications are asked at the first friend match ("Tell me when it's my turn?") on iPhone and Android 13+. The game keeps working if refused. The buzz, internet and ad ID need no prompt.

### 6.5 Tech for ads and purchases

- **Ads:** AdMob mediation with Google's consent tool. It passes the child and teen tags to every network and blocks uncertified networks for children. Only Families-certified networks are added (for example Unity Ads, ironSource, Liftoff/Vungle, InMobi, Chartboost, DT Exchange). AppLovin MAX is out: it must not run for children and left Google's family programme. Unity LevelPlay is the fallback (it has no teen setting).
- **Apple:** every network's SKAdNetwork ID in the app settings; privacy manifests and signatures for each SDK; privacy labels that list what each SDK collects.
- **Android:** ad ID permission declared (the child tag stops it being sent); the Data safety form covers every SDK.
- **Purchases:** Unity IAP v5. Looks are sold directly at real prices; Sparks are earned, never sold (EU virtual-currency principles); no random packs (Brazil and Apple rules). No "buy now" pop-ups for children; the stores' parental controls handle their purchases. Season Pass and VIP only for 13 and up, with clear terms and cancellation.
- **Remote settings** (Unity's Remote Config, part of the online services already picked): ad gaps, daily caps and prices change without an app update.
- **app-ads.txt** on the game's website, which ad networks require. It needs the web address from the business track.

### 6.6 Legal pages and store forms

- **Privacy policy**, linked inside the app: who we are, what each SDK collects, purposes and legal bases, a children's section, how long data is kept, user rights, transfers abroad, and the California notice with "Do Not Sell or Share". The website honours the Global Privacy Control signal; the app has its own toggle.
- **Terms of service:** items are licences, refunds go through the stores, conduct rules, Season Pass and VIP terms.
- **EU and UK representatives** if there is no office in the EU or UK.
- **Store forms:** Apple privacy labels and age questionnaire (4+ / 9+ / 13+ / 16+ / 18+); Google Data safety, Target audience (choosing under-13 groups brings in the Families policy), Ads declaration and IARC rating.
- **Account deletion** inside the app, guest accounts included (Google also needs a web link); Sign in with Apple tokens revoked.
- **Data requests** (access, export, deletion) answered within one month.
- **Watch list:** EU KIDS Act (proposed 17 Sep 2026, covers games), EU Digital Fairness Act (proposal expected mid-November 2026), Apple's EU tracking-prompt change in iOS 27.2.
- This is product-risk research, not legal advice. A lawyer reviews the privacy policy, terms and age set-up before launch (business track).

## 7. Art pipeline: Figma to Unity

**Figma file, one page per topic** (in the owner's own Figma team):

| Page | Holds |
|---|---|
| Colors and sizes | Every Art Bible token as Figma variables. A "Default" and a "High Contrast" mode; later one mode per cosmetic board look. Text styles for the type scale. |
| Icons | 96 px grid, 12 px stroke: four pad symbols, five Twists, menu icons (Art Bible §11). |
| Pads and parts | Each pad built in named layers (base, bevel highlight, shadow, rim, symbol, halo) with every state: Idle, Playback, Pressed, Correct, Wrong, Disabled, Add One. Ring, score header, Twist card, buttons, panels. |
| Screens: playing | Screens 1–7 at 1080 × 1920, every duel state. |
| Screens: online | Screens 8–10. |
| Screens: meta | Screens 11–12: profile with rivalries, badges and level; collection and shop. |
| Motion | Tap, Shift, hand-over, win, with Art Bible timings; prototype links for the key moments. |
| Ready for the game | Final exports named `SD_<Category>_<Element>_<State>_<Variant>`. |

- **Tokens sync:** variables are read from Figma and written into the game's theme in one step, so a color changed in Figma reaches the game without retyping.
- **Pads come over in layers**, so states are animated in the game (tint, scale, glow) instead of exporting an image per state. Export at the Art Bible working sizes (pads 512 px, icons 256 px).
- **Glow is its own layer.** Each pad gets a soft halo picture that the game fades and scales, instead of a full-screen glow effect. The silhouette stays solid, as the Art Bible asks, and cheap phones keep their frame rate.
- **Screens are rebuilt in Unity** from the Figma frames, using the same tokens.

## 8. Engine and tools

In plain words: start on the Unity version already on this Mac (the same one as the owner's latest game) and move to the next long-term version before the art goes in. Make the board from layered pictures with the glow painted in. Build every screen with Unity's long-standing menu system, and use one free animation add-on for every squash and pop.

| Area | Pick | Why | Watch |
|---|---|---|---|
| Unity version | 6.3 LTS (6000.3.23f1, installed). Move to 6.7 LTS when it ships (due end of 2026), before step 7 | Supported to Dec 2027; same version as StickerKnight. 6.6 is a short-lived "Supported" release; 7.0 is an alpha | One upgrade, done before art lands; a 6.6 user reported uGUI layouts that stopped resizing after upgrading |
| Licence | Unity Personal | Free under US$200K revenue plus funding in 12 months; splash screen optional | Pro is US$2,310 per seat per year if that changes |
| Render pipeline | URP with the 2D renderer | The Built-in pipeline is deprecated as of 6.5 | — |
| Board and pads | Layered sprites: base, highlight, shadow, rim, symbol, halo. Glow = halo alpha and scale. No full-screen bloom by default, HDR off | Bloom costs frame time on cheap Android phones and is not covered by on-tile post-processing | Bloom only as an optional high-end setting |
| Screens | uGUI + TextMeshPro everywhere; one Canvas Scaler, 1080 × 1920, scale with screen size | Unity still lists uGUI as the recommended runtime UI; the owner's games use it; particles in UI through the free UIParticle add-on | Re-check UI Toolkit and Unity's Figma connector at 6.7 for the list-heavy meta screens |
| Safe areas | A small SafeArea script on 6.3 (a built-in component arrives with 6.6+) | Art Bible insets and notches | — |
| Tweens | PrimeTween (free, no garbage per tween) | Squash, pop, sweep, sequences, awaitable | Custom licence, fine for commercial games |
| Figma tokens | Read the Figma variables through the Figma connection and write one Theme asset (colors, spacing, radii, type) | Figma's variables API for scripts is Enterprise-only; reading through the connection works on the owner's Pro plan (200 calls a day on a Full seat) | Unity's UI Toolkit Connector for Figma (0.1.0-exp, Sep 2026) imports layouts and tokens, but only for UI Toolkit and still experimental |
| Figma art | PNGs at the Art Bible working sizes, packed in Sprite Atlases, 9-slice for panels and cards | Sharp on 1290-wide phones | — |
| Web build | Unity's web player, compressed with a fallback, full-window portrait page, "tap to start" for sound; private itch.io page for tests, then a web game site (CrazyGames or Poki) for launch | Shareable by link, no store review; the sites bring players and share ad money | iPhone browsers can't buzz; first load 10–20 MB; phone ads and purchases (§6.5) need the phone apps |
| Store floors | Google Play target API 36 (since 31 Aug 2026); Apple builds need Xcode 26 / iOS 26 SDK (since 28 Apr 2026) | Xcode 27 is installed | Confirm the 6.3 patch in use targets API 36 |

## 9. Sound and buzz

- Four pad voices E4, G4, B4, E5 with distinct timbres (strategy doc §12), plus Twist earcons, ownership motifs, error transient, success stinger, celebrations and sparse music.
- Short mono WAV files, loaded in advance and kept uncompressed in memory.
- Mixer groups: Tones, Effects, Interface, Music, Voice. Music ducks during playback and input.

**Making a tap feel instant**

- **One moment for everything.** Sound, light and buzz all start in the same touch-down callback (Input System EnhancedTouch `onFingerDown`), never on release; uGUI `Button.onClick` fires on release, so pads don't use it. Only the first valid touch counts; taps outside the pads do nothing (GDD §5, §12).
- **Unity's own audio is enough.** Since Unity 6, Android uses its fast audio path. Settings: DSP buffer "Best latency", system sample rate 0 (the phone's own). Tones authored as 48 kHz mono; import as PCM, Decompress On Load, Preload Audio Data on; one AudioSource per pad. Measured on a cheap Android in step 2; a small native audio bridge (Google's Oboe library) only if the measurement fails. The well-known paid native-audio add-on has not been updated since January 2022.
- **Wireless headphones add a delay nothing can remove**, so the light and the buzz carry the instant feel. The strategy doc's sound-and-picture timing check comes before any rhythm-style mode.
- **Buzz through our own small native bridge.** iOS: the impact feedback generator, prepared just before input opens, or Apple's Core Haptics Unity plug-in for the per-pad patterns. Android: `performHapticFeedback` (no permission needed), predefined effects from API 29, composed patterns from API 30 where the phone supports every piece. Nice Vibrations is no longer sold on its own and its open-source version was archived in August 2025.
- **Frame rate set on purpose.** Unity runs phones at 30 fps unless told otherwise: 60 fps by default, 120 Hz as an option on phones that have it, lower in menus to save battery.

## 10. Unity project layout

- The Unity project sits at the repository root (`Assets/`, `Packages/`, `ProjectSettings/` beside `Docs/` and `Art/`), which is what the existing `.gitignore` expects and how the owner's other Unity games are laid out.
- This Mac already has Unity 6000.0.67f1, 6000.3.23f1, 6000.4.2f1 and 6000.5.4f1 with iOS and Android support, Xcode 27 and the Android SDK (checked 25 Oct 2026).
- Git LFS for new art, audio and fonts, set up before the first art lands.
- Portrait only. Reference resolution 1080 × 1920, scaled by width; safe areas respected; background extends on unusual aspect ratios.

```
Assets/SimonDuel/
  Rules/        SimonDuel.Rules     plain C#, no engine references; shared with the server
  Game/         SimonDuel.Game      match runner, seats, board, pads, ring, effects, audio, haptics
  UI/           SimonDuel.UI        screens and navigation
  Meta/         SimonDuel.Meta      profile, rivalries, Sparks, collection, badges, level
  Online/       SimonDuel.Online    service adapters behind interfaces; offline stand-ins first
  Common/       shared helpers, grown inside the first feature that needs them
  Settings/     DuelRules, Timing, Feel, ComputerLevels, AudioMix, Economy assets
  Art/ Audio/ Fonts/
  Scenes/       Boot (services), App (every screen and the duel; Rematch under 5 s)
  Tests/        EditMode (every GDD rule as a check), PlayMode (turn flow)
Server/         turn checker using the same Rules code
```

- **Log:** one line per second plus one per event, tagged `[Duel]`, `[Touch]`, `[Audio]`, `[Online]`. Example: `[Duel] rally=2 turn=7 P2 len=8 phase=Repeat taps=5/8 left=4.1s twist=Reverse`.
- **Pretend players in the Editor:** keys to auto-play right, slip, time out, hand the seat to the computer, and a pretend online friend who answers turns at once. This is how the game gets tested alone.

## 11. Build order

Each step ends with something the owner can try on a phone. Nothing is committed unless the owner asks. Every step is broken into small tasks in [TASKS.md](TASKS.md).

| # | Step | The owner can try | Done when |
|---:|---|---|---|
| 1 | Empty game in the browser | The game opens in the owner's computer browser | Web build from one command; works in Chrome and Safari on the computer |
| 2 | The four pads | Click or press a key: light and sound at once | Log shows tap-to-sound delay; under 80 ms perceived |
| 3 | The duel, plain | A full same-computer match with plain shapes and real sounds | Every GDD rule has an automatic check |
| 4 | Tuning from real games | Speeds, timer, match length set from the log | Median first match 60–180 s |
| 5 | Twists | Reverse and Shift, then Echo, with first-time explainers | 80% can explain a Twist before input |
| 6 | Computer and Practice | Beat Easy, lose to Hard | Levels feel distinct in a blind try |
| Phone | Phone browser | A private link plays the same game by touch on the owner's phone | Link works on mobile data; full match passing one phone; speeds re-checked for thumbs |
| — | **Checkpoint** | About ten pairs play | At least half ask for a rematch; otherwise fix the game first |
| 7 | The look | Figma art in the game; motion, effects, less-motion, high contrast | Art Bible acceptance checklist passes |
| 8 | All screens | Home, setup, results, settings, learn-by-playing | First interactive tap under 20 s from launch |
| 9 | Meta | Rivalries, Sparks and collection, badges and level | Sparks earned and spent; rivalries recorded |
| 10 | Money and permissions | Age question, privacy screen, tracking question, ad breaks, reward videos, the shop | Each age tier gets the right ads and prompts, checked on both phones in test mode |
| 11 | Online with friends | Invite link, live and waiting turns, notifications, rematch | A full match between two phones on different networks |
| 12 | Store-ready | Real ads and products, legal pages, store forms, store pages, final name | Passes both stores' review |
| 13 | Later | Party for 3–8 players; anything else only if the numbers ask | Chosen by the numbers |

Figma work runs alongside steps 2–6 so the art is ready for step 7.

**Store-ready checklist (step 12)**

- Public name cleared; bundle ID and invite domain chosen after it, never containing "Simon".
- Privacy policy; account deletion inside the app (Apple and Google) plus a web link (Google).
- Age-rating questionnaires: Apple's 4+ / 9+ / 13+ / 16+ / 18+ scale; declare online play and ads. Regional age checks wired where required.
- Privacy manifests for every third-party SDK (Unity merges its own).
- Android target API 36; Apple builds on the Xcode 26+ SDK.
- Purchases and restore working with real products; ads switched from test to live, checked against the age tiers (§6.3); app-ads.txt on the website.
- Developer accounts: Apple Developer Program (US$99 a year) and Google Play Console (US$25 once).

## 12. Testing and tuning

- **Rules:** every rule in the GDD is an automatic check (Reverse order, Twists never stack, one held Twist, Charge resets, starting player alternates, timeout budget `3.0 s + 0.75 s × length`, capped at 14 s).
- **Replays:** any match can be replayed from its seed and moves; bug reports carry that record.
- **Latency by camera:** film the tap with another phone's slow-motion camera and count frames from finger contact to light and sound. Cheap and honest.
- **Playtests:** the strategy doc's Tests A–E and its first-session funnel; prototype gates in strategy doc §18.

## 13. Risks

| Risk | Plain effect | What we do |
|---|---|---|
| Name and look | "Simon" and the round four-color board belong to Hasbro's world | Neutral app ID from day one; name and board shape cleared before any public page |
| Kids and online | Under-13 players limit names, ads and tracking | E1 picked: age question at first launch, ad tiers by age (§6.3), friends-only online |
| Ads cost players | Each full-screen ad loses a few players the next day | Ad gaps and caps are remote settings, tuned from our own numbers |
| Privacy rules keep changing | New state and EU rules every year | One age-tier rule covers most of them; watch list in §6.6; lawyer review before launch |
| Sound delay on Android | A late tone makes the game feel broken | Measure in step 2 before anything else is built |
| Nobody to play | A small game has few people online at once | Friends-only matches that wait for each other; the computer is always there |
| Note-taking | Online results can be faked by writing the pattern down | Friends only, no rankings; impossible input rejected |
| Meta growing faster than the game | Months on systems before the duel is proven | Meta waits for the checkpoint |

## Sources (checked 25–26 October 2026)

- **Unity and tools:** [Unity 6.3 LTS](https://unity.com/blog/unity-6-3-lts-is-now-available) · [Unity 6 support dates](https://unity.com/releases/unity-6/support) · [6.7 beta](https://discussions.unity.com/t/unity-6-7-beta-is-now-available/1736830) · [render pipeline strategy 2026](https://unity.com/topics/render-pipelines-strategy-for-2026) · [pricing](https://unity.com/products/pricing-updates) · [UI system comparison (6.7)](https://docs.unity.com/en-us/engine/6000.7/manual/uitoolkits/ui-system-compare) · [UI Toolkit in 6.7](https://discussions.unity.com/t/state-of-ui-toolkit-in-unity-6-7/1736756) · [UI Toolkit Connector for Figma](https://discussions.unity.com/t/experimental-ui-toolkit-connector-for-figma-design/1736969) · [Figma variables API](https://developers.figma.com/docs/rest-api/variables-endpoints) · [Figma MCP limits](https://developers.figma.com/docs/figma-mcp-server/rate-limits-access/) · [PrimeTween](https://github.com/KyryloKuzyk/PrimeTween) · [on-tile post-processing](https://docs.unity.com/en-us/engine/6000.6/manual/post-processing-and-full-screen-effects/urp/on-tile-post-processing)
- **Feel:** [Unity audio settings](https://docs.unity3d.com/6000.3/Documentation/Manual/class-AudioManager.html) · [Android low-latency audio](https://developer.android.com/games/sdk/oboe/low-latency-audio) · [Android haptics APIs](https://developer.android.com/develop/ui/views/haptics/haptics-apis) · [Apple Unity plug-ins](https://github.com/apple/unityplugins) · [Nice Vibrations archive](https://github.com/Lofelt/NiceVibrations) · [Input System touch](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.17/manual/Touch.html)
- **Age, privacy, stores:** [FTC COPPA amendments](https://www.ftc.gov/news-events/news/press-releases/2025/01/ftc-finalizes-changes-childrens-privacy-rule-limiting-companies-ability-monetize-kids-data) · [COPPA FAQ](https://www.ftc.gov/business-guidance/resources/complying-coppa-frequently-asked-questions) · [FTC age-verification statement](https://www.ftc.gov/news-events/news/press-releases/2026/02/ftc-issues-coppa-policy-statement-incentivize-use-age-verification-technologies-protect-children) · [Google Play Families policy](https://support.google.com/googleplay/android-developer/answer/9893335) · [App Review Guidelines](https://developer.apple.com/app-store/review/guidelines/) · [Apple age ratings](https://developer.apple.com/help/app-store-connect/reference/app-information/age-ratings-values-and-definitions) · [Apple age assurance](https://developer.apple.com/support/age-assurance/) · [Play Age Signals](https://developer.android.com/google/play/age-signals/overview) · [Android target API](https://developer.android.com/google/play/requirements/target-sdk) · [Google account deletion](https://support.google.com/googleplay/android-developer/answer/13327111)
- **Money:** [Unity IAP v5](https://docs.unity.com/en-us/iAP/upgrade-to-iap-v5) · [Unity IAP consent](https://docs.unity.com/en-us/iap/privacy-and-consent/overview) · [LevelPlay regulation settings](https://docs.unity.com/en-us/grow/levelplay/sdk/unity/regulation-advanced-settings) · [AdMob targeting](https://developers.google.com/admob/unity/targeting)
- **Money:** [Appodeal eCPM report](https://appodeal.com/the-mobile-ecpm-report-updated-q4-2024-view) · [Tenjin ad monetization 2026](https://tenjin.com/blog/ad-mon-gaming-2026/) · [AppsFlyer app monetization 2026](https://gamedevreports.substack.com/p/appsflyer-app-monetization-in-2026) · [GameAnalytics benchmarks 2026](https://gamedevreports.substack.com/p/gameanalytics-mobile-and-pc-game) · [Liftoff casual report](https://liftoff.ai/?p=34267) · [ad quality and churn](https://www.pocketgamer.biz/what-our-data-reveals-about-ad-quality-and-player-churn-in-mobile-games/) · [2 Player Games listing](https://apps.apple.com/us/app/id1465731199)
- **Privacy and ads:** [UMP for Unity](https://developers.google.com/admob/unity/privacy/gdpr) · [AdMob age tags](https://developers.google.com/admob/unity/targeting) · [AdMob Families](https://support.google.com/admob/answer/6223431) · [TCF v2.3](https://support.google.com/admob/answer/9760862) · [Play certified ad SDKs](https://support.google.com/googleplay/android-developer/answer/12955712) · [Apple user privacy and ATT](https://developer.apple.com/app-store/user-privacy-and-data-use/) · [ICO Children's Code](https://ico.org.uk/for-organisations/uk-gdpr-guidance-and-resources/childrens-information/childrens-code-guidance-and-resources/age-appropriate-design-a-code-of-practice-for-online-services/code-standards/) · [EU virtual currency principles](https://ec.europa.eu/commission/presscorner/api/files/document/print/en/ip_25_831/IP_25_831_EN.pdf) · [Brazil ECA Digital](https://www.mayerbrown.com/en/insights/publications/2026/04/enforcement-of-brazils-eca-digital-introduces-new-obligations-for-companies) · [state app-store age laws](https://www.wiley.law/alert-Key-Developments-With-State-App-Store-Accountability-Acts-as-Texas-Act-Takes-Effect) · [EU KIDS Act](https://www.lw.com/en/insights/eu-kids-act-eu-commission-publishes-proposal-on-child-protection-online)
- **Online:** [UGS pricing](https://unity.com/products/gaming-services/pricing) · [Cloud Code push messages](https://docs.unity.com/ugs/en-us/manual/cloud-code/manual/modules/how-to-guides/push-messages) · [Cloud Code external services](https://docs.unity.com/en-us/cloud-code/modules/how-to-guides/external-services-integration) · [UGS billing limits](https://docs.unity.com/services/pricing-and-billing) · [Multiplay end of service](https://discussions.unity.com/t/end-of-services/1698428) · [Firebase pricing](https://firebase.google.com/pricing) · [Dynamic Links shutdown](https://firebase.google.com/support/dynamic-links-faq) · [Nakama multiplayer](https://heroiclabs.com/docs/nakama/concepts/multiplayer/) · [Heroic pricing](https://heroiclabs.com/pricing) · [PlayFab Foundation Mode](https://developer.microsoft.com/en-us/games/articles/2026/03/gdc-2026-introducing-foundation-mode-for-playfab/) · [Photon async](https://doc.photonengine.com/realtime/current/tutorials/async-and-turnbased) · [Game Center turn-based](https://developer.apple.com/documentation/gamekit/gkturnbasedmatch) · [Android FLAG_SECURE](https://developer.android.com/reference/android/view/WindowManager.LayoutParams#FLAG_SECURE) · [EU AI Act transparency](https://www.goodwinlaw.com/en/insights/publications/2026/08/alerts-technology-dpc-eu-ai-act-transparency-obligations-now-in-force)
