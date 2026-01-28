using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthUI : MonoBehaviour
{
    public Image healthBarFill;
    public GameObject gameOverPanel;
    public Button mainMenuButton;

    private PlayerHealth playerHealth;

    void Start()
    {
        TryFindPlayer();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    void Update()
    {
        if (playerHealth == null)
        {
            TryFindPlayer();
        }
    }

    void TryFindPlayer()
    {
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                Debug.Log("HealthUI: Found PlayerHealth!");
                playerHealth.OnHealthChanged += UpdateHealthBar;
                playerHealth.OnDeath += ShowGameOver;
                // Sync initial value
                UpdateHealthBar(playerHealth.currentHealth / playerHealth.maxHealth);
            }
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
            playerHealth.OnDeath -= ShowGameOver;
        }
    }

    void UpdateHealthBar(float pct)
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = pct;
    }

    void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            // Optionally pause game logic?
            // Time.timeScale = 0; 
        }
    }

    void GoToMainMenu()
    {
        // Reset TimeScale if we paused
        Time.timeScale = 1;

        // Destroy persistent objects if needed so they don't duplicate when coming back?
        // Actually, Main Menu is a separate scene.
        // If we go back to Bootstrap, we usually want a fresh start.
        // Destroy EnemyManager and RankingCanvas to force clean re-init in Bootstrap?
        if (EnemyManager.Instance != null) Destroy(EnemyManager.Instance.gameObject);
        
        GameObject rankingCanvas = GameObject.Find("RankingCanvas");
        if (rankingCanvas != null) Destroy(rankingCanvas);
        
        // Also destroy the Player/LoadingUI if they are DontDestroyOnLoad?
        // The Bootstrap script creates them new every time.
        // So we should verify if duplicates happen.
        // For now, let's just load MainMenu.
        SceneManager.LoadScene("MainMenu");
    }
}
