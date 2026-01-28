using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class RankingUI : MonoBehaviour
{
    [System.Serializable]
    public class RankingRow
    {
        public int mapID; // 1-4
        public RectTransform rowTransform;
        public Text countText;
        public Text mapNameText; // Optional if static
    }

    public List<RankingRow> rows;

    private static RankingUI instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnEnemyCountChanged += UpdateRanking;
            UpdateRanking(); // Initial sort
        }
    }

    void OnDestroy()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnEnemyCountChanged -= UpdateRanking;
        }
    }

    void UpdateRanking()
    {
        // 1. Get current counts
        var counts = EnemyManager.Instance.enemyCounts;

        // 2. Sort MapIDs by count descending
        // We only care about map IDs present in our 'rows' list
        var sortedRows = rows.OrderByDescending(r => counts.ContainsKey(r.mapID) ? counts[r.mapID] : 0).ToList();

        // 3. Reorder Logic
        for (int i = 0; i < sortedRows.Count; i++)
        {
            RankingRow row = sortedRows[i];
            
            // Update Sibling Index to reorder visually (Vertical Layout Group handles position)
            row.rowTransform.SetSiblingIndex(i);

            // Update Text
            if (counts.ContainsKey(row.mapID))
            {
                row.countText.text = counts[row.mapID].ToString();
            }
        }
    }
}
