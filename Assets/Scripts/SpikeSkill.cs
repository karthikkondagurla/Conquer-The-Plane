using UnityEngine;

public class SpikeSkill : MonoBehaviour
{
    [Header("Spike Settings")]
    public float cooldown = 3f;        // Seconds between uses
    public int maxSpikes = 5;          // Max active regular spikes at once
    public Color spikeColor = new Color(0.3f, 1f, 0.7f); // Green-blue crystal glow

    private float lastUseTime = -99f;
    private int activeSpikes = 0;

    void Start()
    {
        if (DifficultyConfig.Instance != null)
        {
            cooldown = DifficultyConfig.Instance.SpikeCooldown;
            maxSpikes = DifficultyConfig.Instance.MaxSpikes;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            TryPlaceSpike();
        }
    }

    void TryPlaceSpike()
    {
        // Cooldown check
        if (Time.time - lastUseTime < cooldown)
        {
            float remaining = cooldown - (Time.time - lastUseTime);
            Debug.Log($"Spike on cooldown! {remaining:F1}s remaining");
            return;
        }

        int currentMapID = GameWinManager.GetCurrentMapID();
        bool isOnDemandingPlane = GameWinManager.Instance != null && 
                                  currentMapID == GameWinManager.Instance.DemandingMapID &&
                                  currentMapID != 0;

        // If on demanding plane, this is a victory spike
        if (isOnDemandingPlane)
        {
            PlaceVictorySpike(currentMapID);
        }
        else
        {
            PlaceRegularSpike();
        }
    }

    private void PlaceVictorySpike(int mapID)
    {
        // Only one victory spike at a time
        if (GameWinManager.Instance.IsVictorySpikeActive)
        {
            Debug.Log("Victory spike already active! Defend it!");
            return;
        }

        Vector3 spawnPos = transform.position;
        spawnPos.y = 0f;

        // Victory spike glows brighter gold
        Color victoryColor = new Color(1f, 0.85f, 0.2f); // Gold
        GameObject spike = SpikeTrap.CreateSpikeAsset(victoryColor);
        spike.transform.position = spawnPos;
        spike.transform.localScale = Vector3.one * 1.5f; // Bigger than regular spikes
        spike.name = "VictorySpike";

        SpikeTrap trap = spike.AddComponent<SpikeTrap>();
        trap.isVictorySpike = true;
        trap.plantedMapID = mapID;
        trap.lifetime = 0f; // No auto-destroy

        // Notify the win manager
        GameWinManager.Instance.ActivateVictorySpike(trap, mapID);

        lastUseTime = Time.time;
        Debug.Log($"⚡ VICTORY SPIKE planted on Map {mapID}! Defend for 60 seconds!");
    }

    private void PlaceRegularSpike()
    {
        // Max spikes check
        if (activeSpikes >= maxSpikes)
        {
            Debug.Log("Max spikes reached! Wait for one to expire.");
            return;
        }

        Vector3 spawnPos = transform.position;
        spawnPos.y = 0f;

        GameObject spike = SpikeTrap.CreateSpikeAsset(spikeColor);
        spike.transform.position = spawnPos;
        spike.transform.localScale = Vector3.one;

        SpikeTrap trap = spike.AddComponent<SpikeTrap>();

        activeSpikes++;
        lastUseTime = Time.time;

        StartCoroutine(TrackSpikeLifetime(spike));

        Debug.Log($"Crystal spike planted! ({activeSpikes}/{maxSpikes} active)");
    }

    private System.Collections.IEnumerator TrackSpikeLifetime(GameObject spike)
    {
        while (spike != null)
        {
            yield return new WaitForSeconds(0.5f);
        }
        activeSpikes--;
    }
}
