using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;


namespace Editor
{
    public class ConsoleBuildScript : MonoBehaviour
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
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/MainScene.unity" } // Здесь укажите название вашей основной сцены в проекте - Если сцен много, укажите их как на примере ниже
                                                                   //scenes = new[] { "Assets/Scenes/MainScene.unity", "Assets/Scenes/SecondaryScene.unit”} //Указание происходит в массиве, через запятую
            };

            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            buildPlayerOptions.locationPathName = $"{path}/workspace/test-unity-build2";
            buildPlayerOptions.target = buildTarget;
            EditorUserBuildSettings.overrideMaxTextureSize = 512;
            buildPlayerOptions.options = BuildOptions.None;

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }
        }
    }
}