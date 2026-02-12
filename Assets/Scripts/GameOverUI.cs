using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    private GameObject gameOverPanel;
    private GameObject canvasGO;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start() { SubscribeToPlayerDeath(); }
    void Update() { SubscribeToPlayerDeath(); }

    private void SubscribeToPlayerDeath()
    {
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= ShowGameOver;
            playerHealth.OnDeath += ShowGameOver;
        }
    }

    private void CreateUI()
    {
        EnsureEventSystem();

        canvasGO = new GameObject("GameOverCanvas");
        canvasGO.transform.SetParent(transform);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Full-screen dark overlay
        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasGO.transform, false);
        Image panelImage = gameOverPanel.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.02f, 0.02f, 0.92f);

        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // Red accent line at top
        GameObject topLine = new GameObject("TopLine");
        topLine.transform.SetParent(gameOverPanel.transform, false);
        Image topLineImg = topLine.AddComponent<Image>();
        topLineImg.color = new Color(1f, 0.15f, 0.15f, 0.8f);
        RectTransform topLineRect = topLine.GetComponent<RectTransform>();
        topLineRect.anchorMin = new Vector2(0.2f, 0.5f);
        topLineRect.anchorMax = new Vector2(0.8f, 0.5f);
        topLineRect.pivot = new Vector2(0.5f, 0.5f);
        topLineRect.anchoredPosition = new Vector2(0, 110);
        topLineRect.sizeDelta = new Vector2(0, 2);

        // "WASTED" text
        GameObject wastedGO = new GameObject("WastedText");
        wastedGO.transform.SetParent(gameOverPanel.transform, false);
        Text wastedText = wastedGO.AddComponent<Text>();
        wastedText.text = "WASTED";
        wastedText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        wastedText.fontSize = 100;
        wastedText.fontStyle = FontStyle.Bold;
        wastedText.color = new Color(1f, 0.12f, 0.12f);
        wastedText.alignment = TextAnchor.MiddleCenter;
        RectTransform wastedRect = wastedGO.GetComponent<RectTransform>();
        wastedRect.anchorMin = new Vector2(0.5f, 0.5f);
        wastedRect.anchorMax = new Vector2(0.5f, 0.5f);
        wastedRect.pivot = new Vector2(0.5f, 0.5f);
        wastedRect.anchoredPosition = new Vector2(0, 55);
        wastedRect.sizeDelta = new Vector2(800, 130);

        // Subtitle
        GameObject subGO = new GameObject("SubText");
        subGO.transform.SetParent(gameOverPanel.transform, false);
        Text subText = subGO.AddComponent<Text>();
        subText.text = "The plane remains unconquered.";
        subText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subText.fontSize = 20;
        subText.fontStyle = FontStyle.Italic;
        subText.color = new Color(0.7f, 0.3f, 0.3f);
        subText.alignment = TextAnchor.MiddleCenter;
        RectTransform subRect = subGO.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0.5f, 0.5f);
        subRect.anchorMax = new Vector2(0.5f, 0.5f);
        subRect.pivot = new Vector2(0.5f, 0.5f);
        subRect.anchoredPosition = new Vector2(0, -15);
        subRect.sizeDelta = new Vector2(600, 35);

        // Red accent line below subtitle
        GameObject bottomLine = new GameObject("BottomLine");
        bottomLine.transform.SetParent(gameOverPanel.transform, false);
        Image bottomLineImg = bottomLine.AddComponent<Image>();
        bottomLineImg.color = new Color(1f, 0.15f, 0.15f, 0.8f);
        RectTransform bottomLineRect = bottomLine.GetComponent<RectTransform>();
        bottomLineRect.anchorMin = new Vector2(0.2f, 0.5f);
        bottomLineRect.anchorMax = new Vector2(0.8f, 0.5f);
        bottomLineRect.pivot = new Vector2(0.5f, 0.5f);
        bottomLineRect.anchoredPosition = new Vector2(0, -40);
        bottomLineRect.sizeDelta = new Vector2(0, 2);

        // "Try Again" button
        GameObject buttonGO = new GameObject("TryAgainButton");
        buttonGO.transform.SetParent(gameOverPanel.transform, false);
        Image buttonBorder = buttonGO.AddComponent<Image>();
        buttonBorder.color = new Color(1f, 0.2f, 0.2f, 0.7f);
        Button button = buttonGO.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1f, 0.2f, 0.2f, 0.7f);
        colors.highlightedColor = new Color(1f, 0.3f, 0.3f, 1f);
        colors.pressedColor = new Color(0.8f, 0.1f, 0.1f, 1f);
        button.colors = colors;
        RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0, -90);
        buttonRect.sizeDelta = new Vector2(220, 50);

        // Inner dark fill
        GameObject innerBG = new GameObject("InnerBG");
        innerBG.transform.SetParent(buttonGO.transform, false);
        Image innerImg = innerBG.AddComponent<Image>();
        innerImg.color = new Color(0.08f, 0.02f, 0.02f, 0.95f);
        RectTransform innerRect = innerBG.GetComponent<RectTransform>();
        innerRect.anchorMin = Vector2.zero;
        innerRect.anchorMax = Vector2.one;
        innerRect.sizeDelta = new Vector2(-4, -4);

        // Button text
        GameObject buttonTextGO = new GameObject("ButtonText");
        buttonTextGO.transform.SetParent(buttonGO.transform, false);
        Text btnText = buttonTextGO.AddComponent<Text>();
        btnText.text = "TRY AGAIN";
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 22;
        btnText.fontStyle = FontStyle.Bold;
        btnText.color = new Color(1f, 0.4f, 0.4f);
        btnText.alignment = TextAnchor.MiddleCenter;
        RectTransform btnTextRect = buttonTextGO.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;

        button.onClick.AddListener(RestartGame);
        gameOverPanel.SetActive(false);
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    private void RestartGame()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        Time.timeScale = 1f;

        Destroy(gameObject);

        if (HealthBarUI.Instance != null) Destroy(HealthBarUI.Instance.gameObject);
        if (MapStatusUI.Instance != null) Destroy(MapStatusUI.Instance.gameObject);
        if (EnemyManager.Instance != null) Destroy(EnemyManager.Instance.gameObject);
        if (GameWinManager.Instance != null) Destroy(GameWinManager.Instance.gameObject);
        if (VictoryUI.Instance != null) Destroy(VictoryUI.Instance.gameObject);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) Destroy(player);
        GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
        if (cam != null) Destroy(cam);

        SceneManager.LoadScene(0);
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
