using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadingUI : MonoBehaviour
{
    public static LoadingUI Instance { get; private set; }
    
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Create UI if not assigned manually (failsafe for automation)
            if (fadeImage == null)
            {
                CreateDefaultUI();
            }
            
            // Ensure starts transparent
            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = 0;
                fadeImage.color = c;
                fadeImage.gameObject.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void CreateDefaultUI()
    {
        EnsureEventSystem();

        // scalable UI creation
        GameObject canvasGO = new GameObject("LoadingCanvas");
        canvasGO.transform.SetParent(transform);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // On top of everything
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        
        GameObject imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);
        
        fadeImage = imageGO.AddComponent<Image>();
        fadeImage.color = Color.black;
        
        // Stretch to fill
        RectTransform rt = imageGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        CreatePauseUI(canvasGO.transform);
    }

    private void CreatePauseUI(Transform parent)
    {
        // Full-screen dark overlay
        GameObject panelObj = new GameObject("PausePanel");
        panelObj.transform.SetParent(parent, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.02f, 0.03f, 0.08f, 0.92f); // Dark blue-tinted
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // "PAUSED" title
        GameObject titleObj = new GameObject("PausedTitle");
        titleObj.transform.SetParent(panelObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "PAUSED";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 56;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = new Color(0f, 0.9f, 1f, 0.8f); // Cyan
        titleText.alignment = TextAnchor.MiddleCenter;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0, 130);
        titleRect.sizeDelta = new Vector2(400, 70);

        // Accent line
        GameObject lineObj = new GameObject("AccentLine");
        lineObj.transform.SetParent(panelObj.transform, false);
        Image lineImg = lineObj.AddComponent<Image>();
        lineImg.color = new Color(0f, 0.9f, 1f, 0.4f);
        RectTransform lineRect = lineObj.GetComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0.3f, 0.5f);
        lineRect.anchorMax = new Vector2(0.7f, 0.5f);
        lineRect.pivot = new Vector2(0.5f, 0.5f);
        lineRect.anchoredPosition = new Vector2(0, 90);
        lineRect.sizeDelta = new Vector2(0, 2);

        PauseMenu pauseMenu = gameObject.AddComponent<PauseMenu>();
        pauseMenu.Setup(panelObj);

        // Styled buttons
        CreatePauseButton("ResumeButton", "RESUME", 40, panelObj.transform, () => pauseMenu.Resume(),
            new Color(0f, 0.9f, 1f, 0.7f), new Color(0f, 0.9f, 1f)); // Cyan
        CreatePauseButton("NewGameButton", "NEW GAME", -30, panelObj.transform, () => pauseMenu.RestartGame(),
            new Color(1f, 0.85f, 0.2f, 0.7f), new Color(1f, 0.85f, 0.2f)); // Gold
        CreatePauseButton("QuitButton", "QUIT", -100, panelObj.transform, () => pauseMenu.QuitGame(),
            new Color(1f, 0.3f, 0.3f, 0.6f), new Color(1f, 0.4f, 0.4f)); // Red
    }

    private void CreatePauseButton(string name, string text, float yOffset, Transform parent, 
        UnityEngine.Events.UnityAction action, Color borderColor, Color textColor)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);

        Image borderImg = buttonObj.AddComponent<Image>();
        borderImg.color = borderColor;

        Button btn = buttonObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = borderColor;
        colors.highlightedColor = new Color(borderColor.r, borderColor.g, borderColor.b, 1f);
        colors.pressedColor = new Color(borderColor.r * 0.7f, borderColor.g * 0.7f, borderColor.b * 0.7f, 1f);
        btn.colors = colors;
        btn.onClick.AddListener(action);

        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(280, 50);
        rect.anchoredPosition = new Vector2(0, yOffset);

        // Inner dark fill
        GameObject innerObj = new GameObject("InnerBG");
        innerObj.transform.SetParent(buttonObj.transform, false);
        Image innerImg = innerObj.AddComponent<Image>();
        innerImg.color = new Color(0.03f, 0.03f, 0.06f, 0.95f);
        RectTransform innerRect = innerObj.GetComponent<RectTransform>();
        innerRect.anchorMin = Vector2.zero;
        innerRect.anchorMax = Vector2.one;
        innerRect.sizeDelta = new Vector2(-4, -4);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text txt = textObj.AddComponent<Text>();
        txt.text = text;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = textColor;
        txt.fontSize = 22;
        txt.fontStyle = FontStyle.Bold;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
    }

    public IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;
        
        fadeImage.gameObject.SetActive(true);
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = t / fadeDuration;
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
            yield return null;
        }
        
        Color final = fadeImage.color;
        final.a = 1;
        fadeImage.color = final;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Failsafe: Ensure we fade in if we loaded a scene
        if (fadeImage != null && fadeImage.gameObject.activeSelf)
        {
            StartCoroutine(FadeIn());
        }
    }

    public IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = 1 - (t / fadeDuration);
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
            yield return null;
        }
        
        Color final = fadeImage.color;
        final.a = 0;
        fadeImage.color = final;
        fadeImage.gameObject.SetActive(false);
    }
    
    private bool isTeleporting = false;

    public void TeleportToScene(int sceneIndex)
    {
        if (isTeleporting) return;
        StartCoroutine(TeleportSequence(sceneIndex));
    }

    private IEnumerator TeleportSequence(int sceneIndex)
    {
        isTeleporting = true;

        // 1. Fade Out
        yield return StartCoroutine(FadeOut());

        // 2. Async Load
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        // Wait until almost done
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // Allow activation
        asyncLoad.allowSceneActivation = true;
        
        // Wait for scene to fully load
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 3. Reposition Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        
        if (player != null && spawnPoints.Length > 0)
        {
            // Pick random spawn point
            GameObject spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Disable rigidbodies/colliders momentarily to prevent physics glitches during warp
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            player.transform.position = spawnPoint.transform.position;
            // Optionally preserve rotation or match spawn point
            // player.transform.rotation = spawnPoint.transform.rotation; 

            if (rb != null) rb.isKinematic = false;
        }
        else
        {
            if (player == null) Debug.LogWarning("Player not found after load!");
            if (spawnPoints.Length == 0) Debug.LogWarning("No SpawnPoints found in new scene!");
        }

        // 4. Fade In
        yield return StartCoroutine(FadeIn());

        isTeleporting = false;
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            DontDestroyOnLoad(eventSystem);
        }
    }
}
