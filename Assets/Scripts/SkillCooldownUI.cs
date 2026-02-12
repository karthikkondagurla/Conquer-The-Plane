using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownUI : MonoBehaviour
{
    public static SkillCooldownUI Instance { get; private set; }

    private Canvas canvas;
    private GameObject panelObj;

    // Skill icon containers
    private Image shockwaveFill;
    private Image dashFill;
    private Image boltFill;
    private Text shockwaveKey;
    private Text dashKey;
    private Text boltKey;

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

    void Update()
    {
        UpdateCooldowns();
    }

    private void CreateUI()
    {
        // Canvas
        GameObject canvasGO = new GameObject("SkillCooldownCanvas");
        canvasGO.transform.SetParent(transform);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Bottom-center panel
        panelObj = new GameObject("SkillBar");
        panelObj.transform.SetParent(canvasGO.transform, false);
        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0.02f, 0.03f, 0.06f, 0.75f);

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0, 25);
        panelRect.sizeDelta = new Vector2(420, 100);

        // Top accent line
        GameObject line = new GameObject("AccentLine");
        line.transform.SetParent(panelObj.transform, false);
        Image lineImg = line.AddComponent<Image>();
        lineImg.color = new Color(0f, 0.9f, 1f, 0.5f);
        RectTransform lineRect = line.GetComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0f, 1f);
        lineRect.anchorMax = new Vector2(1f, 1f);
        lineRect.pivot = new Vector2(0.5f, 1f);
        lineRect.anchoredPosition = Vector2.zero;
        lineRect.sizeDelta = new Vector2(0, 2);

        // Create 3 skill icons
        Color shockColor = new Color(0f, 0.9f, 1f);    // Cyan
        Color dashColor = new Color(1f, 0.5f, 0f);     // Orange
        Color boltColor = new Color(0.2f, 1f, 0.4f);    // Green

        shockwaveFill = CreateSkillIcon(panelObj.transform, -125, "Q", "SWV", shockColor, out shockwaveKey);
        dashFill = CreateSkillIcon(panelObj.transform, 0, "⇧", "DSH", dashColor, out dashKey);
        boltFill = CreateSkillIcon(panelObj.transform, 125, "F", "BLT", boltColor, out boltKey);
    }

    private Image CreateSkillIcon(Transform parent, float xOffset, string keyLabel, string skillLabel,
        Color accentColor, out Text keyText)
    {
        float iconSize = 72f;

        // Outer border
        GameObject iconObj = new GameObject("SkillIcon_" + skillLabel);
        iconObj.transform.SetParent(parent, false);
        Image borderImg = iconObj.AddComponent<Image>();
        borderImg.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.6f);

        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(xOffset, 0);
        iconRect.sizeDelta = new Vector2(iconSize, iconSize);

        // Inner dark background
        GameObject innerObj = new GameObject("InnerBG");
        innerObj.transform.SetParent(iconObj.transform, false);
        Image innerImg = innerObj.AddComponent<Image>();
        innerImg.color = new Color(0.03f, 0.03f, 0.06f, 0.95f);
        RectTransform innerRect = innerObj.GetComponent<RectTransform>();
        innerRect.anchorMin = Vector2.zero;
        innerRect.anchorMax = Vector2.one;
        innerRect.sizeDelta = new Vector2(-6, -6);

        // Cooldown fill overlay (fills from bottom)
        GameObject fillObj = new GameObject("CooldownFill");
        fillObj.transform.SetParent(innerObj.transform, false);
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.35f);
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Vertical;
        fillImg.fillOrigin = 0; // Bottom
        fillImg.fillAmount = 1f; // Full = ready

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        // Skill label text
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(innerObj.transform, false);
        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = skillLabel;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 16;
        labelText.fontStyle = FontStyle.Bold;
        labelText.color = accentColor;
        labelText.alignment = TextAnchor.MiddleCenter;
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;
        labelRect.anchoredPosition = new Vector2(0, -7);

        // Key label (bottom)
        GameObject keyObj = new GameObject("KeyLabel");
        keyObj.transform.SetParent(iconObj.transform, false);
        keyText = keyObj.AddComponent<Text>();
        keyText.text = keyLabel;
        keyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        keyText.fontSize = 18;
        keyText.fontStyle = FontStyle.Bold;
        keyText.color = Color.white;
        keyText.alignment = TextAnchor.MiddleCenter;
        RectTransform keyRect = keyObj.GetComponent<RectTransform>();
        keyRect.anchorMin = new Vector2(0.5f, 0f);
        keyRect.anchorMax = new Vector2(0.5f, 0f);
        keyRect.pivot = new Vector2(0.5f, 1f);
        keyRect.anchoredPosition = new Vector2(0, -2);
        keyRect.sizeDelta = new Vector2(iconSize, 22);

        return fillImg;
    }

    private void UpdateCooldowns()
    {
        // Find skills on the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        ShockwaveSkill shockwave = player.GetComponent<ShockwaveSkill>();
        DashStrikeSkill dash = player.GetComponent<DashStrikeSkill>();
        EnergyBoltSkill bolt = player.GetComponent<EnergyBoltSkill>();

        if (shockwave != null && shockwaveFill != null)
        {
            float remaining = shockwave.CooldownRemaining;
            float total = shockwave.CooldownTotal;
            shockwaveFill.fillAmount = 1f - (remaining / total);
            shockwaveKey.color = remaining <= 0 ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        }

        if (dash != null && dashFill != null)
        {
            float remaining = dash.CooldownRemaining;
            float total = dash.CooldownTotal;
            dashFill.fillAmount = 1f - (remaining / total);
            dashKey.color = remaining <= 0 ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        }

        if (bolt != null && boltFill != null)
        {
            float remaining = bolt.CooldownRemaining;
            float total = bolt.CooldownTotal;
            boltFill.fillAmount = 1f - (remaining / total);
            boltKey.color = remaining <= 0 ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        }
    }
}
