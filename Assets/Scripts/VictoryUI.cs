using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VictoryUI : MonoBehaviour
{
    public static VictoryUI Instance { get; private set; }

    private GameObject victoryPanel;
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

    void Start()
    {
        if (GameWinManager.Instance != null)
            GameWinManager.Instance.OnVictory += ShowVictory;
    }

    void Update()
    {
        if (GameWinManager.Instance != null)
        {
            GameWinManager.Instance.OnVictory -= ShowVictory;
            GameWinManager.Instance.OnVictory += ShowVictory;
        }
    }

    private void CreateUI()
    {
        canvasGO = new GameObject("VictoryCanvas");
        canvasGO.transform.SetParent(transform);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Dark green overlay
        victoryPanel = new GameObject("VictoryPanel");
        victoryPanel.transform.SetParent(canvasGO.transform, false);
        Image panelImage = victoryPanel.AddComponent<Image>();
        panelImage.color = new Color(0.02f, 0.08f, 0.04f, 0.92f);
        RectTransform panelRect = victoryPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // Top accent line (gold)
        CreateAccentLine(victoryPanel.transform, 140, new Color(1f, 0.85f, 0.2f, 0.8f));

        // "YOU CONQUERED" title
        GameObject titleGO = new GameObject("TitleText");
        titleGO.transform.SetParent(victoryPanel.transform, false);
        Text titleText = titleGO.AddComponent<Text>();
        titleText.text = "YOU CONQUERED";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 72;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = new Color(0.2f, 1f, 0.4f);
        titleText.alignment = TextAnchor.MiddleCenter;
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0, 90);
        titleRect.sizeDelta = new Vector2(900, 100);

        // "THE PLANE!" subtitle
        GameObject subGO = new GameObject("SubtitleText");
        subGO.transform.SetParent(victoryPanel.transform, false);
        Text subText = subGO.AddComponent<Text>();
        subText.text = "THE PLANE!";
        subText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subText.fontSize = 52;
        subText.fontStyle = FontStyle.Bold;
        subText.color = new Color(1f, 0.85f, 0.2f);
        subText.alignment = TextAnchor.MiddleCenter;
        RectTransform subRect = subGO.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0.5f, 0.5f);
        subRect.anchorMax = new Vector2(0.5f, 0.5f);
        subRect.pivot = new Vector2(0.5f, 0.5f);
        subRect.anchoredPosition = new Vector2(0, 20);
        subRect.sizeDelta = new Vector2(600, 70);

        // Description
        GameObject descGO = new GameObject("DescText");
        descGO.transform.SetParent(victoryPanel.transform, false);
        Text descText = descGO.AddComponent<Text>();
        descText.text = "The spike held for 60 seconds. Victory is yours.";
        descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        descText.fontSize = 20;
        descText.color = new Color(0.6f, 0.9f, 0.7f);
        descText.alignment = TextAnchor.MiddleCenter;
        RectTransform descRect = descGO.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.5f, 0.5f);
        descRect.anchorMax = new Vector2(0.5f, 0.5f);
        descRect.pivot = new Vector2(0.5f, 0.5f);
        descRect.anchoredPosition = new Vector2(0, -35);
        descRect.sizeDelta = new Vector2(600, 35);

        // Bottom accent line
        CreateAccentLine(victoryPanel.transform, -60, new Color(0.2f, 1f, 0.4f, 0.6f));

        // "PLAY AGAIN" button
        GameObject buttonGO = new GameObject("PlayAgainButton");
        buttonGO.transform.SetParent(victoryPanel.transform, false);
        Image buttonBorder = buttonGO.AddComponent<Image>();
        buttonBorder.color = new Color(0.2f, 1f, 0.4f, 0.7f);
        Button button = buttonGO.AddComponent<Button>();
        ColorBlock btnColors = button.colors;
        btnColors.normalColor = new Color(0.2f, 1f, 0.4f, 0.7f);
        btnColors.highlightedColor = new Color(0.3f, 1f, 0.5f, 1f);
        btnColors.pressedColor = new Color(0.15f, 0.7f, 0.3f, 1f);
        button.colors = btnColors;
        RectTransform btnRect = buttonGO.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = new Vector2(0, -110);
        btnRect.sizeDelta = new Vector2(240, 50);

        // Inner dark fill
        GameObject innerBG = new GameObject("InnerBG");
        innerBG.transform.SetParent(buttonGO.transform, false);
        Image innerImg = innerBG.AddComponent<Image>();
        innerImg.color = new Color(0.02f, 0.08f, 0.04f, 0.95f);
        RectTransform innerRect = innerBG.GetComponent<RectTransform>();
        innerRect.anchorMin = Vector2.zero;
        innerRect.anchorMax = Vector2.one;
        innerRect.sizeDelta = new Vector2(-4, -4);

        GameObject btnTextGO = new GameObject("ButtonText");
        btnTextGO.transform.SetParent(buttonGO.transform, false);
        Text btnText = btnTextGO.AddComponent<Text>();
        btnText.text = "PLAY AGAIN";
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 22;
        btnText.fontStyle = FontStyle.Bold;
        btnText.color = new Color(0.2f, 1f, 0.4f);
        btnText.alignment = TextAnchor.MiddleCenter;
        RectTransform btnTextRect = btnTextGO.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;

        button.onClick.AddListener(RestartGame);
        victoryPanel.SetActive(false);
    }

    private void CreateAccentLine(Transform parent, float yOffset, Color color)
    {
        GameObject lineObj = new GameObject("AccentLine");
        lineObj.transform.SetParent(parent, false);
        Image lineImg = lineObj.AddComponent<Image>();
        lineImg.color = color;
        RectTransform lineRect = lineObj.GetComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0.15f, 0.5f);
        lineRect.anchorMax = new Vector2(0.85f, 0.5f);
        lineRect.pivot = new Vector2(0.5f, 0.5f);
        lineRect.anchoredPosition = new Vector2(0, yOffset);
        lineRect.sizeDelta = new Vector2(0, 2);
    }

    private void ShowVictory()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    private void RestartGame()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);
        Time.timeScale = 1f;

        Destroy(gameObject);
        if (GameWinManager.Instance != null) Destroy(GameWinManager.Instance.gameObject);
        if (HealthBarUI.Instance != null) Destroy(HealthBarUI.Instance.gameObject);
        if (MapStatusUI.Instance != null) Destroy(MapStatusUI.Instance.gameObject);
        if (EnemyManager.Instance != null) Destroy(EnemyManager.Instance.gameObject);
        if (GameOverUI.Instance != null) Destroy(GameOverUI.Instance.gameObject);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) Destroy(player);
        GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
        if (cam != null) Destroy(cam);

        SceneManager.LoadScene(0);
    }
}
