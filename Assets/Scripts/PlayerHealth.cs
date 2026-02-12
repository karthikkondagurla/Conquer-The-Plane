using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    
    // Regen
    public float regenRate = 2f; // HP per second (slow heal)
    public float damageCooldown = 3.0f; // Seconds before regen starts
    private float lastDamageTime;

    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    private bool isDead = false;

    void Start()
    {
        // Apply difficulty settings
        if (DifficultyConfig.Instance != null)
        {
            maxHealth = DifficultyConfig.Instance.PlayerMaxHealth;
            regenRate = DifficultyConfig.Instance.RegenRate;
            damageCooldown = DifficultyConfig.Instance.RegenCooldown;
        }

        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    void Update()
    {
        if (isDead) return;

        // Regen Logic
        if (Time.time - lastDamageTime > damageCooldown && currentHealth < maxHealth)
        {
            currentHealth += regenRate * Time.deltaTime;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            OnHealthChanged?.Invoke(currentHealth / maxHealth);
        }
    }

    // Called when colliding with Enemy
    void OnCollisionStay(Collision collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            float dps = DifficultyConfig.Instance != null ? DifficultyConfig.Instance.DamagePerSecond : 30f;
            Debug.Log($"Player colliding with Enemy: {collision.gameObject.name}"); 
            TakeDamage(dps * Time.deltaTime); // Fast continuous damage
        }
    }

    // Also support Trigger if enemies are triggers
    void OnTriggerStay(Collider other)
    {
        if (isDead) return;

        if (other.CompareTag("Enemy"))
        {
            float dps = DifficultyConfig.Instance != null ? DifficultyConfig.Instance.DamagePerSecond : 30f;
            TakeDamage(dps * Time.deltaTime);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        lastDamageTime = Time.time;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player Died!");
        OnDeath?.Invoke();
    }
}
