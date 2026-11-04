# Simon Duel — Task List

**Started:** 26 October 2026 · **Plan:** [UNITY_PLAN.md](UNITY_PLAN.md) · **Rules of the game:** [GDD.md](GDD.md) · **Look:** [ART_BIBLE.md](ART_BIBLE.md)

How we use this list:

- One task at a time, in order. Each one is small enough to finish in one sitting.
- **You:** marks a task that needs your hands, your phone, your accounts or your decision. Everything else Claude does and shows you.
- A task is ticked only after it is checked. Nothing is committed until you say so.
- The art, sound and business tracks run alongside the steps. Each of their tasks says which step needs it.

Order: **1 → 2 → 3 → 4 → 5 → 6 → Phone browser → Checkpoint → 7 → 8 → 9 → 10 → 11 → 12 → 13**. Computer browser first (4 Nov 2026), then phone browsers; the phone apps (phone track) start after the Checkpoint.

---

## Step 1 — Empty game in your browser

You can try at the end: open a link on your phone's browser (or your computer) and a dark navy screen with the game's name appears.

Already in place (checked 26 Oct and 3 Nov 2026):

- [x] **1.01** Unity 6.3 LTS (6000.3.23f1) is installed with web, iOS and Android support.
- [x] **1.02** Xcode 27 and Unity's own Android tools are installed (needed later, for the phone apps).
- [x] **1.03** Git LFS is installed (3.7.1), for storing big art and sound files.
- [x] **1.04** Company: **CuzGames** (owner, 3 Nov 2026). Test app ID `com.cuzgames.duel.dev`; no "Simon"; the permanent ID comes after the public name.

Make the project:

- [x] **1.05** Create the project from Unity's **"Universal 2D"** template (Unity 6.3) in the repo root, beside `Docs/` and `Art/`. *Done when* the project opens with no errors. Done 3 Nov 2026: opened with no errors.
- [x] **1.06** **You:** add it to Unity Hub once: Projects → Add → "Add project from disk" → the `SimonGame` folder. *Done when* it opens and the Console is clean. Done 3 Nov 2026: opened, no errors in the log.
- [x] **1.07** Remove the template extras we won't use: sample scene (removed in 1.15 instead), Visual Scripting, Timeline, Tilemap, Sprite Shape, 2D Animation, Aseprite and PSD importers, Multiplayer Center. *Done when* only the needed packages are left. Done 3 Nov 2026: left are 2D Sprite, 2D Tooling, URP, Input System, uGUI, Test Framework, IDE support.
- [x] **1.08** Set Git LFS rules for pictures, sounds and fonts. Unity scene files stay as readable text. *Done when* the rules are listed. Done 3 Nov 2026 (inside `Assets/` only).

Make it a browser game:

- [x] **1.09** Switch the project to the web, and set the name ("Simon Duel" for now), company CuzGames, the test app ID and version 0.1.0 (build 1). Done 3 Nov 2026; Unity splash off too. Re-run from the menu CuzGames → Apply Project Setup.
- [x] **1.10** A web page that fills the whole browser window, portrait layout, dark navy while loading, with a simple loading bar. Done 3 Nov 2026: checked in a real browser build.
- [x] **1.11** Keep the download small: compressed files with a fallback that works on any web host, unused engine parts stripped. *Done when* the first load is under about 15 MB. Done 3 Nov 2026: first test build is 13.2 MB.
- [x] **1.12** Picture settings: 2D renderer, HDR off, anti-aliasing off, post-processing off, Unity splash screen off, one quality level. Done 3 Nov 2026: one quality level, named "Game".
- [x] **1.13** Input: Unity's new input system only, with touch and mouse both working. Done 3 Nov 2026 (already set by the template; the first screen uses it).
- [x] **1.14** Sound: browsers only allow sound after the first click. Owner, 4 Nov 2026: no "Tap to start" screen; that first click is the one on the menu. Done 3 Nov 2026: the cover clears on the first tap.

The first screen:

- [x] **1.15** One scene called "App": navy background, the game's name, and "0.1 (1)" in a corner so you can recognize the build. Done 3 Nov 2026: shows "0.1.0 (1)"; the sample scene is gone.
- [x] **1.16** On start, write one line to the browser's log: version, browser, phone or computer, screen size. Done 3 Nov 2026: the line appears in the browser log.

Get it running in your computer's browser:

