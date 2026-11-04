using UnityEngine;
using UnityEngine.EventSystems;

namespace SimonDuel.Game
{
    // A button grows a little under the pointer, so it feels alive before it is clicked.
    public class HoverPop : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] Feel feel;

        bool over;

        public void OnPointerEnter(PointerEventData eventData)
        {
            over = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            over = false;
        }

        void OnDisable()
        {
            over = false;
            transform.localScale = Vector3.one;
        }

        void Update()
        {
            float target = over && !GameSettings.ReduceMotion ? feel.hoverScale : 1f;
            float scale = Mathf.MoveTowards(transform.localScale.x, target, feel.hoverSpeed * Time.unscaledDeltaTime);
            transform.localScale = Vector3.one * scale;
        }
    }
}
