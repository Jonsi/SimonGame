using UnityEngine;
using UnityEngine.EventSystems;

namespace SimonDuel.Game
{
    // First thing that runs: writes the start-up log line and clears the "Tap to start" cover.
    // Browsers only allow sound after the player's first tap, so nothing plays before it.
    public class Boot : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] GameObject tapToStart;

        void Start()
        {
            Debug.Log($"[Boot] version={Application.version} platform={Application.platform} mobile={Application.isMobilePlatform} " +
                      $"device=\"{SystemInfo.deviceModel}\" os=\"{SystemInfo.operatingSystem}\" screen={Screen.width}x{Screen.height} " +
                      $"dpi={Screen.dpi:0} safeArea={Screen.safeArea}");
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (tapToStart == null || !tapToStart.activeSelf) return;
            tapToStart.SetActive(false);
            Debug.Log("[Boot] started by first tap");
        }
    }
}
