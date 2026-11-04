using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SimonDuel.Rules;
using UnityEngine;

namespace SimonDuel.Game
{
    // Keeps a summary of each finished match in this browser, one per line, until they are downloaded.
    public static class MatchNotes
    {
        const string Key = "CuzGames.MatchNotes";
        const string FileName = "duel-match-notes.txt";
        const int MaxMatches = 200;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void DownloadTextFile(string fileName, string text);
#endif

        public static int Count => Lines().Count;

        public static void Add(MatchSummary summary, int seconds)
        {
            summary.seconds = seconds;
            summary.finishedAtUtc = DateTime.UtcNow.ToString("u");

            var lines = Lines();
            lines.Add(JsonUtility.ToJson(summary));
            if (lines.Count > MaxMatches) lines.RemoveRange(0, lines.Count - MaxMatches);
            PlayerPrefs.SetString(Key, string.Join("\n", lines));
            PlayerPrefs.Save();
            Debug.Log($"[Duel] notes saved matches={lines.Count}");
        }

        public static void Download()
        {
            string text = PlayerPrefs.GetString(Key, "");
#if UNITY_WEBGL && !UNITY_EDITOR
            DownloadTextFile(FileName, text);
#else
            string path = System.IO.Path.Combine(Application.persistentDataPath, FileName);
            System.IO.File.WriteAllText(path, text);
            Debug.Log($"[Duel] notes written to {path}");
#endif
        }

        static List<string> Lines()
        {
            string text = PlayerPrefs.GetString(Key, "");
            return text.Length == 0 ? new List<string>() : new List<string>(text.Split('\n'));
        }
    }
}
