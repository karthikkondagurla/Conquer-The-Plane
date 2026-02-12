using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameWinManager : MonoBehaviour
{
    public static GameWinManager Instance { get; private set; }

    // Demanding plane state
    public int DemandingMapID { get; private set; } = 0;
    private int previousDemandingMapID = 0;

    // Victory spike state
    public bool IsVictorySpikeActive { get; private set; } = false;
    public float CountdownRemaining { get; private set; } = 0f;
    public int SpikeMapID { get; private set; } = 0;

    private SpikeTrap activeVictorySpike = null;
    private const float VICTORY_TIME = 60f;

    // Events
    public event Action<int> OnDemandingPlaneChanged;  // new mapID
    public event Action OnSpikeActivated;
    public event Action<string> OnSpikeDeactivated;     // reason string
    public event Action OnVictory;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        UpdateDemandingPlane();
        UpdateVictoryCountdown();
    }

    // ===================== DEMANDING PLANE =====================
    private void UpdateDemandingPlane()
    {
        if (EnemyManager.Instance == null) return;

        int maxCount = 0;
        int maxMapID = 1;

        for (int i = 1; i <= 4; i++)
        {
            int count = EnemyManager.Instance.GetEnemyCount(i);
            if (count > maxCount)
            {
                maxCount = count;
                maxMapID = i;
            }
        }

        DemandingMapID = maxMapID;

        if (DemandingMapID != previousDemandingMapID)
        {
            // Demanding plane changed
            OnDemandingPlaneChanged?.Invoke(DemandingMapID);

            // Deactivate spike if it was on the old demanding plane
            if (IsVictorySpikeActive && SpikeMapID != DemandingMapID)
            {
                DeactivateSpike("Demanding plane changed!");
            }

            previousDemandingMapID = DemandingMapID;
        }
    }

    // ===================== VICTORY SPIKE =====================
    public void ActivateVictorySpike(SpikeTrap spike, int mapID)
    {
        // Deactivate previous if any
        if (IsVictorySpikeActive)
        {
            DeactivateSpike("New spike planted");
        }

        activeVictorySpike = spike;
        SpikeMapID = mapID;
        IsVictorySpikeActive = true;
        CountdownRemaining = VICTORY_TIME;

        Debug.Log($"⚡ VICTORY SPIKE activated on Map {mapID}! Defend for {VICTORY_TIME}s!");
        OnSpikeActivated?.Invoke();
    }

    public void DeactivateSpike(string reason)
    {
        if (!IsVictorySpikeActive) return;

        IsVictorySpikeActive = false;
        CountdownRemaining = 0f;

        // Destroy the spike object
        if (activeVictorySpike != null)
        {
            Destroy(activeVictorySpike.gameObject);
            activeVictorySpike = null;
        }

        SpikeMapID = 0;

        Debug.Log($"❌ Victory spike deactivated: {reason}");
        OnSpikeDeactivated?.Invoke(reason);
    }

    private void UpdateVictoryCountdown()
    {
        if (!IsVictorySpikeActive) return;

        // Check if spike was destroyed externally
        if (activeVictorySpike == null)
        {
            DeactivateSpike("Spike was destroyed");
            return;
        }

        CountdownRemaining -= Time.deltaTime;

        if (CountdownRemaining <= 0f)
        {
            CountdownRemaining = 0f;
            IsVictorySpikeActive = false;
            TriggerVictory();
        }
    }

    private void TriggerVictory()
    {
        Debug.Log("🏆 VICTORY! You conquered the demanding plane!");
        OnVictory?.Invoke();
    }

    // Called by SpikeTrap when an enemy touches the victory spike
    public void OnEnemyHitVictorySpike()
    {
        DeactivateSpike("Enemy touched the spike!");
    }

    // Get current map ID from scene name
    public static int GetCurrentMapID()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.StartsWith("Map"))
        {
            int mapID;
            if (int.TryParse(sceneName.Substring(3), out mapID))
                return mapID;
        }
        return 0;
    }
}
