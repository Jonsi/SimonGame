# Simon Duel — Game Design Document

**Status:** Pre-production design baseline  
**Version:** 0.1  
**Primary platform:** iOS and Android phones  
**Orientation:** Portrait  
**Working title:** Simon Duel — internal codename only, pending professional name and trade-dress clearance

## 1. High concept

Simon Duel is a fast, social memory game in which two players build and repeat one shared sequence. Every successful turn makes the pattern longer. Players can occasionally apply a clearly announced Twist that changes how the opponent must read or enter the next sequence.

The game should be understandable in under 30 seconds, produce tension within the first minute, and finish a match in roughly 2–5 minutes.

### Player promise

> “I can learn it immediately, out-remember my friend, and create a dramatic comeback with one clever Twist.”

### Design pillars

1. **Instantly understandable** — watch, repeat, add one.
2. **Social tension** — every added step is a direct challenge to the other player.
3. **Fair surprises** — Twists are announced before input and never hide the rules.
4. **Juicy clarity** — light, sound, motion, and haptics confirm every meaningful event.
5. **Short rematches** — a complete match is quick enough to replay immediately.

## 2. Audience and tone

- Broad casual audience, approximately ages 8+.
- Friends, siblings, couples, families, and short party sessions.
- Suitable for players with little gaming experience.
- Friendly rivalry rather than combat, humiliation, or aggressive taunting.
- Bright, tactile, energetic presentation over a calm dark background.

## 3. Product scope

### First playable

- Local Pass & Play for two players on one phone.
- Best-of-five match: first player to win three rallies wins.
- Four input pads with unique colors, shapes, tones, and haptic identities.
- Two Twists: Reverse and Shift.
- Tutorial, pause, rematch, sound, haptics, reduced motion, and color-accessible presentation.

### Recommended version 1.0

- Pass & Play Duel.
- Solo Practice.
- Party Elimination for 3–8 local players.
- Five Twists: Reverse, Shift, Echo, Rush, and Blackout.
- Player names, local statistics, cosmetic board themes, and a lightweight unlock track.

### Later expansion

- Asynchronous Daily Challenge with a shared seed.
- Friend challenges.
- Live online duels, matchmaking, reconnection, latency handling, moderation, and anti-cheat.

Online multiplayer is intentionally outside the current design phase. It must not distort the simplicity of the local game.

## 4. Core vocabulary

| Term | Meaning |
|---|---|
| Match | The complete contest; first to three rally wins. |
| Rally | One growing sequence, ending when a player fails. |
| Turn | One player watches, repeats, and extends the sequence. |
| Sequence | The ordered list of pad identities. |
| Step | One pad identity inside the sequence. |
| Pad | One of the four colored and symbol-marked inputs. |
| Twist | A one-turn modifier applied to the opponent. |
| Charge | Progress earned toward receiving a Twist. |

## 5. Core rules

### Match structure

1. Both players enter names or accept “Player 1” and “Player 2.”
2. The starting player is chosen randomly for the first rally.
3. Starting player alternates after each rally.
4. The system seeds the rally with a two-step sequence.
5. The active player repeats the sequence from memory, then adds one new step. The sequence is shown once when the rally starts; after that, only each new step is shown, once, at a fixed speed, for both players.
6. The opponent receives the longer sequence and does the same.
7. A mistake or timeout ends the rally; the opponent earns one rally point.
8. First to three rally points wins the match.

### Why the player adds a step

The added step gives each turn authorship. Players do not merely survive a system-generated test; they actively construct the challenge that may defeat their opponent.

### Sequence identity

Every step refers to a **pad identity**, not a screen position. A pad identity always keeps its color, symbol, tone, and haptic signature together. If Shift changes positions, all four identity cues move together.

### Input rules

- Input is locked during playback.
- A correct tap illuminates and depresses the selected pad immediately.
- A wrong tap stops entry and reveals the expected step.
- After repeating the full sequence correctly, the interface enters **ADD ONE** state.
- The added step plays once for both players and becomes the final step of the sequence.
- Rapid accidental multi-touch is ignored; only the first valid touch is accepted.
- Leaving the app pauses local play and obscures the sequence.

## 6. Turn flow

1. **Ownership** — active player color, name, and crown become prominent.
2. **Twist warning** — if a Twist is active, its name and rule appear briefly.
3. **Playback** — only on a rally's first turn: the full sequence plays once. After that the turn goes straight to Repeat.
4. **Ready beat** — a short visual breath separates playback from input.
5. **Repeat** — the player enters the complete sequence.
6. **Extend** — “ADD ONE” appears and the player chooses a new step.
7. **Handoff** — ownership changes and the next turn begins.

