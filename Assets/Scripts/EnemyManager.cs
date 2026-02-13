using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    private List<EnemyAI> persistentEnemies = new List<EnemyAI>();
    
    private int TotalEnemies => DifficultyConfig.Instance != null 
        ? DifficultyConfig.Instance.EnemyCount 
        : 9;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Clean up any stale enemies from previous sessions
            GameObject[] staleEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var stale in staleEnemies)
            {
                Destroy(stale);
            }
            
            SpawnPersistentEnemies();
            
            // Immediately update visibility for current scene (Bootstrap)
            UpdateEnemyVisibility(SceneManager.GetActiveScene());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void SpawnPersistentEnemies()
    {
        // Try to load the Zombie prefab from Resources
        GameObject zombiePrefab = Resources.Load<GameObject>("ZombieEnemy");
        if (zombiePrefab == null)
        {
            Debug.LogWarning("ZombieEnemy prefab not found in Resources. Falling back to primitive cube.");
        }

        // Create 4 enemies
        for (int i = 0; i < TotalEnemies; i++)
        {
            GameObject enemyGO;
            
            if (zombiePrefab != null)
            {
                enemyGO = Instantiate(zombiePrefab);
            }
            else
            {
                // Fallback: Create Primitive
                enemyGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                
                // Visuals - URP compatible (Fallback)
                Renderer r = enemyGO.GetComponent<Renderer>();
                Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
                if (litShader == null) litShader = Shader.Find("Standard");
                Material m = new Material(litShader);
                m.SetColor("_BaseColor", Color.black);
                m.color = Color.black;
                r.sharedMaterial = m;
                
                 // Physics (Fallback)
                Rigidbody rb = enemyGO.AddComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.isKinematic = false; 
                rb.useGravity = true;
                
                BoxCollider enemyCollider = enemyGO.GetComponent<BoxCollider>();
                if (enemyCollider != null) enemyCollider.isTrigger = false;
                
                enemyGO.AddComponent<EnemyAI>();
            }

            enemyGO.name = "PersistentEnemy_" + i;
            enemyGO.tag = "Enemy";
            DontDestroyOnLoad(enemyGO);

            // AI Setup
            EnemyAI ai = enemyGO.GetComponent<EnemyAI>();
            if (ai == null) ai = enemyGO.AddComponent<EnemyAI>();
            
            // Assign random map (1-4)
            ai.currentMapID = Random.Range(1, 5); 
            
            // Random position in that "virtual" map space
            // Random position in that "virtual" map space
            enemyGO.transform.position = new Vector3(Random.Range(-10f, 10f), 0.0f, Random.Range(-10f, 10f));

            persistentEnemies.Add(ai);
            
            // Initially hide them
            enemyGO.SetActive(false);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateEnemyVisibility(scene);
    }

    private void UpdateEnemyVisibility(Scene scene)
    {
        // Determine current map ID
        int currentMapID = 0;
        if (scene.name.StartsWith("Map"))
        {
             int.TryParse(scene.name.Substring(3), out currentMapID);
        }

        // Show/Hide enemies based on map
        foreach (var enemy in persistentEnemies)
        {
            if (enemy == null) continue;

            if (enemy.currentMapID == currentMapID && currentMapID != 0)
            {
                enemy.gameObject.SetActive(true);
            }
            else
            {
                enemy.gameObject.SetActive(false);
            }
        }
    }

    // Called when an enemy enters a wormhole
    public void TeleportEnemy(EnemyAI enemy)
    {
        // Pick a new random map (1-4)
        // Ensure it's different from current? Or just random? 
        // "Teleport one map to other" implies change.
        int newMapID = Random.Range(1, 5);
        
        // Optional: Ensure it's different
        // while (newMapID == enemy.currentMapID) newMapID = Random.Range(1, 5);

        enemy.currentMapID = newMapID;

        // Determine if we should show or hide it immediately
        int playerMapID = 0;
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.StartsWith("Map")) int.TryParse(sceneName.Substring(3), out playerMapID);

        if (enemy.currentMapID != playerMapID)
        {
            enemy.gameObject.SetActive(false);
        }
        else
        {
            // It teleported TO the player's current map (rare but possible if we allow it)
            // Relocate to random position
            enemy.transform.position = new Vector3(Random.Range(-10f, 10f), 0.5f, Random.Range(-10f, 10f));
            enemy.gameObject.SetActive(true);
            enemy.PickNewRoamTarget();
        }
    }

    // Get the count of enemies currently in a specific map
    public int GetEnemyCount(int mapID)
    {
        int count = 0;
        foreach (var enemy in persistentEnemies)
        {
            if (enemy != null && enemy.currentMapID == mapID)
            {
                count++;
            }
        }
        return count;
    }
}
