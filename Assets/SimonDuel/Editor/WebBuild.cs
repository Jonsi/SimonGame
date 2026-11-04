using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SimonDuel.EditorTools
{
    // Builds the browser version. From the menu: CuzGames > Build for Web.
    // From a terminal: Unity -batchmode -quit -projectPath . -executeMethod SimonDuel.EditorTools.WebBuild.Build [-buildPath <folder>]
    public static class WebBuild
    {
        const string DefaultPath = "Builds/Web";

        [MenuItem("CuzGames/Build for Web")]
        public static void Build()
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-buildPath");
            string path = i >= 0 && i + 1 < args.Length ? args[i + 1] : DefaultPath;

            ProjectSetup.ApplyQuality();

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = path,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            });

            long bytes = Directory.Exists(path)
                ? new DirectoryInfo(path).GetFiles("*", SearchOption.AllDirectories).Sum(f => f.Length)
                : 0;
            Debug.Log($"[Setup] web build result={report.summary.result} path={path} size={bytes / 1048576f:0.0}MB time={report.summary.totalTime}");
            if (Application.isBatchMode && report.summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
        }
    }
}
