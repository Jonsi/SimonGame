# Simon Duel — Art and UI Bible

**Status:** Art-direction baseline  
**Reference canvas:** 1080 × 1920 portrait  
**Purpose:** Define a consistent visual target before Unity implementation.

**Naming note:** Simon Duel is an internal codename. Public branding and the final board silhouette require professional name and trade-dress clearance.

## 1. Art direction

Simon Duel combines the clarity of a physical memory toy with the energy of a modern competitive mobile game.

### Keywords

**Tactile · luminous · friendly · focused · competitive · premium casual**

### Visual hierarchy

1. Active sequence pad.
2. Current player and Twist rule.
3. Shared board and progress ring.
4. Rally score.
5. Twist inventory and secondary controls.

If an effect competes with sequence readability, the effect loses.

## 2. Approved references

- [Gameplay/UI direction](../Art/References/simon-duel-ui-direction.png)
- [Visual-language sheet](../Art/References/simon-duel-visual-language.png)
- [Screen-family reference](../Art/References/simon-duel-screen-family.png)
- [Motion and feedback reference](../Art/References/simon-duel-motion-feedback.png)
- [Exact palette board](../Art/References/simon-duel-palette.svg)
- [Gameplay layout specification](../Art/References/simon-duel-layout-spec.svg)

Generated reference images communicate style and mood. This document and the SVG boards are authoritative for exact colors, measurements, and labels.

## 3. Color system

### Foundation

| Token | Hex | Use |
|---|---|---|
| Night 1000 | `#030817` | Outer background, deepest vignette |
| Navy 950 | `#050B1D` | Primary screen background |
| Navy 900 | `#07142B` | Board cavity, modal backdrop |
| Navy 800 | `#0B2242` | Raised panels and cards |
| Navy 700 | `#12345C` | Elevated surfaces |
| Stroke 500 | `#2A527D` | Panel outline and inactive ring |
| Glass | `#17385FCC` | Glass panel fill at 80% alpha |

### Text and neutral

| Token | Hex | Use |
|---|---|---|
| Text Primary | `#F7FBFF` | Titles and primary content |
| Text Secondary | `#B7CAE0` | Supporting content |
| Text Muted | `#8199B5` | Captions and inactive labels |
| Disabled | `#536A86` | Disabled icons and pips |
| Dark Ink | `#07142B` | Symbols on yellow and mint |

### Pad identities

| Identity | Base | Highlight | Shadow | Symbol | Meaning |
|---|---|---|---|---|---|
| Circle / Coral | `#FF5A52` | `#FF9188` | `#B92F3C` | `#07142B` | Low tone; rounded pulse |
| Triangle / Sun | `#FFC62E` | `#FFE477` | `#C88900` | `#07142B` | Bright tone; crisp pulse |
| Square / Mint | `#42D79A` | `#87EBC2` | `#168E67` | `#07142B` | Mid tone; double soft pulse |
| Star / Electric | `#2B9DF4` | `#79C9FF` | `#125FAF` | `#07142B` | High tone; pulse with tail |

### Player and system accents

| Token | Hex | Use |
|---|---|---|
| Player 1 Orange | `#FF9418` | Player 1 focus, crown, active border |
| Player 1 Light | `#FFC557` | Orange highlight and glow core |
| Player 2 Cyan | `#20D7ED` | Player 2 focus, crown, active border |
| Player 2 Light | `#78F0FF` | Cyan highlight and glow core |
| Twist Violet | `#A664FF` | Rare/system Twist emphasis |
| Success | `#73E36A` | Correct completion only |
| Error | `#FF4658` | Incorrect input and destructive actions |
| Warning | `#FFB020` | Timer urgency and caution |

### Color usage rules

- Use Navy 950 as the normal screen field; Night 1000 appears at edges and behind overlays.
- Keep large surfaces neutral so pad identities remain dominant.
- Never recolor pad identity to show player ownership.
- Player orange/cyan belongs to rings, labels, crowns, and borders.
- Success green is feedback, not a fifth gameplay pad.
- Error red must use an X, shake, or broken ring so it cannot be confused with the coral Circle pad.
- Bloom is a derived effect, not a replacement for a solid silhouette.
- Maximum simultaneous saturated accents during normal play: the four pads plus one active player color.
- Player 1 ownership uses a solid perimeter/ring pattern; Player 2 uses a segmented pattern so ownership does not rely on orange/cyan alone.

### Contrast audit