- [x] **1.17** One-command web build: a Unity menu item and a terminal command that build to `Builds/Web`. Done 4 Nov 2026: the terminal command is checked; the menu item is CuzGames > Build for Web.
- [x] **1.18** Local test: Claude serves the build from this Mac; you open the address in your computer's browser. *Done when* the navy screen opens and clicking clears "Tap to start". Done 4 Nov 2026: served from this Mac; the tap clears the cover in the built-in browser.
- [x] **1.21** Check it in Chrome and Safari on this computer. (1.19 and 1.20 moved to the Phone browser step.) Done 4 Nov 2026: the owner checked both.
- [x] **1.22** Write down how to build, where the browser log is, and how to upload, in the repo's `CLAUDE.md`. Done 4 Nov 2026 (the upload part moved to M.02).
- [ ] **1.23** Optional: connect the Unity bridge your Knife-Toss project uses, so Claude can press Play, read the Console and take Editor screenshots.
- [x] **1.24** **You:** say "commit" when happy. First commit of the Unity project. Done 4 Nov 2026.

## Step 2 — The four pads

You can try at the end: click the four pads in your computer's browser, or press keys 1–4; each one lights and sounds the instant you press.

- [x] **2.01** Make the rules area: a part of the code with no Unity inside, plus its test area. It knows the four pads: Circle, Triangle, Square, Star. *Done when* a first test passes. Done 4 Nov 2026: first test passes.
- [x] **2.02** Make the "Feel" settings file: every timing and size for the pads as a named number. Done 4 Nov 2026.
- [x] **2.03** Screen scaling: lay out at 1920 × 1080 (widescreen layout W1) and fill the browser window. Done 4 Nov 2026.
- [x] **2.04** Draw four plain pads in the right colors, with dark symbols, at the Art Bible positions. Done 4 Nov 2026 (stand-in shapes).
- [x] **2.05** Press: react when the mouse button or finger goes down, not when it lifts. Each tap area is bigger than its pad. Only the first finger counts; taps outside the pads do nothing. Done 4 Nov 2026 for mouse; finger checks are M.07.
- [x] **2.06** Light: a pad brightens the moment it is touched and settles back. Done 4 Nov 2026.
- [x] **2.07** Make four stand-in tones (E4, G4, B4, E5) as short, clean sound files. The final sounds come in the sound track. Done 4 Nov 2026: low to high, Circle, Square, Triangle, Star.
- [x] **2.08** Sound: each pad plays its tone in the same instant as its light. Done 4 Nov 2026: the owner checked it.
- [x] **2.10** Check the four sounds start together with the light in Safari and Chrome on this computer. Done 4 Nov 2026: the owner checked it in both browsers.
- [x] **2.12** Run smoothly at the browser's own frame rate (usually 60). Done 4 Nov 2026: the log shows the browser's full rate.
- [x] **2.13** Touch log: one line per tap (which pad, how long it took to react) and one summary line per second. Done 4 Nov 2026.
- [x] **2.14** Keys 1–4 and mouse clicks press the pads, in the browser as well as the Editor. Done 4 Nov 2026.
- [ ] **2.15** **You:** click fast and slow while filming the screen with your phone's slow-motion camera. We count frames from click to light and sound. *Done when* it feels instant (under about 80 ms).
- [x] **2.16** Only if sound is late in a browser: play the tones through the browser's own sound system directly, as a small add-on. Not needed: the owner reported no late sound, 4 Nov 2026.
- [x] **2.17** **You:** say how it feels. Your words go in the journal. Done 4 Nov 2026: "Feels good."

## Step 3 — The duel, plain

You can try at the end: a full two-player match on one computer's browser, swapping seats, with plain shapes and real sounds.

Rules (tested without opening the app):

- [x] **3.01** "Duel rules" settings: rallies to win (2, so best of 3), a 2-step start, timer 3.0 s + 0.75 s per step up to 14 s, and every other rule number. Done 4 Nov 2026.
- [x] **3.02** One seeded random source: the same seed always gives the same match. Done 4 Nov 2026.
- [x] **3.03** Match state: players, score, who starts, the pattern, whose turn. Done 4 Nov 2026.
- [x] **3.04** Start a rally: a random 2-step pattern; random first starter, then alternate each rally. Done 4 Nov 2026.
- [x] **3.05** Turn plan: what must be entered, how fast playback goes for this length, the time allowed. Done 4 Nov 2026.
- [x] **3.06** Check each tap: right, finished, or wrong (and what was expected). Done 4 Nov 2026.
- [x] **3.07** Add one step and hand the turn over. Done 4 Nov 2026.
- [x] **3.08** Time running out loses the rally. Done 4 Nov 2026.
- [x] **3.09** Rally and match end: first to 2 rallies wins; Rematch flips who starts. Done 4 Nov 2026.
- [x] **3.10** Match record: the seed plus every move, enough to replay a match exactly. A test replays one and gets the same result. Done 4 Nov 2026.

