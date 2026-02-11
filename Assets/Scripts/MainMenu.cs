using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // The name of the main gameplay scene.
    // Ensure this matches exactly with the scene name in your project.
    private const string GameSceneName = "Bootstrap";

    /// <summary>
    /// Resumes the game by unpausing time and loading the last played scene.
    /// Currently, this just loads the main game scene as a placeholder for a save/load system.
    /// </summary>
    public void ResumeGame()
    {
        // 1. Unpause the game.
        Time.timeScale = 1f;

        // 2. Logic to handle "Resume":
        // If we have a persistent player (mid-session), we might want to reset their position 
        // if we are loading back into the Bootstrap hub to avoid falling.
        if (PlayerPersistent.Instance != null && PlayerPersistent.Instance.gameObject != null)
        {
            // Reset position to safe hub spawn if loading Bootstrap
            // Assuming (0, 0.5, 0) is safe based on SceneSetupTools
            // But if we are resuming to a Map, we wouldn't do this.
            // For now, since we only load Bootstrap:
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

    /// <summary>
    /// Starts a new game by ensuring time is unpaused, destroying old session state, and loading a fresh instance.
    /// </summary>
    public void NewGame()
    {
        // 1. Reset time scale.
        Time.timeScale = 1f;

        // 2. Cleanup old Singletons to ensure fresh start
        if (PlayerPersistent.Instance != null) Destroy(PlayerPersistent.Instance.gameObject);
        if (PersistentCamera.Instance != null) Destroy(PersistentCamera.Instance.gameObject);
        if (EnemyManager.Instance != null) Destroy(EnemyManager.Instance.gameObject);
        if (LoadingUI.Instance != null) Destroy(LoadingUI.Instance.gameObject);

        // 3. Load the main gameplay scene (Bootstrap).
        Debug.Log("New Game: Loading " + GameSceneName);
        SceneManager.LoadScene(GameSceneName);
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit Game requested.");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
