using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private const string GameSceneName = "Bootstrap";

    public void ResumeGame()
    {
        Time.timeScale = 1f;

        if (PlayerPersistent.Instance != null && PlayerPersistent.Instance.gameObject != null)
        {
            Rigidbody rb = PlayerPersistent.Instance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            PlayerPersistent.Instance.transform.position = new Vector3(0, 0.5f, 0);
        }

        Debug.Log("Resume Game: Loading " + GameSceneName);
        SceneManager.LoadScene(GameSceneName);
    }

    public void NewGame()
    {
        Time.timeScale = 1f;

        // Cleanup old Singletons
        if (PlayerPersistent.Instance != null) Destroy(PlayerPersistent.Instance.gameObject);
        if (PersistentCamera.Instance != null) Destroy(PersistentCamera.Instance.gameObject);
        if (EnemyManager.Instance != null) Destroy(EnemyManager.Instance.gameObject);
        if (LoadingUI.Instance != null) Destroy(LoadingUI.Instance.gameObject);
        if (SkillCooldownUI.Instance != null) Destroy(SkillCooldownUI.Instance.gameObject);
        if (DifficultyConfig.Instance != null) Destroy(DifficultyConfig.Instance.gameObject);

        // Create DifficultyConfig singleton with fixed values
        GameObject configGO = new GameObject("DifficultyConfig");
        configGO.AddComponent<DifficultyConfig>();
        // DontDestroyOnLoad is handled in Awake

        Debug.Log("New Game: Loading " + GameSceneName);
        SceneManager.LoadScene(GameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game requested.");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
