using UnityEditor;
using UnityEditor.Build.Reporting;
using System.Linq; // Для Any и Where
using UnityEngine; // Для LogType и Debug

namespace Editor
{
    public class ConsoleBuildScript
    {
        [MenuItem("Build/BuildIOS")]
        public static void MyBuildIOS()
        {
            MyBuild(BuildTarget.iOS);
        }

        [MenuItem("Build/BuildAndroid")]
        public static void MyBuildAndroid()
        {
            MyBuild(BuildTarget.Android);
        }
        
        private static void MyBuild(BuildTarget buildTarget)
        {
            // Проверка существования сцен
            string[] scenes = new[]
            {
                "Assets/Scenes/MainScene.unity"
            };
            foreach (var scene in scenes)
            {
                if (!System.IO.File.Exists(scene))
                {
                    UnityEngine.Debug.LogError($"Scene not found: {scene}");
                    return;
                }
            }

            // Путь для вывода билда
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            string buildPath = System.IO.Path.Combine(path, "workspace", "test-unity-build2",
                buildTarget == BuildTarget.iOS ? "iOSBuild" : "AndroidBuild");
            if (!System.IO.Directory.Exists(buildPath))
            {
                System.IO.Directory.CreateDirectory(buildPath);
            }

            // Настройка сборки
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = buildTarget,
                options = BuildOptions.None
            };

            EditorUserBuildSettings.overrideMaxTextureSize = 512;

            // Выполнение сборки
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                UnityEngine.Debug.Log($"Build succeeded: {summary.totalSize} bytes");
            }
            else
            {
                UnityEngine.Debug.LogError($"Build failed: {summary.totalErrors} errors");
                foreach (var step in report.steps)
                {
                    if (step.messages.Any(m => m.type == LogType.Error)) // Используем Any с LINQ
                    {
                        UnityEngine.Debug.LogError($"Step {step.name} failed: {string.Join(", ", step.messages.Where(m => m.type == LogType.Error).Select(m => m.content))}"); // Используем Where с LINQ
                    }
                }
            }
        }
    }
}