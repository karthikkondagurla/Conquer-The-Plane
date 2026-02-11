using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class SceneSetupTools : EditorWindow
{
    [MenuItem("Tools/Setup Wormhole Test")]
    public static void SetupWormholeTest()
    {
        EnsureTagExists("SpawnPoint");
        EnsureTagExists("Enemy");

        string sceneDir = "Assets/Scenes";
        if (!Directory.Exists(sceneDir))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        // 1. Create Scenes
        string[] sceneNames = { "Bootstrap", "Map1", "Map2", "Map3", "Map4" }; // "MainMenu" removed
        EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[sceneNames.Length];

        // Pre-calculate random enemy distribution (Total 4)
        int[] enemyDist = new int[4];
        for (int k = 0; k < 4; k++)
        {
            enemyDist[Random.Range(0, 4)]++;
        }

        for (int i = 0; i < sceneNames.Length; i++)
        {
            string scenePath = $"{sceneDir}/{sceneNames[i]}.unity";
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Add Basic Lighting
            Light light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);

            // Add Camera (Only for Maps, Bootstrap has special setup)
            if (sceneNames[i].StartsWith("Map"))
            {
                // Maps need spawn points and wormholes
                // sceneNames: 0=MainMenu, 1=Bootstrap, 2=Map1
                // We need enemyDist[0] for Map1. So index is i - 2.
                SetupMapEnvironment(sceneNames[i], enemyDist[i - 1]);
            }
            else if (sceneNames[i] == "MainMenu")
            {
                // CreateMainMenu();
                Debug.Log("MainMenu scene creation skipped (deleted by user request).");
            }
            else if (sceneNames[i] == "Bootstrap")
            {
                // Bootstrap needs Player
                SetupBootstrapEnvironment();
            }

            EditorSceneManager.SaveScene(scene, scenePath);
            buildScenes[i] = new EditorBuildSettingsScene(scenePath, true);
        }

        // 2. Set Build Settings
        EditorBuildSettings.scenes = buildScenes;
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Complete", "Initialized Bootstrap and 4 Map scenes with Build Settings.", "OK");
    }

    private static void SetupBootstrapEnvironment()
    {
        // 0. Create EnemyManager (Singleton)
        new GameObject("EnemyManager").AddComponent<EnemyManager>();

        // 1. Create Loading UI
        // Trigger LoadingUI Awake via creating GO
        GameObject uiGO = new GameObject("LoadingUI");
        uiGO.AddComponent<LoadingUI>();

        // 2. Create Ranking UI
        // CreateRankingUI(); // Removed by user request

        // 3. Create Health UI (Top Right + Game Over)
        // CreateHealthUI(); // Removed by user request

        // 4. Create Player
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        
        // Add Physics Material for rolling (friction)
        PhysicsMaterial ballMat = new PhysicsMaterial();
        ballMat.dynamicFriction = 0.6f;
        ballMat.staticFriction = 0.6f;
        ballMat.bounciness = 0.6f;
        ballMat.frictionCombine = PhysicsMaterialCombine.Average;
        ballMat.bounceCombine = PhysicsMaterialCombine.Average;
        player.GetComponent<Collider>().material = ballMat;
        player.name = "PlayerManager";
        player.tag = "Player";
        player.AddComponent<Rigidbody>().isKinematic = false; 
        player.AddComponent<PlayerPersistent>();
        player.AddComponent<PlayerHealth>(); // Add Health Component
        
        // Add BallMovement
        player.AddComponent<BallMovement>();
        
        GameObject cam = new GameObject("Main Camera");
        cam.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.AddComponent<CameraFollow>().target = player.transform;
        cam.AddComponent<PersistentCamera>();
        
        // Position Camera behind player
        cam.transform.position = player.transform.position + new Vector3(0, 3, -5);
        cam.transform.LookAt(player.transform);

        // Ground
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.transform.position = Vector3.zero;
        player.transform.position = Vector3.up * 0.5f; // Touching ground (Radius 0.5)
        
        // Add a "Start Wormhole" to verify
        CreateWormhole(new Vector3(3, 0, 3));
    }

    private static void SetupMapEnvironment(string mapName, int enemyCount)
    {
        // Ground - Scale up to 25x25 (Scale 2.5) to give more room
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Ground_" + mapName;
        floor.transform.localScale = new Vector3(2.5f, 1, 2.5f);
        
        // Color it differently
        Renderer ren = floor.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.grey;
        ren.sharedMaterial = mat;

        // SpawnPoints (3-5 random locations)
        int spawnCount = Random.Range(3, 6);
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject spawn = new GameObject("SpawnPoint_" + i);
            spawn.tag = "SpawnPoint";
            // Random position on the larger floor (-10 to 10 safe range). Y=0.5 to touch ground.
            spawn.transform.position = new Vector3(Random.Range(-10f, 10f), 0.5f, Random.Range(-10f, 10f));
        }

        // Enemies (Fixed Count passed from Manager)
        int mapID = 0;
        if (mapName.StartsWith("Map")) int.TryParse(mapName.Substring(3), out mapID);

        for (int j = 0; j < enemyCount; j++)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemy.name = "Enemy_" + j;
            enemy.tag = "Enemy"; // Ensure Tag is set
            // Random position
            enemy.transform.position = new Vector3(Random.Range(-10f, 10f), 0.5f, Random.Range(-10f, 10f));
            
            // Visuals
            Renderer r = enemy.GetComponent<Renderer>();
            Material m = new Material(Shader.Find("Standard"));
            m.color = Color.black;
            r.sharedMaterial = m;

            // Logic
            EnemyAI ai = enemy.AddComponent<EnemyAI>();
            ai.originMapID = mapID;
            
            // Physics
            Rigidbody rb = enemy.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation; // Slide, don't roll
        }

        // Wormholes (3-5)
        int count = Random.Range(3, 6);
        for (int k = 0; k < count; k++)
        {
            // Random position on the larger floor (-10 to 10 safe range)
            Vector3 pos = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
            CreateWormhole(pos);
        }
    }

    private static void CreateWormhole(Vector3 position)
    {
        GameObject wh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        wh.name = "Wormhole";
        // Position at y=0.5 (radius) so it sits ON the floor, not in the air
        wh.transform.position = new Vector3(position.x, 0.5f, position.z);
        wh.GetComponent<Collider>().isTrigger = true;
        wh.AddComponent<Wormhole>();
        
        // Visuals: make it look like a portal
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.magenta;
        wh.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static void EnsureTagExists(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(tag)) { found = true; break; }
        }

        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
            n.stringValue = tag;
            tagManager.ApplyModifiedProperties();
        }
    }

    // private static void CreateRankingUI() { ... } // Removed by user request

    // private static void CreateMainMenu() { ... } // Removed by user request

    // private static void CreateHealthUI() { ... } // Removed by user request
}
