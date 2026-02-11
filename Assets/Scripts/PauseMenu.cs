using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    private bool isPaused = false;

    void Update()
    {
        // Toggle pause on Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f; // Stop time
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f; // Resume time
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    public void RestartGame()
    {
        Resume(); // Ensure time is back to 1 before reloading
        
        // Similar to MainMenu.NewGame logic
        if (PlayerPersistent.Instance != null) Destroy(PlayerPersistent.Instance.gameObject);
        if (PersistentCamera.Instance != null) Destroy(PersistentCamera.Instance.gameObject);
        if (EnemyManager.Instance != null) Destroy(EnemyManager.Instance.gameObject);
        // We don't destroy LoadingUI because it hosts us!
        
        SceneManager.LoadScene("Bootstrap");
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Setup helper called by LoadingUI or creator
    public void Setup(GameObject panel)
    {
        this.menuPanel = panel;
        Resume(); // Start hidden
    }
}
