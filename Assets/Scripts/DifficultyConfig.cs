using UnityEngine;

public class DifficultyConfig : MonoBehaviour
{
    public static DifficultyConfig Instance { get; private set; }

    public enum Difficulty { Easy, Normal, Hard, Nightmare }

    public Difficulty CurrentDifficulty { get; private set; } = Difficulty.Normal;

    // === Cached values for current difficulty ===
    public int EnemyCount { get; private set; }
    public float EnemySpeed { get; private set; }
    public float EnemyChaseDistance { get; private set; }
    public float PlayerMaxHealth { get; private set; }
    public float RegenRate { get; private set; }
    public float RegenCooldown { get; private set; }
    public float DamagePerSecond { get; private set; }
    public float SpikeCooldown { get; private set; }
    public int MaxSpikes { get; private set; }
    public float VictoryTime { get; private set; }
    public float WormholeRelocateMin { get; private set; }
    public float WormholeRelocateMax { get; private set; }
    public int WormholesPerMapMin { get; private set; }
    public int WormholesPerMapMax { get; private set; }
    public float ShockwaveCooldown { get; private set; }
    public float DashCooldown { get; private set; }
    public float BoltCooldown { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ApplyDifficulty(CurrentDifficulty);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetDifficulty(Difficulty difficulty)
    {
        CurrentDifficulty = difficulty;
        ApplyDifficulty(difficulty);
        Debug.Log($"Difficulty set to: {difficulty}");
    }

    private void ApplyDifficulty(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                EnemyCount = 8;
                EnemySpeed = 0.8f;
                EnemyChaseDistance = 7f;
                PlayerMaxHealth = 150f;
                RegenRate = 8f;
                RegenCooldown = 1.5f;
                DamagePerSecond = 20f;
                SpikeCooldown = 2f;
                MaxSpikes = 7;
                VictoryTime = 30f;
                WormholeRelocateMin = 3f;
                WormholeRelocateMax = 8f;
                WormholesPerMapMin = 4;
                WormholesPerMapMax = 6;
                ShockwaveCooldown = 4f;
                DashCooldown = 3f;
                BoltCooldown = 1.5f;
                break;

            case Difficulty.Normal:
                EnemyCount = 16;
                EnemySpeed = 3.0f;
                EnemyChaseDistance = 10f;
                PlayerMaxHealth = 100f;
                RegenRate = 5f;
                RegenCooldown = 2.0f;
                DamagePerSecond = 30f;
                SpikeCooldown = 3f;
                MaxSpikes = 5;
                VictoryTime = 45f;
                WormholeRelocateMin = 5f;
                WormholeRelocateMax = 15f;
                WormholesPerMapMin = 2;
                WormholesPerMapMax = 4;
                ShockwaveCooldown = 5f;
                DashCooldown = 4f;
                BoltCooldown = 2f;
                break;

            case Difficulty.Hard:
                EnemyCount = 24;
                EnemySpeed = 4.0f;
                EnemyChaseDistance = 14f;
                PlayerMaxHealth = 75f;
                RegenRate = 3f;
                RegenCooldown = 3.0f;
                DamagePerSecond = 40f;
                SpikeCooldown = 5f;
                MaxSpikes = 3;
                VictoryTime = 60f;
                WormholeRelocateMin = 10f;
                WormholeRelocateMax = 20f;
                WormholesPerMapMin = 1;
                WormholesPerMapMax = 3;
                ShockwaveCooldown = 7f;
                DashCooldown = 5f;
                BoltCooldown = 3f;
                break;

            case Difficulty.Nightmare:
                EnemyCount = 32;
                EnemySpeed = 5.0f;
                EnemyChaseDistance = 18f;
                PlayerMaxHealth = 50f;
                RegenRate = 0f;  // No regen!
                RegenCooldown = 999f;
                DamagePerSecond = 50f;
                SpikeCooldown = 8f;
                MaxSpikes = 2;
                VictoryTime = 90f;
                WormholeRelocateMin = 20f;
                WormholeRelocateMax = 40f;
                WormholesPerMapMin = 1;
                WormholesPerMapMax = 2;
                ShockwaveCooldown = 10f;
                DashCooldown = 7f;
                BoltCooldown = 4f;
                break;
        }
    }

    /// <summary>
    /// Returns a display name and color for the current difficulty.
    /// </summary>
    public static string GetDifficultyName(Difficulty d)
    {
        switch (d)
        {
            case Difficulty.Easy: return "EASY";
            case Difficulty.Normal: return "NORMAL";
            case Difficulty.Hard: return "HARD";
            case Difficulty.Nightmare: return "NIGHTMARE";
            default: return "NORMAL";
        }
    }

    public static Color GetDifficultyColor(Difficulty d)
    {
        switch (d)
        {
            case Difficulty.Easy: return new Color(0.2f, 1f, 0.4f);       // Green
            case Difficulty.Normal: return new Color(0f, 0.9f, 1f);        // Cyan
            case Difficulty.Hard: return new Color(1f, 0.5f, 0f);          // Orange
            case Difficulty.Nightmare: return new Color(1f, 0.15f, 0.15f); // Red
            default: return Color.white;
        }
    }
}
