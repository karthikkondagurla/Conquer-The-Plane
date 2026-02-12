using UnityEngine;

public class PlayerPersistent : MonoBehaviour
{
    public static PlayerPersistent Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSkills();
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Ensure frame rate cap as requested
        Application.targetFrameRate = 30;
    }

    /// <summary>
    /// Ensures all player skills are attached at runtime.
    /// This guarantees skills work even on scenes that weren't regenerated.
    /// </summary>
    private void EnsureSkills()
    {
        if (GetComponent<PlayerHealth>() == null) gameObject.AddComponent<PlayerHealth>();
        if (GetComponent<SpikeSkill>() == null) gameObject.AddComponent<SpikeSkill>();
        if (GetComponent<ShockwaveSkill>() == null) gameObject.AddComponent<ShockwaveSkill>();
        if (GetComponent<DashStrikeSkill>() == null) gameObject.AddComponent<DashStrikeSkill>();
        if (GetComponent<EnergyBoltSkill>() == null) gameObject.AddComponent<EnergyBoltSkill>();
        if (GetComponent<BallMovement>() == null) gameObject.AddComponent<BallMovement>();
    }
}