On screen:

- [x] **3.11** Turn order on screen: whose turn → watch → short pause → repeat → add one → hand over. Done 4 Nov 2026; owner removed the "click when ready" step the same day, so a turn starts by itself.
- [x] **3.12** Watch: pads light and sound in order at the right speed. Taps are ignored meanwhile. Done 4 Nov 2026.
- [x] **3.13** Repeat: the timer runs; a slip ends the rally and shows the pad that was expected. Done 4 Nov 2026.
- [x] **3.14** Add one: "ADD ONE" shows; the chosen pad plays once and joins the pattern. Done 4 Nov 2026.
- [x] **3.15** Hand over: the banner names the next player and the ring sweeps round, then the turn starts by itself. Done 4 Nov 2026.
- [x] **3.16** Widescreen panels and banner: names, score pips, state word (WATCH / REPEAT / ADD ONE), pattern length, the ring as the timer. Done 4 Nov 2026.
- [x] **3.17** Rally result: the slip for a moment, then the point to the other side. Done 4 Nov 2026.
- [x] **3.18** Match result: winner, score, a big Rematch button, a smaller Home button. Done 4 Nov 2026.
- [x] **3.19** Leaving the app pauses the game and hides the pattern; coming back resumes it. Done 4 Nov 2026.
- [x] **3.20** Duel log: one line per second and one per event. Done 4 Nov 2026.
- [x] **3.21** Pretend players in the Editor: keys for "play it right", "slip" and "time out". Done 4 Nov 2026: F1 right, F2 slip, F3 out of time, in test builds only.
- [x] **3.22** Automatic test: a whole match with pretend players ends with the right winner. Done 4 Nov 2026: 19 tests pass.
- [x] **3.23** **You:** play a full match with someone on your computer. Your notes go in the journal. Done 4 Nov 2026: "Feels good."

## Step 4 — Tuning from real games

You can try at the end: the same game, with speeds and timers set from how people actually played.

Owner, 4 Nov 2026: skip 4.03–4.09 for now and do them with the matches from the Checkpoint, when Twists are in.

- [x] **4.01** The game saves a short summary of every match: lengths reached, time per tap, timeouts, match length. Done 4 Nov 2026: kept in the browser, turn by turn, up to 200 matches.
- [x] **4.02** A way to save that file to your computer (a download button). Done 4 Nov 2026: "Download match notes" on the winner screen.
- [ ] **4.03** **You:** play at least 5 matches with different people.
- [ ] **4.04** Read the summaries: typical match length, where slips happen, how often the timer runs out.
- [ ] **4.05** Set the playback speeds from the data.
- [ ] **4.06** Set the timer from the data.
- [ ] **4.07** **You:** pick best of 3 or best of 5 after playing both.
- [ ] **4.08** **You:** pick a 1-step or 2-step start after playing both.
- [ ] **4.09** Record the new numbers and why in the journal.

## Step 5 — Twists

You can try at the end: Reverse, Shift and Echo in real matches, each explained the first time it appears.

- [x] **5.01** Charge: one pip per successful turn, a Twist at three; none in a rally's first two turns. Done 4 Nov 2026.
- [x] **5.02** Hold at most one Twist; Twists never stack. Done 4 Nov 2026.
- [x] **5.03** A shuffled bag of Twists, so the same one rarely repeats. Done 4 Nov 2026.
- [x] **5.04** Arming: one tap to use it after Add One; skipping keeps it. A setting can make it automatic instead. Done 4 Nov 2026: 4 seconds to choose, then it is kept.
- [x] **5.05** Reverse rule: enter the pattern back to front. Done 4 Nov 2026.
- [x] **5.06** Reverse on screen: "REVERSE" banner, ring runs the other way. Done 4 Nov 2026.
- [x] **5.07** Shift rule: after watching, every pad moves one slot clockwise; you tap by pad, not by place. Done 4 Nov 2026.
- [x] **5.08** Shift on screen: pads slide to their new slots, then slide home during the hand-over. Done 4 Nov 2026.
- [ ] **5.09** Shift with less motion: a fade plus numbered slots. Waits for the less-motion setting in step 8.
- [x] **5.10** Echo rule: one marked step must be tapped twice. Done 4 Nov 2026.
- [x] **5.11** Echo on screen: the doubled step pulses twice. Done 4 Nov 2026.
- [x] **5.12** Charge pips, Twist slot and arm button on screen (plain). Done 4 Nov 2026.
- [x] **5.13** First-time explainer: one sentence plus a short demo per Twist; a short banner after that. Done 4 Nov 2026: shown once per browser.
- [x] **5.14** Twist log lines, and Editor keys to force any Twist. Done 4 Nov 2026: R, S and E hand over a Twist, in test builds only.
- [ ] **5.15** **You:** play with Twists. Can people explain the rule before they tap? Notes go in the journal.

