using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MapStatusUI : MonoBehaviour
{
    public static MapStatusUI Instance { get; private set; }

    private Dictionary<int, Text> mapCountTexts = new Dictionary<int, Text>();
    private Dictionary<int, Image> mapCardImages = new Dictionary<int, Image>();
    private Dictionary<int, Image> mapCardBorders = new Dictionary<int, Image>();
    private Dictionary<int, Text> mapNameTexts = new Dictionary<int, Text>();
    private GameObject canvasGO;

    // Countdown UI
    private Text countdownText;
    private Text statusText;

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
        UpdateAllCounts();
        UpdateVisibility();
        SubscribeToEvents();
    }

    void Update()
    {
        UpdateAllCounts();
        UpdateCountdown();
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        if (GameWinManager.Instance != null)
        {
            GameWinManager.Instance.OnSpikeActivated -= OnSpikeActivated;
            GameWinManager.Instance.OnSpikeActivated += OnSpikeActivated;
            GameWinManager.Instance.OnSpikeDeactivated -= OnSpikeDeactivated;
            GameWinManager.Instance.OnSpikeDeactivated += OnSpikeDeactivated;
        }
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

    private void CreateUI()
    {
        EnsureEventSystem();

        canvasGO = new GameObject("MapStatusCanvas");
        canvasGO.transform.SetParent(transform);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Container (top-left)
        GameObject container = new GameObject("MapStatusContainer");
        container.transform.SetParent(canvasGO.transform, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = new Vector2(18, -18);
        containerRect.sizeDelta = new Vector2(270, 400);

        // "MAPS" label
        GameObject labelGO = new GameObject("MapsLabel");
        labelGO.transform.SetParent(container.transform, false);
        Text labelText = labelGO.AddComponent<Text>();
        labelText.text = "MAPS";
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 18;
        labelText.fontStyle = FontStyle.Bold;
        labelText.color = new Color(0.5f, 0.5f, 0.6f);
        labelText.alignment = TextAnchor.MiddleLeft;
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 1);
        labelRect.anchorMax = new Vector2(0, 1);
        labelRect.pivot = new Vector2(0, 1);
        labelRect.anchoredPosition = new Vector2(2, 0);
        labelRect.sizeDelta = new Vector2(90, 24);

        // Create 4 map cards
        for (int i = 1; i <= 4; i++)
        {
            CreateMapCard(container.transform, i, 28 + (i - 1) * 62);
        }

        // Countdown timer text
        GameObject countdownGO = new GameObject("CountdownText");
        countdownGO.transform.SetParent(container.transform, false);
        countdownText = countdownGO.AddComponent<Text>();
        countdownText.text = "";
        countdownText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        countdownText.fontSize = 24;
        countdownText.fontStyle = FontStyle.Bold;
        countdownText.color = new Color(1f, 0.85f, 0.2f);
        countdownText.alignment = TextAnchor.MiddleCenter;
        RectTransform cdRect = countdownGO.GetComponent<RectTransform>();
        cdRect.anchorMin = new Vector2(0, 1);
        cdRect.anchorMax = new Vector2(0, 1);
        cdRect.pivot = new Vector2(0, 1);
        cdRect.anchoredPosition = new Vector2(0, -285);
        cdRect.sizeDelta = new Vector2(270, 32);

        // Status text
        GameObject statusGO = new GameObject("StatusText");
        statusGO.transform.SetParent(container.transform, false);
        statusText = statusGO.AddComponent<Text>();
        statusText.text = "";
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.fontSize = 19;
        statusText.fontStyle = FontStyle.Bold;
        statusText.color = new Color(1f, 0.3f, 0.3f);
        statusText.alignment = TextAnchor.MiddleCenter;
        RectTransform stRect = statusGO.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(0, 1);
        stRect.anchorMax = new Vector2(0, 1);
        stRect.pivot = new Vector2(0, 1);
        stRect.anchoredPosition = new Vector2(0, -320);
        stRect.sizeDelta = new Vector2(270, 32);
    }

    private void CreateMapCard(Transform parent, int mapID, float yOffset)
    {
        // Outer border
        GameObject borderGO = new GameObject($"MapBorder_{mapID}");
        borderGO.transform.SetParent(parent, false);
        Image borderImage = borderGO.AddComponent<Image>();
        borderImage.color = new Color(0.3f, 0.3f, 0.35f, 0.5f);
        RectTransform borderRect = borderGO.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0, 1);
        borderRect.anchorMax = new Vector2(0, 1);
        borderRect.pivot = new Vector2(0, 1);
        borderRect.anchoredPosition = new Vector2(0, -yOffset);
        borderRect.sizeDelta = new Vector2(270, 58);

        // Inner dark card
        GameObject cardGO = new GameObject($"MapCard_{mapID}");
        cardGO.transform.SetParent(borderGO.transform, false);
        Image cardImage = cardGO.AddComponent<Image>();
        cardImage.color = new Color(0.06f, 0.06f, 0.09f, 0.9f);
        RectTransform cardRect = cardGO.GetComponent<RectTransform>();
        cardRect.anchorMin = Vector2.zero;
        cardRect.anchorMax = Vector2.one;
        cardRect.sizeDelta = new Vector2(-3, -3);

        // Map name
        GameObject nameGO = new GameObject("MapNameText");
        nameGO.transform.SetParent(borderGO.transform, false);
        Text mapNameText = nameGO.AddComponent<Text>();
        mapNameText.text = $"Map {mapID}";
        mapNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        mapNameText.fontSize = 20;
        mapNameText.fontStyle = FontStyle.Bold;
        mapNameText.color = new Color(0.8f, 0.8f, 0.85f);
        mapNameText.alignment = TextAnchor.MiddleLeft;
        RectTransform nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0);
        nameRect.anchorMax = new Vector2(0.55f, 1);
        nameRect.pivot = new Vector2(0, 0.5f);
        nameRect.anchoredPosition = new Vector2(10, 0);
        nameRect.sizeDelta = Vector2.zero;

        // Enemy count
        GameObject countGO = new GameObject("EnemyCountText");
        countGO.transform.SetParent(borderGO.transform, false);
        Text countText = countGO.AddComponent<Text>();
        countText.text = "0";
        countText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        countText.fontSize = 19;
        countText.color = new Color(0.6f, 0.6f, 0.65f);
        countText.alignment = TextAnchor.MiddleRight;
        RectTransform countRect = countGO.GetComponent<RectTransform>();
        countRect.anchorMin = new Vector2(0.55f, 0);
        countRect.anchorMax = new Vector2(1, 1);
        countRect.pivot = new Vector2(1, 0.5f);
        countRect.anchoredPosition = new Vector2(-10, 0);
        countRect.sizeDelta = Vector2.zero;

        mapCountTexts[mapID] = countText;
        mapCardImages[mapID] = cardImage;
        mapCardBorders[mapID] = borderImage;
        mapNameTexts[mapID] = mapNameText;
    }

    private void UpdateAllCounts()
    {
        if (EnemyManager.Instance == null) return;

        int demandingMapID = GameWinManager.Instance != null ? GameWinManager.Instance.DemandingMapID : 0;

        List<KeyValuePair<int, int>> mapCounts = new List<KeyValuePair<int, int>>();
        for (int mapID = 1; mapID <= 4; mapID++)
        {
            int count = EnemyManager.Instance.GetEnemyCount(mapID);
            mapCounts.Add(new KeyValuePair<int, int>(mapID, count));
        }

        mapCounts.Sort((a, b) => b.Value.CompareTo(a.Value));

        for (int i = 0; i < mapCounts.Count; i++)
        {
            int mapID = mapCounts[i].Key;
            int count = mapCounts[i].Value;
            
            if (!mapCountTexts.ContainsKey(mapID)) continue;

            string enemyWord = count == 1 ? "enemy" : "enemies";
            mapCountTexts[mapID].text = $"{count} {enemyWord}";
            
            RectTransform borderRect = mapCardBorders[mapID].GetComponent<RectTransform>();
            float yPos = 28 + i * 62;
            borderRect.anchoredPosition = new Vector2(0, -yPos);

            if (mapID == demandingMapID)
            {
                mapCardBorders[mapID].color = new Color(1f, 0.75f, 0f, 0.7f);
                mapCardImages[mapID].color = new Color(0.12f, 0.08f, 0.02f, 0.9f);
                mapNameTexts[mapID].text = $"⚡ Map {mapID}";
                mapNameTexts[mapID].color = new Color(1f, 0.85f, 0.2f);
                mapCountTexts[mapID].color = new Color(1f, 0.85f, 0.2f, 0.8f);
            }
            else
            {
                mapCardBorders[mapID].color = new Color(0.3f, 0.3f, 0.35f, 0.4f);
                mapCardImages[mapID].color = new Color(0.06f, 0.06f, 0.09f, 0.9f);
                mapNameTexts[mapID].text = $"Map {mapID}";
                mapNameTexts[mapID].color = new Color(0.8f, 0.8f, 0.85f);
                mapCountTexts[mapID].color = new Color(0.6f, 0.6f, 0.65f);
            }
        }
    }

    private void UpdateCountdown()
    {
        if (GameWinManager.Instance == null || countdownText == null) return;

        if (GameWinManager.Instance.IsVictorySpikeActive)
        {
            float remaining = GameWinManager.Instance.CountdownRemaining;
            countdownText.text = $"⚡ DEFEND: {remaining:F0}s";

            if (remaining <= 10f)
            {
                float flash = Mathf.PingPong(Time.time * 4f, 1f);
                countdownText.color = Color.Lerp(new Color(1f, 0.3f, 0.3f), new Color(1f, 0.85f, 0.2f), flash);
            }
            else
            {
                countdownText.color = new Color(1f, 0.85f, 0.2f);
            }
        }
        else
        {
            countdownText.text = "";
        }
    }

    private void OnSpikeActivated()
    {
        if (statusText != null) statusText.text = "";
    }

    private void OnSpikeDeactivated(string reason)
    {
        if (statusText != null)
        {
            statusText.text = reason;
            StartCoroutine(ClearStatusAfterDelay(3f));
        }
    }

    private System.Collections.IEnumerator ClearStatusAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (statusText != null) statusText.text = "";
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
