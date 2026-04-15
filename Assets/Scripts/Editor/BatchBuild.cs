using UnityEditor;
using UnityEngine;

namespace JamTemplate.Editor
{
    public static class BatchBuild
    {
        public static void BuildWebGL()
        {
            string[] scenes = GetBuildScenes();
            var report = BuildPipeline.BuildPlayer(scenes, "Build/WebGL", BuildTarget.WebGL, BuildOptions.None);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.LogError($"WebGL build failed: {report.summary.result}");
                EditorApplication.Exit(1);
            }
        }

        public static void BuildWindows()
        {
            string[] scenes = GetBuildScenes();
            var report = BuildPipeline.BuildPlayer(scenes, "Build/Windows/JamTemplate.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.LogError($"Windows build failed: {report.summary.result}");
                EditorApplication.Exit(1);
            }
        }

        static string[] GetBuildScenes()
        {
            var scenes = EditorBuildSettings.scenes;
            var paths = new string[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
                paths[i] = scenes[i].path;
            return paths;
        }
    }
}
