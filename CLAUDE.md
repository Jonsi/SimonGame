# Simon Duel — house rules for this repo

Memory duel built in Unity under CuzGames: widescreen in a computer browser first, then phone browsers, then iOS/Android apps. Step 1 in progress (4 Nov 2026).

## Where things are

- Rules of the game: `Docs/GDD.md`. Look and sizes: `Docs/ART_BIBLE.md` (wins over the reference pictures). Product calls: `Docs/MARKET_AND_CREATIVE_STRATEGY.md`.
- How we build it, step order, gaps between the docs: `Docs/UNITY_PLAN.md`.
- What to do next: `Docs/TASKS.md`. Work the first unticked task; tick it only after it is checked.
- Owner's picks and the build board link: the project journal (memory files `simon-duel-*`). Read it first.
- Art source: the owner's Figma team (account yonohana95@gmail.com). Never a work account.
- `Art/References/` and `Deliverables/` are mood and decks; don't edit them to match code.

## How to run it

- Unity 6000.3.23f1 (6.3 LTS), project at the repo root. Web build, with the project closed in Unity:
  `/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -projectPath . -executeMethod SimonDuel.EditorTools.WebBuild.Build -logFile Logs/web-build.log`
  With Unity open, use the menu: CuzGames > Build for Web. Output goes to `Builds/Web` (not in git).
  Add `-buildPath <folder> -development` for a test build with pretend-player keys (F1 right, F2 slip, F3 out of time).
- Tests, with the project closed in Unity: same command with `-runTests -testPlatform EditMode -testResults <file>` in place of `-quit` and `-executeMethod ...`.
- Try it on this Mac: `python3 -m http.server 8765 --bind 127.0.0.1 --directory Builds/Web`, then open http://127.0.0.1:8765/.
- Logs: in the browser's developer console while the game runs; the Unity Editor's is `~/Library/Logs/Unity/Editor.log`.
- Phone browser link and phone apps: not set up yet (tasks M.01–M.03, then the phone track).

## Rules while working

- Build in the plan's order, one step the owner can try at a time. Never commit unless the owner asks; after each step say what is built and that it is not committed.
- Rules code stays plain C# with no Unity references, so the online checker can share it.
- Every tunable number lives in a settings asset, never inline.
- Log lines are tagged `[Duel]`, `[Touch]`, `[Audio]`, `[Online]`.
- When the owner is playing in the Unity Editor: don't recompile, stop play, or edit open scripts. Ask first (see the live-app-testing skill).
- Public name is not decided: never put "Simon" in the app's store ID or bundle ID.