## Step 6 — Computer opponent and Practice

You can try at the end: single player. Beat Easy, lose to Hard, and practise alone.

- [x] **6.01** The computer's memory: slips become likely past its comfortable length (Easy 5, Medium 8, Hard 12), mostly in the middle of the pattern, more under Twists. Done 4 Nov 2026.
- [x] **6.02** The computer's timing: human-like gaps between taps, slower near its limit. Done 4 Nov 2026.
- [x] **6.03** The computer's Add One: random on Easy, tricky runs on Hard. Done 4 Nov 2026.
- [x] **6.04** The computer's Twists: used at once on Easy, saved for long patterns on Hard. Done 4 Nov 2026.
- [x] **6.05** "Computer levels" settings file: every number per level. Done 4 Nov 2026.
- [x] **6.06** The computer plays on the board while you watch, always labelled "COMPUTER". Done 4 Nov 2026.
- [x] **6.07** Pick Easy, Medium or Hard before the match. Done 4 Nov 2026: a plain menu after "Tap to start".
- [x] **6.08** Practice: Classic (the game adds steps) and Endless (how long can you go). Done 4 Nov 2026.
- [x] **6.09** Practice: Twist practice, sight-only and sound-only. Done 4 Nov 2026.
- [x] **6.10** Automatic test: 1,000 computer-vs-computer matches per level; the levels must differ clearly. Done 4 Nov 2026: Hard wins over 85% against Easy; the 55 tests pass.
- [ ] **6.11** **You:** beat Easy, lose to Hard. Notes go in the journal.

## Phone browser — after step 6, before the Checkpoint

You can try at the end: the same game from a private link in your phone's browser, played by touch, including a full match passing one phone.

- [ ] **M.01** **You:** create a free itch.io account under CuzGames, for a private test page. (Was 1.19.)
- [ ] **M.02** Upload the build to a private itch.io page with a secret link. *Done when* the link opens on your phone over mobile data. (Was 1.20.)
- [ ] **M.03** Check it in Safari on iPhone and Chrome on Android.
- [ ] **M.04** An upright phone layout at the Art Bible's 1080 × 1920 sizes, keeping clear of notches and the home bar.
- [ ] **M.05** Buzz in the browser: works on Android phones; iPhone browsers can't buzz, so nothing happens there. (Was 2.09.)
- [ ] **M.06** A buzz on/off setting. In the Unity Editor the buzz only writes to the log. (Was 2.11.)
- [ ] **M.07** Check touch: only the first finger counts, and taps outside the pads do nothing.
- [ ] **M.08** **You:** tap fast and slow while filming with another phone's slow-motion camera. *Done when* it feels instant (under about 80 ms).
- [ ] **M.09** **You:** play a full match passing one phone. Your notes go in the journal. (The turn now starts by itself; check there is enough time to pass the phone.)
- [ ] **M.10** Adjust speeds and the turn timer if thumbs need different numbers than the mouse did.

## Checkpoint — real players

- [ ] **C.01** A private test link on itch.io that friends open in their phone's browser.
- [ ] **C.02** The match summaries from step 4 saved per test so we can read them afterwards.
- [ ] **C.03** A one-page sheet of what to watch: rematch asked for? Twist understood? Laughs? Arguments?
- [ ] **C.04** **You:** about ten pairs play.
- [ ] **C.05** **You:** decide: go on, or fix the game first. At least half asking for a rematch means go on. The decision goes in the journal.

## Step 7 — The look

You can try at the end: the game looking and moving like the Art Bible, on your phone.

