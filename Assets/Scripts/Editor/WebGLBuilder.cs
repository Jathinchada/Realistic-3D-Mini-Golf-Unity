using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AntiGravityGolf.Editor
{
    public static class WebGLBuilder
    {
        [MenuItem("Anti-Gravity Golf/Build WebGL for GitHub Pages")]
        public static void BuildWebGL()
        {
            Debug.Log("[WebGLBuilder] Starting WebGL Build for GitHub Pages...");

            // Switch to WebGL target
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

            // Configure WebGL Player Settings for GitHub Pages compatibility:
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.threadsSupport = false;

            // Product name and company
            PlayerSettings.productName = "Realistic 3D Mini Golf";
            PlayerSettings.companyName = "Jathinchada";

            string[] scenes = new string[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Gameplay.unity"
            };

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string buildPath = Path.Combine(projectRoot, "Build_WebGL");

            if (Directory.Exists(buildPath))
            {
                try { Directory.Delete(buildPath, true); } catch { }
            }
            Directory.CreateDirectory(buildPath);

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[WebGLBuilder] SUCCESS! WebGL build size: {summary.totalSize} bytes. Output: {buildPath}");
            }
            else
            {
                Debug.LogError($"[WebGLBuilder] FAILED with result: {summary.result}");
                foreach (var step in report.steps)
                {
                    foreach (var msg in step.messages)
                    {
                        if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        {
                            Debug.LogError($"[WebGLBuilder Error] {msg.content}");
                        }
                    }
                }
            }
        }
    }
}
