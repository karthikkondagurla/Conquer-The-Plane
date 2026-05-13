using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEditor.Build.Reporting;

[InitializeOnLoad]
public class AutoBuildScript
{
    static AutoBuildScript()
    {
        if (!SessionState.GetBool("HasBuiltGameOnce", false))
        {
            SessionState.SetBool("HasBuiltGameOnce", true);
            EditorApplication.delayCall += DoBuild;
        }
    }

    [MenuItem("Tools/Build Game (Linux)")]
    public static void DoBuild()
    {
        string[] levels = new string[] 
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Bootstrap.unity",
            "Assets/Scenes/Map1.unity",
            "Assets/Scenes/Map2.unity",
            "Assets/Scenes/Map3.unity",
            "Assets/Scenes/Map4.unity"
        };
        string path = "CTP.x86_64";
        
        Debug.Log("Starting build to " + path + "...");
        BuildReport report = BuildPipeline.BuildPlayer(levels, path, BuildTarget.StandaloneLinux64, BuildOptions.None);
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build Succeeded! Output: " + report.summary.outputPath);
        }
        else
        {
            Debug.LogError("Build Failed: " + report.summary.result);
        }
    }
}