- [ ] **7.01** Move to Unity 6.7 LTS if it has shipped; check everything still works.
- [ ] **7.02** Color sync: read the colors and sizes from Figma and write them into the game in one step.
- [ ] **7.03** Fonts in, with the type sizes from the Art Bible.
- [ ] **7.04** Add the animation add-on (PrimeTween).
- [ ] **7.05** Real pads from Figma, in layers, with every state: resting, lit, pressed, right, wrong, off, add one.
- [x] **7.06** Tap squash 70 ms in and 110 ms out; playback glow 420 ms; right-tap ripple; wrong-tap shake. Done 4 Nov 2026 in code; the shapes are still stand-ins.
- [x] **7.07** Glow as a soft halo under each pad, faded and scaled. No full-screen glow effect. Done 4 Nov 2026.
- [ ] **7.08** The ring: timer runs clockwise, Reverse runs back with arrowheads; Player 1 solid, Player 2 segmented.
- [ ] **7.09** Real top bar, banners, Twist slot and panels.
- [ ] **7.10** Shift orbit (650 ms) with the less-motion version.
- [x] **7.11** Small particle bursts within the Art Bible counts. Done 4 Nov 2026.
- [x] **7.12** Hand-over sweep, score pip pop, result card motion, win confetti (skippable). Done 4 Nov 2026.
- [ ] **7.13** "Reduce motion" setting works everywhere.
- [ ] **7.14** "High contrast" setting swaps to the high-contrast colors.
- [ ] **7.15** Color-blind presets that shift colors but keep the symbols.
- [ ] **7.16** Final sounds and buzz patterns in; volumes in groups; music dips during play.
- [ ] **7.17** Check small, tall and wide phone screens.
- [ ] **7.18** Speed check on a cheap Android phone: steady 60 frames a second, no hiccups during play.
- [ ] **7.19** Art Bible checklist passes.
- [ ] **7.20** **You:** look and feel on your phone. Notes go in the journal.

## Step 8 — All the screens

You can try at the end: the whole game from launch: title, home, setup, play, results, settings, first-time tutorial.

- [ ] **8.01** Moving between screens, including Android's back button.
- [ ] **8.02** Title screen; the very first launch goes straight into the tutorial.
- [ ] **8.03** Home: Computer, Same phone, Online, plus Sparks and level.
- [ ] **8.04** Setup: two names for same phone; a level for the computer.
- [x] **8.05** Pause card. Done 4 Nov 2026: pause button, "Carry on" and "Quit to menu".
- [ ] **8.06** Settings: volumes, buzz, reduce motion, high contrast, color presets, playback speed, input time, language. Started 4 Nov 2026: sound on/off, reduce motion and slower patterns are in.
- [x] **8.07** Learn-by-playing tutorial: watch, repeat, add one, see the other side repeat, then Reverse. Done 4 Nov 2026: runs once on first launch.
- [x] **8.08** "How to play" replays the tutorial demos. Done 4 Nov 2026: "How to play" on the menu replays it.
- [ ] **8.09** Save on the phone: settings, names, stats, safe across app updates.
- [ ] **8.10** All text in one table, ready for other languages; English first.
- [ ] **8.11** Basic usage counts for the first session, with no tracking (safe for kids).
- [ ] **8.12** Check: first tap within 20 seconds of opening; Rematch back in play within 5 seconds.
- [ ] **8.13** **You:** go through the whole game from launch. Notes go in the journal.

## Step 9 — Meta: rivalries, Sparks and collection, badges and level

You can try at the end: earn Sparks, unlock a board look, see your record against each person.

- [ ] **9.01** Profile on the phone: level, points toward the next level, Sparks, owned and equipped items, badges, stats.
- [ ] **9.02** "Economy" settings file: Sparks per match, win bonus, first match of the day, prices.
- [ ] **9.03** Earn Sparks at the end of each match, shown on the result.
- [ ] **9.04** Level and level-up moment.
- [ ] **9.05** Rivalries: record against each name: wins, losses, best pattern together, longest streak.
- [ ] **9.06** Profile screen with rivalries and a Revenge button that starts a rematch.
- [ ] **9.07** Badges: the first set and what earns each.
- [ ] **9.08** Badges on the profile, with a small "new badge" moment.
- [ ] **9.09** Collection: board looks, pad sounds, crowns; buy with Sparks, equip.
- [ ] **9.10** Board looks change the board but always keep the symbols and contrast; high contrast wins over any look.
- [ ] **9.11** Pad sound sets change the four tones together.
- [ ] **9.12** Shop corner in the collection (purchases come in step 10).
- [ ] **9.13** Check how long each unlock takes to earn, and adjust prices.
- [ ] **9.14** **You:** earn, unlock, equip. Notes go in the journal.

## Step 10 — Money and permissions

You can try at the end: the first launch asks your age, then privacy choices and Apple's tracking question only where they apply. Test ads show after every second match; reward videos and the shop work with test purchases.

In the browser (first):

