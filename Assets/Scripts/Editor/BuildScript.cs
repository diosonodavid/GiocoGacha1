using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GachaGame.Editor
{
    // Batchmode-friendly build entry points (e.g. `-executeMethod GachaGame.Editor.BuildScript.BuildPC`),
    // plus editor menu items for building PC and Mobile client artifacts from inside the editor.
    public static class BuildScript
    {
        private const string OutputRoot = "Builds";

        [MenuItem("GachaGame/Build/PC (Windows x64)")]
        public static void BuildPC() => BuildPlayer(BuildTarget.StandaloneWindows64, $"{OutputRoot}/PC/GachaGame.exe");

        [MenuItem("GachaGame/Build/Android")]
        public static void BuildAndroid() => BuildPlayer(BuildTarget.Android, $"{OutputRoot}/Android/GachaGame.apk");

        [MenuItem("GachaGame/Build/iOS")]
        public static void BuildIOS() => BuildPlayer(BuildTarget.iOS, $"{OutputRoot}/iOS");

        [MenuItem("GachaGame/Build/All Platforms")]
        public static void BuildAllPlatforms()
        {
            BuildPC();
            BuildAndroid();
        }

        private static void BuildPlayer(BuildTarget target, string outputPath)
        {
            var scenes = Array.ConvertAll(EditorBuildSettings.scenes, s => s.path);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = target,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
                Debug.Log($"Build succeeded for {target}: {summary.totalSize} bytes at {outputPath}");
            else
                Debug.LogError($"Build failed for {target} with result {summary.result}.");
        }
    }
}
