using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class AutoCharacterSetup
{
    static AutoCharacterSetup()
    {
        EditorApplication.delayCall += CheckAndSetupCharacter;
    }

    [MenuItem("Tools/Setup Robot Character")]
    public static void CheckAndSetupCharacter()
    {
        GameObject player = GameObject.Find("PlayerPersistent");
        if (player == null) player = GameObject.Find("Player"); // Fallback
        
        if (player == null)
        {
            // Don't spam log if player isn't found (might be in a different scene)
            return;
        }

        BallMovement movement = player.GetComponent<BallMovement>();
        if (movement == null)
        {
            Debug.LogWarning("Player found but no BallMovement script attached.");
            return;
        }

        // Check if Robot Sphere is already attached
        Transform existingRobot = player.transform.Find("robotSphere");
        if (existingRobot != null)
        {
            // Already set up
            return;
        }

        Debug.Log("Initializing Robot Sphere Character Setup...");

        // Load Prefab
        GameObject robotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RobotSphere/Assets/Prefab/robotSphere.prefab");
        if (robotPrefab == null)
        {
            Debug.LogError("Could not find Robot Sphere prefab at 'Assets/RobotSphere/Assets/Prefab/robotSphere.prefab'. Please check the path.");
            return;
        }

        // Clean up old visuals (Primitives)
        MeshRenderer oldMesh = player.GetComponent<MeshRenderer>();
        if (oldMesh != null)
        {
            // We can't destroy component easily in edit mode safely without Undo, 
            // so we disable it or remove it if possible.
            // Let's just disable the MeshRenderer
            oldMesh.enabled = false;
        }
        
        MeshFilter oldFilter = player.GetComponent<MeshFilter>();
        // Cannot disable MeshFilter, but if Renderer is off, it's invisible.

        // Instantiate Robot
        GameObject robotInstance = (GameObject)PrefabUtility.InstantiatePrefab(robotPrefab, player.transform);
        robotInstance.name = "robotSphere";
        robotInstance.transform.localPosition = Vector3.zero;
        robotInstance.transform.localRotation = Quaternion.identity;
        
        // Adjust Scale if needed (Robot might be huge or tiny)
        // Usually safe to leave as 1, user can adjust.

        // Register Undo
        Undo.RegisterCreatedObjectUndo(robotInstance, "Setup Robot Character");
        Undo.RecordObject(player.GetComponent<MeshRenderer>(), "Refine Player Visuals");

        // Save
        EditorSceneManager.MarkSceneDirty(player.scene);
        
        Debug.Log("<color=green>Robot Sphere Character Setup Complete!</color>");
    }
}