- [ ] **10.W1** **You:** pick the web game site to launch on: CrazyGames or Poki (they bring players and share ad money). Claude compares their terms first.
- [ ] **10.W2** Add that site's own add-on: its ad break after every 2nd match and its reward videos, in test mode.
- [ ] **10.W3** Check which age and privacy screens the site already shows, and add only what's missing (age question, privacy settings button).
- [ ] **10.W4** "No ads + starter looks" on the web, only if the site allows purchases; otherwise Sparks only.
- [ ] **10.W5** **You:** play the web version as an adult and as a child; sit through an ad break. Notes go in the journal.

Phone apps (after the phone track, same rules):

- [ ] **10.01** **You:** pick how hard the game sells (board choice F).
- [ ] **10.02** "Money" settings file: ad break after every 2nd match, at most 6 a day, none in the first session, reward-video limits, prices. Every number can change later without an app update.
- [ ] **10.03** Age question at first launch: month and year, nothing pre-filled, no hint of a cut-off, no instant retry. No answer counts as a child. Asked once.
- [ ] **10.04** Three age groups inside the game: child (under 13 or unknown), teen (13–17), adult (18+), plus each country's age for privacy consent.
- [ ] **10.05** Where the law requires it (Texas, Brazil, more US states in 2027), read the phone's own age signal before asking.
- [ ] **10.06** **You:** open an AdMob account in your own name, with tax and payment details. Claude lists the ad slots to create.
- [ ] **10.07** Add Google's ad and consent tools, in test mode only.
- [ ] **10.08** Privacy screen: only in the EU, UK, Switzerland and the 20 US states that need it, and only above that country's consent age.
- [ ] **10.09** "Privacy settings" button in Settings to change choices later, plus a "Do Not Sell or Share" switch.
- [ ] **10.10** Apple's tracking question: adults only, after the privacy screen, never after a refusal, with Apple's required one-line reason.
- [ ] **10.11** Mark every ad request with the player's age group: children get only family-certified ad companies with no ad ID; teens get ads that are not personalised.
- [ ] **10.12** Start ads and stats only after the age question and privacy screen are done.
- [ ] **10.13** Ad break on the results screen after every 2nd match. Never mid-rally, during a hand-over, in the first session, or for anyone who paid. For children: closable after 5 seconds and never at launch.
- [ ] **10.14** Reward video: double the Sparks from a match.
- [ ] **10.15** Reward video: try a locked look or sound pack for 24 hours.
- [ ] **10.16** Reward video: a daily bonus.
- [ ] **10.17** Reward video: one second chance per run, against the computer and in practice only.
- [ ] **10.18** Add more ad companies (family-certified only), each with Apple's tracking IDs and privacy files.
- [ ] **10.19** **You:** in App Store Connect and Google Play, sign the paid-apps agreement and add bank and tax details. Needed even to test purchases.
- [ ] **10.20** **You:** create the first products: "No ads + starter looks" ($4.99) and the first single looks ($0.99–1.99). Claude writes the list.
- [ ] **10.21** Purchases: buy, restore, receipt check, local prices shown.
- [ ] **10.22** "No ads" works at once: ad breaks stop, reward videos stay optional.
- [ ] **10.23** Shop with real prices. Sparks are earned, never sold; no random packs; no "buy now" pop-ups for children.
- [ ] **10.24** Season Pass, if F2 or F3 is picked: free and paid tracks, 13 and up, a settings file per season.
- [ ] **10.25** VIP monthly subscription, only if F3 is picked: 13 and up, terms and cancellation shown before buying.
- [ ] **10.26** Change ad gaps, caps and prices from the Unity dashboard without an app update.
- [ ] **10.27** Money log lines (`[Ads]`, `[Shop]`) and our own revenue counts, with no tracking for anyone under 18.
- [ ] **10.28** Test every path on both phones with test ads: child, teen, adult; EU, UK, a US state, elsewhere.
- [ ] **10.29** **You:** do the first launch as an adult and as a child, sit through an ad break, buy something with a test account. Notes go in the journal.

## Step 11 — Online with friends

You can try at the end: a full match with a friend on another phone, live or turn by turn with notifications.

