using UnityEngine;
using System.Collections.Generic;
using System;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    // MapID -> EnemyCount
    public Dictionary<int, int> enemyCounts = new Dictionary<int, int>();

    public event Action OnEnemyCountChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize counts for Maps 1-4
            for (int i = 1; i <= 4; i++)
            {
                enemyCounts[i] = 0;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterEnemy(int mapID)
    {
        if (!enemyCounts.ContainsKey(mapID)) enemyCounts[mapID] = 0;
        enemyCounts[mapID]++;
        OnEnemyCountChanged?.Invoke();
    }

    public void UnregisterEnemy(int mapID)
    {
        if (enemyCounts.ContainsKey(mapID))
        {
            enemyCounts[mapID]--;
            if (enemyCounts[mapID] < 0) enemyCounts[mapID] = 0;
            OnEnemyCountChanged?.Invoke();
        }
    }
}