The August 2026 accessibility audit found that white on Coral (`2.95:1`) and white on Electric (`2.79:1`) miss the `3:1` meaningful non-text contrast target. Dark Ink is now authoritative for those pad glyphs (`5.98:1` and `6.32:1` respectively against the base colors). If a future theme retains a white glyph, it requires a thick continuous Dark Ink keyline that remains visible at the least-contrasting point of every gradient and glow state.

- Stroke 500 on Navy 800 is decorative only; it is approximately `1.97:1`.
- Use `#4679A8` or brighter for meaningful interactive outlines on Navy 800.
- Disabled `#536A86` is not permitted for small functional text.
- Use Text Muted `#8199B5` or brighter for small secondary text on Navy 800.
- Validate all final themes against WCAG 2.2 text, non-text contrast, and use-of-color criteria.

## 4. Typography

### Typeface direction

- **Display:** rounded geometric sans, wide and confident, weight 700–800.
- **UI:** clean geometric sans, weight 500–700.
- **Numbers:** tabular figures when available.
- Avoid condensed, handwritten, serif, or highly futuristic type.

Suggested free-font direction for later evaluation: **Nunito Sans**, **Rubik**, or **Baloo 2** for display paired with **Inter** or **Nunito Sans** for UI. Font licensing and final selection are deferred.

### Type scale on 1080 × 1920 baseline

| Style | Size | Line height | Weight | Use |
|---|---:|---:|---:|---|
| Logo | 96 | 104 | 800 | Title screen only |
| Display | 72 | 80 | 800 | Match winner |
| H1 | 56 | 64 | 800 | Screen title |
| H2 | 44 | 52 | 700 | Turn and Twist banners |
| H3 | 36 | 44 | 700 | Modal and section title |
| Body Large | 32 | 42 | 600 | Primary instructions |
| Body | 28 | 38 | 500 | Standard UI copy |
| Label | 24 | 30 | 700 | Buttons and score names |
| Caption | 20 | 26 | 600 | Secondary statistics |

### Typography rules

- Use uppercase for short state labels: WATCH, REPEAT, ADD ONE, REVERSE.
- Keep instructional copy sentence case.
- Maximum tutorial line length: 32 characters where practical.
- Center gameplay-state copy; left-align settings and long-form content.
- Use a soft navy shadow or panel behind text over effects.
- Never render essential text below 20 baseline pixels.

## 5. Spatial system

### Baseline and scaling

- Design at 1080 × 1920 portrait.
- Scale the whole layout proportionally to device width.
- Extend the background to fill unusual aspect ratios.
- Preserve safe areas before compressing the board.
- Landscape is not part of the initial visual target.

### Spacing scale

| Token | Pixels | Typical use |
|---|---:|---|
| 1 | 8 | Icon optical adjustment |
| 2 | 16 | Tight internal spacing |
| 3 | 24 | Standard component gap |
| 4 | 32 | Card padding |
| 6 | 48 | Section spacing |
| 8 | 64 | Screen side margin |
| 12 | 96 | Major separation / safe inset |

All spacing should resolve to multiples of 8 unless optical alignment requires a small exception.

### Safe areas

- Left/right content inset: 64 px.
- Minimum top safe inset: 96 px.
- Minimum bottom safe inset: 120 px.
- Interactive content width: 952 px.
- Keep the main board clear of camera cutouts and home indicators.

## 6. Gameplay screen geometry

Authoritative baseline measurements:

| Region | Position / size |
|---|---|
| Top safe area | `y 0–96` |
| Score header | `x 64, y 104, w 952, h 176` |
| State/Twist banner | `x 128, y 304, w 824, h 128` |
| Board outer bounds | `x 148, y 468, w 784, h 784` |
| Sequence ring diameter | `824` centered on board |
| Main pad visual bounds | approximately `336 × 300` each |
| Gap between pad silhouettes | `24` minimum |
| Minimum pad hit region | `360 × 324` each |
| Twist dock | `x 96, y 1320, w 888, h 208` |
| Progress / help region | `x 160, y 1560, w 760, h 104` |
| Footer controls | `x 64, y 1696, w 952, h 104` |
| Bottom safe area | `y 1800–1920` |

The board may shift vertically by up to 80 px on compact devices, but its center and pad hit regions remain the visual priority.

## 7. Component specifications

### Main pads

- Four rounded quadrant forms around a small central breathing gap.
- Visual size: about 336 × 300 px each on baseline.
- Hit size: at least 360 × 324 px each.
- Outer corner radius: 72 px.
- Inner corner radius: 44 px.
- Bevel band: 16 px.
- Resting elevation: 12 px visual depth.
- Symbol: 112 × 112 px optical box, 16–20 px stroke.
- Pad symbol remains upright during Shift movement.