- [ ] **11.01** **You:** link the project to your Unity account's online services (Unity Cloud dashboard).
- [ ] **11.02** Quiet guest sign-in the first time someone goes online.
- [ ] **11.03** Under 13: names from a list and preset emotes only. 13 and up: typed names through a word filter.
- [ ] **11.04** The server-side turn checker, using the same rules code as the phone.
- [ ] **11.05** Create a match and a short invite code.
- [ ] **11.06** Join a match by code.
- [ ] **11.07** Send a finished turn; the server checks and saves it; sending twice does no harm.
- [ ] **11.08** Live updates while both players are in the game.
- [ ] **11.09** **You:** create Apple's notification key in your developer account; Claude sets up Google's free notification service.
- [ ] **11.10** Ask for notifications at the first friend match ("Tell me when it's my turn?"); the game still works if refused.
- [ ] **11.11** "Your turn" notifications when the other player is away. Under 18: "your turn" only, never offers.
- [ ] **11.12** "Your online games" screen: live now, your turn, their turn.
- [ ] **11.13** "Their turn, replayed" before your own turn (skippable).
- [ ] **11.14** 24-hour turn limit and what happens when it runs out.
- [ ] **11.15** Leaving mid-turn: 10 seconds to come back, then it counts as a timeout.
- [ ] **11.16** Online rematch; online friends join your rivalries; past opponents become your friend list.
- [ ] **11.17** Report and block (13 and up).
- [ ] **11.18** Profile, Sparks and items saved online, writable only by the server.
- [ ] **11.19** Sign in with Apple or Google to keep progress on a new phone.
- [ ] **11.20** Delete account inside the app, guest accounts included; Sign in with Apple access revoked.
- [ ] **11.21** Invite links that open the game: needs the web address from B.06; until then, codes only.
- [ ] **11.22** Pretend online friend in the Editor who answers turns at once.
- [ ] **11.23** Test on two phones on different networks, including airplane mode in the middle of a turn.
- [ ] **11.24** **You:** play a full online match with a friend. Notes go in the journal.

## Step 12 — Store-ready

You can try at the end: the game as it will appear in the stores, ready for review.

- [ ] **12.01** Final app ID from the public name; set once and never changed.
- [ ] **12.02** Final logo and app icon from Figma, in every size the stores ask for.
- [ ] **12.03** Switch ads from test to live; app-ads.txt on the website (ad companies require it).
- [ ] **12.04** **You:** products live in both stores, priced for every country.
- [ ] **12.05** Privacy policy and terms published (after the lawyer's review, B.08), linked inside the app; web page for deleting an account.
- [ ] **12.06** **You:** privacy answers in both stores (Apple's privacy labels, Google's data safety form), drafted by Claude to match every add-on.
- [ ] **12.07** **You:** Google Play target audience and ads declaration; age-rating questionnaires in both stores, declaring ads and purchases.
- [ ] **12.08** Privacy files and signatures for every outside add-on.
- [ ] **12.09** Crash reports, with settings that are safe for children.
- [ ] **12.10** Store text, screenshots, and a 10–15 second video of hands and reactions.
- [ ] **12.11** Wider test: TestFlight for outside testers; Google's closed test (new personal Google accounts need 12 testers for 14 days before going public).
- [ ] **12.12** Final check on a cheap phone: speed, battery, small screen, ad loading.
- [ ] **12.13** **You:** send to Apple and Google for review.
- [ ] **12.14** **You:** release.

## Step 13 — Later, only if the numbers ask

- [ ] **13.01** Party mode for 3–8 players on one phone.
- [ ] **13.02** More Twists (Rush, Blackout) if wanted.
- [ ] **13.03** **You:** anything else the numbers point to (daily pattern, random players, computer tour, seasons).

---

## Phone track — after the Checkpoint, needed by step 10 (phone money) and step 12

- [ ] **P.01** Phone settings: portrait only; iPhone only for now (an iPad runs it in iPhone mode); iOS 15.6 or newer, automatic signing with your personal team (never the korro.ai one).
- [ ] **P.02** Android settings: 64-bit only, Unity's default minimum Android version, target API 36 (Google's rule since August 2026).
- [ ] **P.03** One-command builds for iPhone (`Builds/iOS`) and Android (`Builds/Android`).
- [ ] **P.04** **You:** plug your iPhone into this Mac, tap "Trust", turn on Settings → Privacy & Security → Developer Mode.
- [ ] **P.05** Build for iPhone and install it from the terminal. *Done when* the game opens on your iPhone.
- [ ] **P.06** **You (if you have an Android phone):** Developer options → USB debugging on; plug it in and allow this Mac.
- [ ] **P.07** Build for Android and install it.
- [ ] **P.08** iPhone buzz: a small bridge to the iPhone's tap feedback.
- [ ] **P.09** Android buzz through Android's own tap feedback.
- [ ] **P.10** 60 frames a second, with 120 as an option on phones that support it.
- [ ] **P.11** **You:** the slow-motion camera check again on the phone app. Only if sound is late: a small native sound bridge.
- [ ] **P.12** **You:** create the test apps in App Store Connect and Google Play (B.01–B.02 first); TestFlight and Android internal testing for friends.
- [ ] **P.13** Where the phone logs live, written into `CLAUDE.md`.

