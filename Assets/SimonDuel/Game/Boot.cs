using UnityEngine;

namespace SimonDuel.Game
{
    // First thing that runs: writes the start-up log line and clears the "Tap to start" cover.
    // Browsers only allow sound after the player's first tap, so nothing plays before it.
    public class Boot : MonoBehaviour
    {
        [SerializeField] GameObject tapToStart;

        void Start()
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Debug.Log($"[Boot] version={Application.version} platform={Application.platform} mobile={Application.isMobilePlatform} " +
                      $"device=\"{SystemInfo.deviceModel}\" os=\"{SystemInfo.operatingSystem}\" screen={Screen.width}x{Screen.height} " +
                      $"dpi={Screen.dpi:0} safeArea={Screen.safeArea}");
        }

        // Clears the cover on the first press. Returns true if this press was the one that did it,
        // so the starting tap never also presses a pad.
        public bool TryStart()
        {
            if (!tapToStart.activeSelf) return false;

            tapToStart.SetActive(false);
            Debug.Log("[Boot] started by first tap");
            return true;
        }
    }
}
