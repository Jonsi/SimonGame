using UnityEngine;

namespace SimonDuel.Game
{
    // First thing that runs: writes the start-up log line.
    // Browsers hold sound back until the first click, which is the click on the menu.
    public class Boot : MonoBehaviour
    {
        void Start()
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Debug.Log($"[Boot] version={Application.version} platform={Application.platform} mobile={Application.isMobilePlatform} " +
                      $"device=\"{SystemInfo.deviceModel}\" os=\"{SystemInfo.operatingSystem}\" screen={Screen.width}x{Screen.height} " +
                      $"dpi={Screen.dpi:0} safeArea={Screen.safeArea}");
        }
    }
}
