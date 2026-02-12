using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Wormhole : MonoBehaviour
{
    private Vector3 fullScale;
    private float spawnDuration = 0.6f; // seconds to grow to full size

    private void Start()
    {
        fullScale = transform.localScale;
        StartCoroutine(SpawnAnimation());
        StartCoroutine(RelocateRoutine());
    }

    private System.Collections.IEnumerator SpawnAnimation()
    {
        transform.localScale = Vector3.zero;
        float elapsed = 0f;
        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spawnDuration;
            // Ease-out: fast start, slow finish
            float eased = 1f - (1f - t) * (1f - t);
            transform.localScale = fullScale * eased;
            yield return null;
        }
        transform.localScale = fullScale;
    }

    private System.Collections.IEnumerator RelocateRoutine()
    {
        while (true)
        {
            float minWait = DifficultyConfig.Instance != null ? DifficultyConfig.Instance.WormholeRelocateMin : 1f;
            float maxWait = DifficultyConfig.Instance != null ? DifficultyConfig.Instance.WormholeRelocateMax : 5f;
            float waitTime = Random.Range(minWait, maxWait);
            yield return new WaitForSeconds(waitTime);

            // Relocate to a random position on the floor
            Vector3 newPos = new Vector3(Random.Range(-10f, 10f), 1.25f, Random.Range(-10f, 10f));
            transform.position = newPos;

            // Play spawn animation again
            yield return StartCoroutine(SpawnAnimation());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (LoadingUI.Instance != null)
            {
                int targetIndex = GetRandomMapIndex();
                if (targetIndex != -1)
                {
                    LoadingUI.Instance.TeleportToScene(targetIndex);
                }
            }
            else
            {
                Debug.LogError("LoadingUI Instance not found! Ensure the Bootstrap scene was loaded first.");
            }
        }
        else if (other.CompareTag("Enemy") || other.name.Contains("Enemy"))
        {
            // Persistent Enemy Teleportation
            EnemyAI ai = other.GetComponent<EnemyAI>();
            if (ai != null && EnemyManager.Instance != null)
            {
                EnemyManager.Instance.TeleportEnemy(ai);
            }
        }
    }

    private int GetRandomMapIndex()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        List<int> availableIndices = new List<int>();

        // Assuming maps are at indices 1-4. 
        // If scene count is less, adjust logic.
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            int sceneNameStart = path.LastIndexOf('/') + 1;
            int sceneNameEnd = path.LastIndexOf('.');
            string sceneName = path.Substring(sceneNameStart, sceneNameEnd - sceneNameStart);

            // Only allow scenes that start with "Map" (Map1, Map2, etc.)
            if (sceneName.StartsWith("Map") && i != currentSceneIndex)
            {
                availableIndices.Add(i);
            }
        }

        if (availableIndices.Count > 0)
        {
            return availableIndices[Random.Range(0, availableIndices.Count)];
        }
        
        Debug.LogWarning("No other maps found to teleport to!");
        return -1;
    }
}
