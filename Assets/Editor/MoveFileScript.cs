using UnityEditor;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public class MoveFileScript
{
    static MoveFileScript()
    {
        string src = "Assets/freesound_community-zombie-bite-96528.mp3";
        string dest = "Assets/Resources/freesound_community-zombie-bite-96528.mp3";
        if (File.Exists(src))
        {
            if (!Directory.Exists("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            string err = AssetDatabase.MoveAsset(src, dest);
            if (string.IsNullOrEmpty(err)) {
                Debug.Log("Moved MP3 to Resources folder successfully.");
            } else {
                Debug.LogError("Error moving MP3: " + err);
            }
        }
    }
}
