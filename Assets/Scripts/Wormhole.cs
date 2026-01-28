using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Wormhole : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(RelocateRoutine());
    }

    private System.Collections.IEnumerator RelocateRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(1f, 5f);
            yield return new WaitForSeconds(waitTime);

            // Relocate to a random position on the floor (Assumed 25x25 area from SetupTools)
            // Valid range approx -10 to 10 to stay on the plane safely
            Vector3 newPos = new Vector3(Random.Range(-10f, 10f), 0.5f, Random.Range(-10f, 10f));
            transform.position = newPos;
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
    }

    private int GetRandomMapIndex()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        List<int> availableIndices = new List<int>();

        // Assuming maps are at indices 1-4. 
        // If scene count is less, adjust logic.
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        for (int i = 1; i < sceneCount; i++) // Start at 1 to skip Bootstrap if needed, or include it? 
        // User asked for 4 maps. Bootstrap is 0. Maps are 1,2,3,4.
        // We only want to jump to Maps (1-4).
        {
            // Don't jump to current scene (unless there's only 1 map?)
            if (i != currentSceneIndex)
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
