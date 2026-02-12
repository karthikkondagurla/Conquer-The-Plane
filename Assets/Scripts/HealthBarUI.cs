using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthBarUI : MonoBehaviour
{
    public static HealthBarUI Instance { get; private set; }

    private Image healthBarFill;
    private Image healthBarBorder;
    private Text healthText;
    private Text healthLabel;
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
        SubscribeToPlayerHealth();
        UpdateVisibility();
    }

    void Update()
    {
        if (healthBarFill != null) SubscribeToPlayerHealth();
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { UpdateVisibility(); }

    private void UpdateVisibility()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        bool isMapScene = sceneName.StartsWith("Map");
        if (canvasGO != null) canvasGO.SetActive(isMapScene);
    }

    private void SubscribeToPlayerHealth()
    {
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
            playerHealth.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar(playerHealth.currentHealth / playerHealth.maxHealth);
        }
    }

    private void CreateUI()
    {
        EnsureEventSystem();

        canvasGO = new GameObject("HealthBarCanvas");
        canvasGO.transform.SetParent(transform);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Container (top-right)
        GameObject container = new GameObject("HealthBarContainer");
        container.transform.SetParent(canvasGO.transform, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(1, 1);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.pivot = new Vector2(1, 1);
        containerRect.anchoredPosition = new Vector2(-20, -20);
        containerRect.sizeDelta = new Vector2(380, 65);

        // Label: "HP"
        GameObject labelGO = new GameObject("HPLabel");
        labelGO.transform.SetParent(container.transform, false);
        healthLabel = labelGO.AddComponent<Text>();
        healthLabel.text = "HP";
        healthLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        healthLabel.fontSize = 20;
        healthLabel.fontStyle = FontStyle.Bold;
        healthLabel.color = new Color(0f, 0.9f, 1f, 0.8f);
        healthLabel.alignment = TextAnchor.MiddleLeft;
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 1);
        labelRect.anchorMax = new Vector2(0, 1);
        labelRect.pivot = new Vector2(0, 1);
        labelRect.anchoredPosition = new Vector2(0, 0);
        labelRect.sizeDelta = new Vector2(45, 26);

        // Outer border
        GameObject borderGO = new GameObject("HealthBarBorder");
        borderGO.transform.SetParent(container.transform, false);
        healthBarBorder = borderGO.AddComponent<Image>();
        healthBarBorder.color = new Color(0f, 0.9f, 1f, 0.5f);
        RectTransform borderRect = borderGO.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0, 0);
        borderRect.anchorMax = new Vector2(1, 1);
        borderRect.pivot = new Vector2(0.5f, 0.5f);
        borderRect.anchoredPosition = new Vector2(0, -10);
        borderRect.sizeDelta = new Vector2(0, -20);

        // Inner dark background
        GameObject bgGO = new GameObject("HealthBarBG");
        bgGO.transform.SetParent(borderGO.transform, false);
        Image bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0.05f, 0.05f, 0.08f, 0.9f);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = new Vector2(-4, -4);

        // Fill bar
        GameObject fillGO = new GameObject("HealthBarFill");
        fillGO.transform.SetParent(bgGO.transform, false);
        healthBarFill = fillGO.AddComponent<Image>();
        healthBarFill.color = new Color(0f, 1f, 0.6f);
        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.anchoredPosition = new Vector2(2, 0);
        fillRect.sizeDelta = new Vector2(370, -6);

        // Percentage text overlay
        GameObject textGO = new GameObject("HealthText");
        textGO.transform.SetParent(borderGO.transform, false);
        healthText = textGO.AddComponent<Text>();
        healthText.text = "100%";
        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        healthText.fontSize = 20;
        healthText.fontStyle = FontStyle.Bold;
        healthText.color = Color.white;
        healthText.alignment = TextAnchor.MiddleCenter;
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
    }

    private void UpdateHealthBar(float healthPercent)
    {
        if (healthBarFill == null) return;

        RectTransform fillRect = healthBarFill.GetComponent<RectTransform>();
        if (fillRect != null)
        {
            fillRect.localScale = new Vector3(healthPercent, 1, 1);
        }

        if (healthPercent > 0.6f)
        {
            healthBarFill.color = new Color(0f, 1f, 0.6f);
            if (healthBarBorder != null)
                healthBarBorder.color = new Color(0f, 0.9f, 1f, 0.5f);
        }
        else if (healthPercent > 0.3f)
        {
            healthBarFill.color = new Color(1f, 0.75f, 0f);
            if (healthBarBorder != null)
                healthBarBorder.color = new Color(1f, 0.75f, 0f, 0.5f);
        }
        else
        {
            healthBarFill.color = new Color(1f, 0.15f, 0.15f);
            if (healthBarBorder != null)
                healthBarBorder.color = new Color(1f, 0.15f, 0.15f, 0.6f);
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(healthPercent * 100)}%";
        }
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
