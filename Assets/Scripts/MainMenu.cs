using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private const string GameSceneName = "Bootstrap";

    // Selected difficulty (default Normal)
    private DifficultyConfig.Difficulty selectedDifficulty = DifficultyConfig.Difficulty.Normal;

    // UI references for difficulty buttons
    private Image[] difficultyButtonBorders;
    private Text difficultyLabel;

    void Start()
    {
        // Create difficulty selector UI if not already present
        CreateDifficultySelector();
    }

    private void CreateDifficultySelector()
    {
        // Find the CenterCard panel (parent of all buttons)  
        // The existing scene layout (Y positions from center):
        //   TitleText: 140, SubtitleText: 85, Separator: 55
        //   ResumeButton: 5, NewGameButton: -65, QuitButton: -135, Tagline: -200
        
        // We need to shift existing elements to make room for the difficulty panel
        // New layout:
        //   TitleText: 190, SubtitleText: 145, Separator: 120
        //   ResumeButton: 75, NewGameButton: 10
        //   [DIFFICULTY PANEL: -55 to -125]
        //   QuitButton: -165, Tagline: -210
        
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        // Find and reposition existing elements by name
        float shift = 50f; // Shift upper elements up by 50
        string[] upperElements = { "TitleText", "SubtitleText", "Separator", "ResumeButton", "NewGameButton", "AccentBar" };
        string[] lowerElements = { "QuitButton", "Tagline" };

        foreach (Transform t in canvas.GetComponentsInChildren<Transform>(true))
        {
            RectTransform rt = t as RectTransform;
            if (rt == null) continue;

            foreach (string name in upperElements)
            {
                if (t.name == name)
                {
                    Vector2 pos = rt.anchoredPosition;
                    pos.y += shift;
                    rt.anchoredPosition = pos;
                }
            }
            foreach (string name in lowerElements)
            {
                if (t.name == name)
                {
                    Vector2 pos = rt.anchoredPosition;
                    pos.y -= 30f; // Push down a bit
                    rt.anchoredPosition = pos;
                }
            }
            
            // Also expand the CenterCard to fit
            if (t.name == "CenterCard")
            {
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, 560);
            }
        }

        // Find the CenterCard to parent our difficulty panel to it
        Transform centerCard = null;
        foreach (Transform t in canvas.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "CenterCard") { centerCard = t; break; }
        }
        if (centerCard == null) centerCard = canvas.transform;

        // Difficulty Panel — positioned between NewGameButton and QuitButton
        GameObject panel = new GameObject("DifficultyPanel");
        panel.transform.SetParent(centerCard, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, -80);
        panelRect.sizeDelta = new Vector2(380, 90);

        // Semi-transparent background
        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.05f, 0.08f, 0.6f);

        // "DIFFICULTY" label
        GameObject titleObj = new GameObject("DiffTitle");
        titleObj.transform.SetParent(panel.transform, false);
        Text title = titleObj.AddComponent<Text>();
        title.text = "DIFFICULTY";
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 12;
        title.fontStyle = FontStyle.Bold;
        title.color = new Color(0.6f, 0.6f, 0.7f);
        title.alignment = TextAnchor.MiddleCenter;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.75f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        // 4 difficulty buttons in a row
        DifficultyConfig.Difficulty[] levels = {
            DifficultyConfig.Difficulty.Easy,
            DifficultyConfig.Difficulty.Normal,
            DifficultyConfig.Difficulty.Hard,
            DifficultyConfig.Difficulty.Nightmare
        };

        difficultyButtonBorders = new Image[levels.Length];

        float buttonWidth = 80f;
        float gap = 8f;
        float totalWidth = buttonWidth * levels.Length + gap * (levels.Length - 1);
        float startX = -totalWidth / 2f + buttonWidth / 2f;

        for (int i = 0; i < levels.Length; i++)
        {
            DifficultyConfig.Difficulty diff = levels[i];
            Color color = DifficultyConfig.GetDifficultyColor(diff);
            string name = DifficultyConfig.GetDifficultyName(diff);

            GameObject btnObj = new GameObject("Btn_" + name);
            btnObj.transform.SetParent(panel.transform, false);

            Image borderImg = btnObj.AddComponent<Image>();
            borderImg.color = diff == selectedDifficulty
                ? new Color(color.r, color.g, color.b, 0.9f)
                : new Color(color.r, color.g, color.b, 0.25f);
            difficultyButtonBorders[i] = borderImg;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1.2f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            btn.colors = colors;

            int capturedIndex = i;
            DifficultyConfig.Difficulty capturedDiff = diff;
            btn.onClick.AddListener(() => SelectDifficulty(capturedDiff, capturedIndex));

            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = new Vector2(startX + i * (buttonWidth + gap), -5);
            btnRect.sizeDelta = new Vector2(buttonWidth, 38);

            // Inner dark fill
            GameObject innerObj = new GameObject("InnerBG");
            innerObj.transform.SetParent(btnObj.transform, false);
            Image innerImg = innerObj.AddComponent<Image>();
            innerImg.color = new Color(0.03f, 0.03f, 0.06f, 0.92f);
            innerImg.raycastTarget = false;
            RectTransform innerRect = innerObj.GetComponent<RectTransform>();
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            innerRect.sizeDelta = new Vector2(-3, -3);

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            Text txt = textObj.AddComponent<Text>();
            txt.text = name;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 11;
            txt.fontStyle = FontStyle.Bold;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }

        // Description text at bottom of panel
        GameObject descObj = new GameObject("DiffDescription");
        descObj.transform.SetParent(panel.transform, false);
        difficultyLabel = descObj.AddComponent<Text>();
        difficultyLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        difficultyLabel.fontSize = 10;
        difficultyLabel.color = new Color(0.5f, 0.5f, 0.6f);
        difficultyLabel.alignment = TextAnchor.MiddleCenter;
        difficultyLabel.raycastTarget = false;
        RectTransform descRect = descObj.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0f, 0f);
        descRect.anchorMax = new Vector2(1f, 0.2f);
        descRect.offsetMin = new Vector2(5, 0);
        descRect.offsetMax = new Vector2(-5, 0);

        UpdateDifficultyDescription();
    }

    private void SelectDifficulty(DifficultyConfig.Difficulty diff, int index)
    {
        selectedDifficulty = diff;

        // Update button visuals
        DifficultyConfig.Difficulty[] levels = {
            DifficultyConfig.Difficulty.Easy,
            DifficultyConfig.Difficulty.Normal,
            DifficultyConfig.Difficulty.Hard,
            DifficultyConfig.Difficulty.Nightmare
        };

        for (int i = 0; i < difficultyButtonBorders.Length; i++)
        {
            Color c = DifficultyConfig.GetDifficultyColor(levels[i]);
            difficultyButtonBorders[i].color = i == index
                ? new Color(c.r, c.g, c.b, 0.9f)
                : new Color(c.r, c.g, c.b, 0.3f);
        }

        UpdateDifficultyDescription();
    }

    private void UpdateDifficultyDescription()
    {
        if (difficultyLabel == null) return;
        switch (selectedDifficulty)
        {
            case DifficultyConfig.Difficulty.Easy:
                difficultyLabel.text = "8 Enemies · 4-5 Wormholes · Regen 8 HP/s · Defend 30s";
                break;
            case DifficultyConfig.Difficulty.Normal:
                difficultyLabel.text = "16 Enemies · 2-3 Wormholes · Regen 5 HP/s · Defend 45s";
                break;
            case DifficultyConfig.Difficulty.Hard:
                difficultyLabel.text = "24 Enemies · 1-2 Wormholes · Regen 3 HP/s · Defend 60s";
                break;
            case DifficultyConfig.Difficulty.Nightmare:
                difficultyLabel.text = "32 Enemies · 1 Wormhole · NO Regen · Defend 90s";
                break;
        }
    }

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

        // Create DifficultyConfig with selected difficulty
        GameObject configGO = new GameObject("DifficultyConfig");
        DifficultyConfig config = configGO.AddComponent<DifficultyConfig>();
        // DontDestroyOnLoad is handled in Awake

        // Apply selected difficulty after Awake runs
        config.SetDifficulty(selectedDifficulty);

        Debug.Log("New Game: Difficulty=" + selectedDifficulty + " Loading " + GameSceneName);
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
