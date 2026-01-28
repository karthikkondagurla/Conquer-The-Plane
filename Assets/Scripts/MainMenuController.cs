using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Button playButton;
    public Button quitButton;

    void Start()
    {
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    void PlayGame()
    {
        // Load the Bootstrap scene which initializes everything
        SceneManager.LoadScene("Bootstrap");
    }

    void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
