using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Wormhole : MonoBehaviour
{
    private Vector3 fullScale;
    private float spawnDuration   = 0.45f; // seconds to grow to full size
    private float despawnDuration = 0.35f; // seconds to shrink away
    private float invisibleGap    = 0.2f;  // seconds hidden before reappearing

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

            // 1. Shrink away
            yield return StartCoroutine(DespawnAnimation());

            // 2. Brief invisible pause, then teleport
            yield return new WaitForSeconds(invisibleGap);
            transform.position = PickNonOverlappingPosition();

            // 3. Grow back at the new location
            yield return StartCoroutine(SpawnAnimation());
        }
    }

    private System.Collections.IEnumerator DespawnAnimation()
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        while (elapsed < despawnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / despawnDuration;
            // Ease-in: slow start, fast finish
            float eased = t * t;
            transform.localScale = startScale * (1f - eased);
            yield return null;
        }
        transform.localScale = Vector3.zero;
    }

    /// Minimum distance between two wormhole centres to avoid visual overlap.
    private const float MIN_WORMHOLE_SEPARATION = 4f;
    private const int MAX_PLACEMENT_ATTEMPTS = 15;

    private Vector3 PickNonOverlappingPosition()
    {
        Wormhole[] others = FindObjectsByType<Wormhole>(FindObjectsSortMode.None);

        Vector3 bestCandidate = new Vector3(Random.Range(-10f, 10f), 1.25f, Random.Range(-10f, 10f));
        float bestMinDist = 0f;

        for (int attempt = 0; attempt < MAX_PLACEMENT_ATTEMPTS; attempt++)
        {
            Vector3 candidate = new Vector3(Random.Range(-10f, 10f), 1.25f, Random.Range(-10f, 10f));

            // Measure the closest other wormhole
            float minDist = float.MaxValue;
            foreach (Wormhole w in others)
            {
                if (w == this) continue;
                float dist = Vector3.Distance(new Vector3(candidate.x, 0, candidate.z),
                                              new Vector3(w.transform.position.x, 0, w.transform.position.z));
                if (dist < minDist) minDist = dist;
            }

            // Accept immediately if far enough, otherwise keep the best so far
            if (minDist >= MIN_WORMHOLE_SEPARATION)
                return candidate;

            if (minDist > bestMinDist)
            {
                bestMinDist = minDist;
                bestCandidate = candidate;
            }
        }

        // Fallback: return the candidate with the most separation we could find
        return bestCandidate;
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
