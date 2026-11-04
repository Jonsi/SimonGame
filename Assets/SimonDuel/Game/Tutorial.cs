using System.Collections;
using System.Collections.Generic;
using SimonDuel.Rules;
using UnityEngine;

namespace SimonDuel.Game
{
    // Learn by playing: watch, repeat, add one, see the other side copy it, then one Twist.
    public class Tutorial : MonoBehaviour
    {
        [SerializeField] PadBoard board;
        [SerializeField] DuelScreen screen;
        [SerializeField] SoundBoard sounds;
        [SerializeField] Timing timing;

        const string DoneKey = "CuzGames.TutorialDone";

        Pad pressed;
        bool listening;

        public static bool Done => PlayerPrefs.GetInt(DoneKey, 0) == 1;

        public void Press(Pad pad)
        {
            if (listening) pressed = pad;
        }

        public IEnumerator Run()
        {
            PlayerPrefs.SetInt(DoneKey, 1);
            PlayerPrefs.Save();
            screen.ShowTutorialLook();

            var pattern = new List<PadId> { PadId.Circle, PadId.Star };
            yield return Say("LEARN BY PLAYING");
            yield return Task("WATCH THE PADS", "NOW PRESS THE SAME PADS", pattern, false);

            screen.ShowBanner("ADD ONE: PRESS ANY PAD");
            yield return WaitForPress();
            pressed.Press();
            pattern.Add(pressed.Id);
            sounds.Play(GameSound.Done);
            yield return Say("THAT STEP IS NOW PART OF THE PATTERN");

            screen.ShowBanner("THE OTHER PLAYER MUST COPY IT ALL");
            yield return PlayPattern(pattern, true);
            var added = pattern[pattern.Count - 1] == PadId.Triangle ? PadId.Square : PadId.Triangle;
            pattern.Add(added);
            board.Find(added).Press();
            yield return Say("AND ADDS ONE MORE FOR YOU");

            yield return Task("YOUR TURN: WATCH", "REPEAT THE LONGER PATTERN", pattern, false);

            sounds.Play(GameSound.Twist);
            yield return Say("TWISTS CHANGE THE RULE FOR ONE TURN");
            yield return Task("REVERSE: WATCH", "NOW PRESS IT BACK TO FRONT", pattern, true);

            yield return Say("ONE SLIP AND THE OTHER PLAYER SCORES");
            sounds.Play(GameSound.Point);
            yield return Say("FIRST TO 2 POINTS WINS. GOOD LUCK!");
        }

        IEnumerator Task(string watch, string ask, List<PadId> pattern, bool backwards)
        {
            var answer = new List<PadId>(pattern);
            if (backwards) answer.Reverse();

            while (true)
            {
                screen.ShowBanner(watch);
                yield return PlayPattern(pattern, false);
                screen.ShowBanner(ask);

                bool slipped = false;
                foreach (var step in answer)
                {
                    yield return WaitForPress();
                    if (pressed.Id == step)
                    {
                        pressed.Press();
                        continue;
                    }

                    pressed.ShowWrong();
                    sounds.Play(GameSound.Wrong);
                    slipped = true;
                    break;
                }

                if (!slipped) break;
                yield return Say("NOT QUITE. WATCH AGAIN");
            }

            sounds.Play(GameSound.Done);
            yield return new WaitForSeconds(timing.tutorialPauseSeconds);
        }

        IEnumerator PlayPattern(List<PadId> pattern, bool asPresses)
        {
            foreach (var step in pattern)
            {
                var pad = board.Find(step);
                if (asPresses) pad.Press();
                else pad.Play(timing.tutorialStepSeconds);
                yield return new WaitForSeconds(timing.tutorialStepSeconds + timing.tutorialGapSeconds);
            }
        }

        IEnumerator Say(string line)
        {
            screen.ShowBanner(line);
            yield return new WaitForSeconds(timing.tutorialLineSeconds);
        }

        IEnumerator WaitForPress()
        {
            pressed = null;
            listening = true;
            while (pressed == null) yield return null;
            listening = false;
        }
    }
}
