using UnityEngine;

public class DifficultyConfig : MonoBehaviour
{
    public static DifficultyConfig Instance { get; private set; }

    // === Fixed game balance values ===
    public int EnemyCount { get; private set; } = 16;
    public float EnemySpeed { get; private set; } = 3.0f;
    public float EnemyChaseDistance { get; private set; } = 10f;
    public float PlayerMaxHealth { get; private set; } = 100f;
    public float RegenRate { get; private set; } = 5f;
    public float RegenCooldown { get; private set; } = 2.0f;
    public float DamagePerSecond { get; private set; } = 30f;
    public float SpikeCooldown { get; private set; } = 3f;
    public int MaxSpikes { get; private set; } = 5;
    public float VictoryTime { get; private set; } = 45f;
    public float WormholeRelocateMin { get; private set; } = 3f;
    public float WormholeRelocateMax { get; private set; } = 8f;
    public int WormholesPerMapMin { get; private set; } = 2;
    public int WormholesPerMapMax { get; private set; } = 4;
    public float ShockwaveCooldown { get; private set; } = 5f;
    public float DashCooldown { get; private set; } = 4f;
    public float BoltCooldown { get; private set; } = 2f;

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
}