### Sequence ring

- Outer diameter: 824 px.
- Standard stroke: 12 px.
- Active glow core: 8 px plus 20–32 px bloom.
- Four subtle cardinal markers may indicate pad slots.
- Timer depletion proceeds clockwise in normal play.
- Reverse proceeds counter-clockwise and adds arrowheads.

### Score header

- Height: 176 px.
- Glass fill over solid Navy 900 support.
- Player names use Label style.
- Three rally pips per player; pip diameter 34 px; gap 18 px.
- Active crown optical size: 52 × 40 px.
- Center rally badge: 96 × 96 px.

### Twist card

- Full card: 160 × 208 px.
- Compact dock slot: 136 × 176 px.
- Corner radius: 28 px.
- Border: 4 px neutral; 6 px active player accent.
- Icon optical box: 72 × 72 px.
- Active elevation: 16 px plus glow.
- Held-card maximum: one per player in the core design.

### Buttons

| Type | Height | Radius | Horizontal padding |
|---|---:|---:|---:|
| Primary | 104 | 36 | 48 |
| Secondary | 88 | 30 | 40 |
| Compact | 72 | 24 | 28 |
| Icon only | 80 × 80 | 28 | n/a |

Minimum separation between unrelated buttons is 24 px.

### Panels and modals

- Standard card radius: 32 px.
- Large modal radius: 48 px.
- Card border: 2 px at Stroke 500.
- Interior padding: 32–48 px.
- Modal backdrop: Night 1000 at 72% opacity.
- High Contrast mode uses opaque Navy 900 panels.

## 8. Pad state language

| State | Scale / position | Color and light | Effect |
|---|---|---|---|
| Idle | 100%, elevated 12 px | Base gradient, low rim light | Very slow 2% breathing only outside Reduce Motion |
| Playback | 104% | Highlight toward center, 24 px bloom | Ring ripple; tone and haptic |
| Pressed | 96%, depth reduced to 3 px | Slightly darker base | Fast squash, no bloom spike |
| Correct step | Return to 100% | 100 ms Success rim | Small outward particles |
| Wrong step | 98% lateral shake | Error outline; keep identity color visible | Broken-ring sparks and expected-pad ghost |
| Disabled | 100% | 45% saturation and 55% brightness | No breathing or glow |
| Add One eligible | 101% | All pads receive subtle white rim | Central prompt and shared pulse |

## 9. Motion language

### Principles

- Motion is fast, elastic, and purposeful.
- Gameplay response begins immediately on touch-down.
- Circular motion communicates sequence, turn transfer, and Reverse.
- Scale motion communicates touch and success.
- Lateral motion is reserved for errors.

### Timing and easing

| Motion | Duration | Character |
|---|---:|---|
| Tap squash | 70 ms in / 110 ms out | Sharp ease-out, soft overshoot |
| Playback glow | 420 ms | Smooth in/out |
| Correct ripple | 260 ms | Fast expansion and fade |
| Error shake | 320 ms | Three diminishing offsets |
| Turn transfer | 450 ms | Circular sweep |
| Shift move | 650 ms | Clear one-slot clockwise arc with ease-in-out |
| Twist card arm | 380 ms | Lift, tilt, settle |
| Rally point pip | 500 ms | Pop with one overshoot |
| Result modal | 420 ms | Scale 92% to 100% plus fade |

### Reduced Motion

- No pad breathing.
- Shift crossfades positions with numbered slot markers instead of orbiting.
- Turn transfer uses color fade, not a sweeping ring.
- Result screen uses fade only.
- Essential state changes remain at least 150 ms so they are perceivable.

## 10. Effects budget and style

- Use simple circles, diamonds, short trails, and ring segments.
- Standard correct tap: maximum 12 particles.
- Twist activation: maximum 24 particles.
- Rally win: maximum 48 particles, short-lived.
- Match win: one larger burst, then settle within three seconds.
- Particle lifetime: generally 0.25–0.8 seconds.
- Avoid persistent snow, dust, smoke, or screen-filling confetti during gameplay.
- Bloom should never obscure a symbol edge.
- Screen shake is not part of the default style; use a tiny board nudge only for a wrong input.

## 11. Iconography

- Rounded geometric construction.
- Standard stroke: 12 px in a 96 px icon box.
- Round caps and joins.
- One primary metaphor per icon.
- Avoid thin interior details.
- Mirror directional icons correctly for right-to-left localization when meaning depends on reading direction; Reverse itself remains circular.