### Target timing

| Event | Target duration |
|---|---:|
| Turn ownership banner | 0.55 s |
| Twist explanation | 1.25 s first encounter; 0.65 s later |
| Standard pad illumination | 0.42 s |
| Gap between playback steps | 0.14 s |
| Ready beat | 0.35 s |
| Correct tap response | under 0.08 s perceived latency |
| Rally loss reveal | 1.10 s |
| Point celebration | 1.40 s |
| Match victory celebration | 2.50–3.50 s, skippable |

Playback accelerates gently as the rally grows, but never below 0.32 seconds of illumination plus a 0.10-second gap. Speed is a tension tool, not a readability trap.

## 7. Twists

### Twist economy

- Twists are disabled for the first two completed turns of a rally.
- Each successful turn fills one of three Charge pips.
- At three Charge, the player receives one randomly selected Twist.
- A player may hold only one Twist.
- A Twist is armed after successfully extending the sequence and affects the opponent’s next turn.
- Only one Twist may affect a turn; Twists never stack.
- Using a Twist consumes it and resets that player’s Charge.
- Twist selection uses a controlled bag to reduce repeats.

This cadence makes Twists occasional dramatic moments rather than constant noise.

### Twist set

| Twist | Rule | Presentation | Release |
|---|---|---|---|
| Reverse | Repeat the sequence from last step to first. | Circular reverse arrows; sequence ring travels counter-clockwise. | First playable |
| Shift | After playback, all pad identities rotate one slot clockwise. Repeat by identity, not old position. | Pads visibly orbit into new slots; color, symbol, tone remain paired. | First playable |
| Echo | One clearly marked step is flagged and must be entered twice. | Concentric pulse and paired beat marker. | 1.0 |
| Rush | Playback and input pulse run faster for one turn. | Forward streaks and accelerating ring. | 1.0 |
| Blackout | One step is presented by tone and haptic only. | Pad silhouette dims; audio-wave marker appears. | 1.0 |

### Twist fairness rules

- The active rule is always named before playback.
- First encounter includes a one-sentence explanation and demonstration.
- Shift completes before input begins.
- Blackout is unavailable when game audio and haptics are both disabled.
- No Twist changes more than one mental rule at a time.
- Reduced-motion mode replaces orbiting or spinning transitions with crossfades and position markers.

## 8. Difficulty and pacing

Difficulty emerges mainly from sequence length, then from modest tempo increases and occasional Twists.

| Sequence length | Intended feeling | Playback |
|---:|---|---|
| 2–4 | Learn and settle in | Slow and generous |
| 5–7 | Focused competition | Standard |
| 8–11 | Rising tension | Slightly faster |
| 12–15 | High-skill rally | Fast but readable |
| 16+ | Showcase territory | Minimum timing cap |

There is no hidden input-speed scaling during ordinary turns. The player’s memory should fail before the interface does.

### Timeout

The input timer is communicated by the outer ring. It begins after the Ready beat and pauses briefly after each accepted tap. The initial target budget is:

`3.0 seconds + 0.75 seconds per sequence step`, capped at 14 seconds.

This is a tuning baseline, not an implementation contract.

## 9. Game modes

### Duel — primary mode

- Two local players.
- Best of five rallies.
- Full Twist system.
- Target match length: 2–5 minutes.

### Solo Practice

- System continuously extends the sequence.
- No match score or opponent.
- Player chooses Classic, Twist Practice, or Endless.
- Supports learning and accessibility calibration.

### Party Elimination

- 3–8 names in a circular turn order.
- One failed turn removes one life; default is one life.
- Sequence resets after an elimination to keep waiting time reasonable.
- Last remaining player wins.

### Daily Challenge — later

- Same deterministic sequence and Twist schedule for everyone that day.
- One practice attempt and one ranked attempt.
- Friends compare maximum completed length and total response time.

## 10. Scoring and results

The primary duel score is rally wins, shown as three pips per player. Secondary statistics exist for celebration and mastery, not to override the winner.

Results show:

- Winner.
- Final rally score.
- Longest sequence reached.
- Best response streak.
- Twists successfully survived.
- “Rematch” as the dominant action.
- “Change Players” and “Home” as secondary actions.

Starting player flips automatically on rematch.

## 11. Onboarding

The first session teaches through play in under one minute:

1. “WATCH” — show a two-step sequence.
2. “REPEAT” — player taps the same two pads.
3. “ADD ONE” — player chooses a third pad.
4. Demonstrate the opponent repeating the longer pattern.
5. Explain: “Make a mistake and your opponent scores.”
6. Introduce Reverse only after both players have completed one normal turn.

Tutorial copy must fit on one line where practical. Demonstration is preferred over paragraphs.

