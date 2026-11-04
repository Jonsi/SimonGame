using System.Collections.Generic;
using SimonDuel.Rules;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace SimonDuel.Game
{
    // Turns presses into pads: mouse, first finger or keys 1–4, the moment they go down.
    // Presses arrive as events, so a click too quick to last a whole frame is still counted.
    public class PadBoard : MonoBehaviour
    {
        [SerializeField] Feel feel;
        [SerializeField] Pad[] pads;
        [SerializeField] DuelRunner runner;

        readonly List<InputAction> actions = new List<InputAction>();
        RectTransform board;
        float secondStart;
        int frames, taps;
        double reactTotal, reactMax;

        void Awake()
        {
            board = (RectTransform)transform;
            board.anchoredPosition = new Vector2(0f, -feel.boardDropFromCenter);
            foreach (var pad in pads) pad.Setup(feel);
        }

        void OnEnable()
        {
            var pointer = new InputAction(type: InputActionType.Button);
            pointer.AddBinding("<Mouse>/leftButton");
            pointer.AddBinding("<Touchscreen>/primaryTouch/press");
            pointer.started += OnPointerPress;
            actions.Add(pointer);

            for (int i = 0; i < pads.Length; i++)
            {
                var pad = pads[i];
                var key = new InputAction(type: InputActionType.Button, binding: $"<Keyboard>/{i + 1}");
                key.started += context => OnKeyPress(pad, context);
                actions.Add(key);
            }

            foreach (var action in actions) action.Enable();
        }

        void OnDisable()
        {
            foreach (var action in actions) action.Dispose();
            actions.Clear();
        }

        public Pad Find(PadId id)
        {
            foreach (var pad in pads)
                if (pad.Id == id) return pad;
            throw new System.ArgumentException($"No pad for {id}");
        }

        void OnPointerPress(InputAction.CallbackContext context)
        {
            if (runner.PressAnywhere()) return;

            var pointer = (Pointer)context.control.device;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(board, pointer.position.ReadValue(), null, out var point);
            foreach (var pad in pads)
            {
                if (!pad.Contains(point)) continue;
                Press(pad, pointer is Touchscreen ? "finger" : "mouse", context.time);
                return;
            }
        }

        void OnKeyPress(Pad pad, InputAction.CallbackContext context)
        {
            if (runner.PressAnywhere()) return;
            Press(pad, "key", context.time);
        }

        void Press(Pad pad, string source, double inputTime)
        {
            runner.PressPad(pad);
            double react = (InputState.currentTime - inputTime) * 1000d;
            taps++;
            reactTotal += react;
            reactMax = System.Math.Max(reactMax, react);
            Debug.Log($"[Touch] pad={pad.Id} from={source} react={react:0.0}ms");
        }

        void Update()
        {
            frames++;
            float elapsed = Time.unscaledTime - secondStart;
            if (elapsed < 1f) return;

            string react = taps > 0 ? $" reactAvg={reactTotal / taps:0.0}ms reactMax={reactMax:0.0}ms" : "";
            Debug.Log($"[Touch] second={Mathf.FloorToInt(Time.unscaledTime)} taps={taps} fps={frames / elapsed:0}{react}");
            secondStart = Time.unscaledTime;
            frames = taps = 0;
            reactTotal = reactMax = 0d;
        }
    }
}