## Art track (Figma) — alongside steps 2–6, ready for step 7

- [ ] **A.01** **You:** say whether a Figma file for the game already exists. If not, Claude creates one in your team with the 8 pages from the plan.
- [ ] **A.02** Colors as Figma variables, from the Art Bible.
- [ ] **A.03** Spacing, corners and sizes as variables; text styles for the type scale.
- [ ] **A.04** **You:** pick the fonts from 2–3 samples on the board.
- [ ] **A.05** **You:** pick the pad shape from 2–3 options that avoid the classic round Simon look.
- [ ] **A.06** One pad, built in layers: base, highlight, shadow, rim, symbol, halo.
- [ ] **A.07** All four pads.
- [ ] **A.08** Every pad state: resting, lit, pressed, right, wrong, off, add one.
- [ ] **A.09** The four symbols on the 96 px grid, in dark ink.
- [ ] **A.10** Twist icons: Reverse, Shift, Echo.
- [ ] **A.11** Menu icons: play, pause, home, settings, sound, music, buzz, reduce motion, high contrast, help, close, crown, versus, lock, right, wrong, timer, charge pip, player badge.
- [ ] **A.12** The ring: neutral, Player 1 solid, Player 2 segmented, error broken.
- [ ] **A.13** Top bar: names, score pips, rally badge, crowns.
- [ ] **A.14** State banner and Twist card (empty, held, active, off).
- [ ] **A.15** Buttons (four sizes) and panels (glass and high contrast).
- [ ] **A.16** **You:** pick high-contrast colors from 2 options on the board.
- [ ] **A.17** Duel screen in every state: watch, repeat, add one, Twist warning, wrong, hand-over, tap when ready.
- [ ] **A.18** Rally result and match result.
- [ ] **A.19** Title, Home, and the two setup screens.
- [ ] **A.20** Pause card and Settings.
- [ ] **A.21** Tutorial frames.
- [ ] **A.22** Online screens: your games, invite, age question, their turn replayed.
- [ ] **A.23** Profile (rivalries, badges, level) and Collection with shop.
- [ ] **A.24** The first 3–4 board looks for the collection.
- [ ] **A.25** The first 12 badges.
- [ ] **A.26** Crowns, badge frames, win effects.
- [ ] **A.27** Particle pieces: dot, diamond, spark, streak.
- [ ] **A.28** Background, with a lower-glow version.
- [ ] **A.29** Motion page: tap, Shift, hand-over, win, with timings.
- [ ] **A.30** Contrast check of every color pair.
- [ ] **A.31** Export page: every piece named the way the game expects, at the right size.
- [ ] **A.32** Placeholder logo and app icon (final ones wait for the public name).

## Sound track — alongside steps 2–7

- [ ] **S.01** Final four pad tones with clearly different characters; checked on a phone speaker.
- [ ] **S.02** Twist sounds: Reverse, Shift, Echo.
- [ ] **S.03** Player 1 and Player 2 turn motifs.
- [ ] **S.04** Wrong-tap sound, sequence-done sound, ready tick.
- [ ] **S.05** Point and match-win celebrations.
- [ ] **S.06** Menu sounds.
- [ ] **S.07** One quiet menu music loop; an optional tension bed for the final rally.
- [ ] **S.08** Buzz patterns: one per pad, plus sequence done, Twist armed, Shift ticks, wrong, point, match win.
- [ ] **S.09** Mix check on phone speaker, earbuds and Bluetooth.
- [ ] **S.10** **You:** listen on your phone and approve.

## Business track — any time before it's needed

- [ ] **B.01** **You:** Apple Developer Program active (US$99 a year). Needed by the phone track.
- [ ] **B.02** **You:** Google Play Console account (US$25 once). Needed by the phone track.
- [ ] **B.03** Name ideas: a shortlist from Claude.
- [ ] **B.04** **You:** pick a name and have a lawyer check it. Needed by step 11 (invite web address) and step 12.
- [ ] **B.05** **You:** ask a lawyer whether "8+" marketing makes the game "made for children". Needed by step 10.
- [ ] **B.06** **You:** buy the web address (about US$15 a year), after the name. It holds the invite links, app-ads.txt and the privacy pages.
- [ ] **B.07** **You:** put a payment card on the Unity online services account before launch, so going over a free limit never switches the game off.
- [ ] **B.08** **You:** have a lawyer review the privacy policy, terms and age set-up, and say whether you need EU and UK representatives. Needed by step 12.
- [ ] **B.09** First drafts of the privacy policy and terms (Claude), on a free web page until the web address exists. Needed by step 10.