## 12. Controls and feedback

### Touch

- Pads respond on touch-down for immediacy.
- Visible pad hit area extends beyond the art silhouette.
- Minimum interactive target: 96 × 96 design pixels on the 1080 × 1920 baseline.
- Main pads are far larger than the minimum and support relaxed thumbs.
- Taps outside valid controls do nothing and never count as a sequence mistake.

### Audio identities

- Circle / coral: low rounded tone.
- Triangle / yellow: bright upper-mid tone.
- Square / mint: warm mid tone.
- Star / blue: clear high tone.
- Tones form a pleasant four-note chord when heard together.

### Haptic identities

- Circle: one soft pulse.
- Triangle: one crisp pulse.
- Square: two short soft pulses.
- Star: one short plus one lighter tail.

Haptic differences are supportive; players are never required to identify a pad through haptics alone except in an explicitly enabled Blackout experience.

## 13. Accessibility

- Color is never the only information channel.
- Every pad has a permanent unique symbol and tone.
- Yellow and mint use dark symbol ink for contrast; coral and blue use light symbol ink.
- Color-blind presets may adjust hue while preserving names and symbols.
- Reduce Motion replaces spins, orbits, and large scale changes.
- Haptics and music have independent toggles.
- Effects, music, and gameplay tones have separate volume controls.
- High Contrast strengthens edges, removes glass transparency, and reduces bloom.
- Tutorial demonstrations can be replayed.
- No essential information depends on text smaller than the defined caption size.
- Flash effects avoid rapid full-screen luminance changes.

## 14. Visual direction summary

- Deep navy stage with luminous, soft-plastic controls.
- Four pads read like satisfying physical toys, not flat dashboard buttons.
- Glass panels organize information without competing with gameplay.
- Player 1 uses warm orange; Player 2 uses cool cyan.
- White, yellow, and cyan highlights are reserved for focus and feedback.
- Motion follows circular paths around the shared board to reinforce “one shared sequence.”
- Crowns, score pips, and edge lighting establish friendly competition.

Exact palette, typography, geometry, state treatments, and export sizes are defined in [ART_BIBLE.md](./ART_BIBLE.md).

## 15. Audio direction summary

- Clean synthesized tones with short tails and no harsh transients.
- Musical key should support all sequence combinations without dissonant fatigue.
- UI sounds are quieter and shorter than gameplay tones.
- Player 1 feedback has a subtle warm/percussive flavor; Player 2 has a subtle airy/glassy flavor.
- Music remains sparse during sequence playback and rises between rallies.

Detailed production and integration decisions are deferred to the technical phase.

## 16. Progression and cosmetics

Progression must remain cosmetic and never alter competitive rules.

- Complete matches to earn Sparks.
- Unlock board materials, backgrounds, particle styles, and player badges.
- Every theme preserves the four identity symbols and minimum contrast.
- No consumable Twist purchases.
- No stat boosts.
- No random paid rewards.

Monetization model is undecided. If ads are used, they must never interrupt a rally or appear between every rematch.

## 17. UX screen list

### Required for first playable

1. Boot / title.
2. Home.
3. Local player setup.
4. How to Play.
5. Duel gameplay.
6. Pause / settings overlay.
7. Rally result.
8. Match result.

### Version 1.0 additions

9. Mode select.
10. Solo setup.
11. Party setup.
12. Cosmetics.
13. Statistics.
14. Accessibility preview.

## 18. Success criteria

The design is succeeding when:

- A new player can explain the core rule after one demonstrated turn.
- Players can identify all four pads without relying on color.
- A wrong input feels attributable to memory, not unclear UI.
- Most first matches reach at least a six-step sequence.
- Rematch is selected frequently after close matches.
- Twists cause anticipation and laughter without requiring rule debate.
- The screen remains readable on a small phone and in reduced-motion mode.

## 19. Open design questions for playtesting

- Is a two-step seed more satisfying than a one-step seed?
- Should the active player choose when to arm a Twist, or should it trigger automatically at full Charge?
- Does the first-playable one-slot clockwise Shift create enough challenge, or should later tests add a clearly announced counter-clockwise variant?
- Is best-of-five the ideal match length for local play?
- Does the timeout add healthy tension or punish thoughtful players?
- How many normal turns should occur before the first Twist?
- Can nearby spectators follow the sequence and score at a glance?

## 20. Out of scope for this phase

- Unity architecture and packages.
- Scene hierarchy and prefab structure.
- Networking technology.
- Save format and backend services.
- Analytics SDK selection.
- Store integration.
- Asset import settings and performance implementation.

Those decisions should follow a paper prototype or clickable UX test using this design baseline.