### Required icon set

- Circle, triangle, square, star identities.
- Reverse, Shift, Echo, Rush, Blackout.
- Play, pause, resume, restart, home, settings, sound, music, haptics.
- Reduce Motion, High Contrast, help, close, crown, versus, lock.
- Correct, wrong, timer, charge pip, player badge.

## 12. Logo direction

- Wordmark: “SIMON DUEL.”
- SIMON carries warm yellow-to-orange energy; DUEL carries cyan-to-blue energy.
- One small crown over each side communicates equal competitors.
- Use a wide rounded geometric wordmark with a subtle beveled face.
- Logo must also work as a flat one-color silhouette.
- App icon should use the shared circular board or an orange/cyan versus crown, not the full wordmark.

The current generated logo is directional reference only; final trademark review and vector construction are required later.

## 13. Screen-specific art notes

### Home

- Logo occupies the upper third.
- Shared board silhouette provides the hero object.
- Primary DUEL button is unmistakable.
- Secondary modes remain visually quieter.

### Player setup

- Player 1 and Player 2 occupy equal visual weight.
- Orange and cyan frames never overlap text fields.
- Random-start explanation is secondary.

### Gameplay

- Board owns at least 40% of usable vertical area.
- Active state is readable through crown, name, ring color, and banner.
- Twist dock never covers a pad hit region.

### Rally result

- Show expected sequence ending and incorrect step briefly.
- Winning player accent surrounds the point pip.
- Rematch is not shown until the match is complete.

### Match result

- Winner name and 3-pip score dominate.
- Confetti uses both identity colors and the winner accent.
- Rematch uses the winner accent only as a border; the fill remains neutral for fairness.

## 14. Responsive rules

- Reference aspect ratio: 9:16.
- On taller screens, add space above the header and below the footer; do not enlarge pads beyond 840 px board diameter.
- On shorter screens, reduce gaps before reducing pad size.
- Minimum supported visual board diameter target: 680 baseline px.
- If system text scaling is supported, allow labels to wrap or expand panels rather than clipping.
- Gameplay-state labels remain single-line through localized short-form strings.

## 15. Asset production matrix

| Asset | Source format | Working size | Variants |
|---|---|---:|---|
| Pad silhouette | Vector or layered raster | 512 × 512 | 4 identities; state tintable |
| Pad symbol | Vector | 256 × 256 | 4 |
| Twist icon | Vector | 256 × 256 | 5 + future |
| Player crown | Vector | 256 × 192 | Orange, cyan, neutral |
| Card frame | 9-slice raster/vector | 256 × 320 | Idle, held, active, disabled |
| Panel frame | 9-slice raster/vector | 256 × 256 | Glass, opaque high-contrast |
| Ring segment | Vector/procedural target | 512 × 512 | Neutral, player accents, error |
| Particles | Raster sprites | 64–128 | Dot, diamond, spark, streak |
| Logo | Vector master | 2048 px wide | Full color, flat light, flat dark |
| App icon master | Layered square | 2048 × 2048 | Platform crops later |
| Background | Layered raster | 2160 × 3840 | Standard, reduced bloom |

These are source-art targets, not engine import settings.

## 16. Naming convention for art sources

`SD_<Category>_<Element>_<State>_<Variant>`

Examples:

- `SD_Pad_Circle_Idle_Default`
- `SD_Pad_Triangle_Playback_Default`
- `SD_Icon_Twist_Reverse_Default`
- `SD_Panel_ScoreHeader_Glass`
- `SD_FX_Correct_Diamond`

## 17. Art acceptance checklist

- Reads at intended phone size.
- Matches an approved palette token.
- Preserves symbol and tone identity.
- Has no essential detail below 4 baseline pixels.
- Does not rely on bloom for its silhouette.
- Supports Idle, Pressed, Playback, Correct, Wrong, and Disabled states where applicable.
- Works against Navy 950 and Navy 900.
- Has an opaque or high-contrast alternative when needed.
- Fits the spacing and radius system.
- Is free of unlicensed marks or derivative branding.

## 18. Deferred technical decisions

- Sprite versus vector rendering method.
- Shader implementation.
- Atlas strategy and compression.
- Canvas scaling setup.
- Safe-area implementation.
- Animation system and tween library.
- Particle system choice.
- Dynamic material and accessibility switching.

These should be resolved in the Unity technical design phase after the layout and game rules are playtested.
