using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class SceneSetupTools : EditorWindow
{
    [MenuItem("Tools/Setup Wormhole Test")]
    public static void SetupWormholeTest()
    {
        EnsureTagExists("SpawnPoint");
        EnsureTagExists("Enemy");

        string sceneDir = "Assets/Scenes";
        if (!Directory.Exists(sceneDir))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        // 1. Create Scenes
        string[] sceneNames = { "MainMenu", "Bootstrap", "Map1", "Map2", "Map3", "Map4" }; // Added MainMenu
        EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[sceneNames.Length];

        // Pre-calculate random enemy distribution (Total 9)
        int[] enemyDist = new int[4];
        for (int k = 0; k < 9; k++)
        {
            enemyDist[Random.Range(0, 4)]++;
        }

        for (int i = 0; i < sceneNames.Length; i++)
        {
            string scenePath = $"{sceneDir}/{sceneNames[i]}.unity";
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Add Basic Lighting
            Light light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);

            // Add Camera (Only for Maps, Bootstrap has special setup)
            if (sceneNames[i].StartsWith("Map"))
            {
                // Maps need spawn points and wormholes
                // sceneNames: 0=MainMenu, 1=Bootstrap, 2=Map1
                // We need enemyDist[0] for Map1. So index is i - 2.
                SetupMapEnvironment(sceneNames[i], enemyDist[i - 2]);
            }
            else if (sceneNames[i] == "MainMenu")
            {
                CreateMainMenu();
            }
            else if (sceneNames[i] == "Bootstrap")
            {
                // Bootstrap needs Player
                SetupBootstrapEnvironment();
            }

            EditorSceneManager.SaveScene(scene, scenePath);
            buildScenes[i] = new EditorBuildSettingsScene(scenePath, true);
        }

        // 2. Set Build Settings
        EditorBuildSettings.scenes = buildScenes;
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Complete", "Initialized Bootstrap and 4 Map scenes with Build Settings.", "OK");
    }

    private static void SetupBootstrapEnvironment()
    {
        // 0. Create EnemyManager (Singleton)
        new GameObject("EnemyManager").AddComponent<EnemyManager>();

        // 1. Create Loading UI
        // Trigger LoadingUI Awake via creating GO
        GameObject uiGO = new GameObject("LoadingUI");
        uiGO.AddComponent<LoadingUI>();

        // 2. Create Ranking UI
        CreateRankingUI();

        // 3. Create Health UI (Top Right + Game Over)
        CreateHealthUI();

        // 4. Create Player
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        
        // Add Physics Material for rolling (friction)
        PhysicsMaterial ballMat = new PhysicsMaterial();
        ballMat.dynamicFriction = 0.6f;
        ballMat.staticFriction = 0.6f;
        ballMat.bounciness = 0.6f;
        ballMat.frictionCombine = PhysicsMaterialCombine.Average;
        ballMat.bounceCombine = PhysicsMaterialCombine.Average;
        player.GetComponent<Collider>().material = ballMat;
        player.name = "PlayerManager";
        player.tag = "Player";
        player.AddComponent<Rigidbody>().isKinematic = false; 
        player.AddComponent<PlayerPersistent>();
        player.AddComponent<PlayerHealth>(); // Add Health Component
        
        // Add BallMovement
        player.AddComponent<BallMovement>();
        
        GameObject cam = new GameObject("Main Camera");
        cam.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.AddComponent<CameraFollow>().target = player.transform;
        cam.AddComponent<PersistentCamera>();
        
        // Position Camera behind player
        cam.transform.position = player.transform.position + new Vector3(0, 3, -5);
        cam.transform.LookAt(player.transform);

        // Ground
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.transform.position = Vector3.zero;
        player.transform.position = Vector3.up * 0.5f; // Touching ground (Radius 0.5)
        
        // Add a "Start Wormhole" to verify
        CreateWormhole(new Vector3(3, 0, 3));
    }

    private static void SetupMapEnvironment(string mapName, int enemyCount)
    {
        // Ground - Scale up to 25x25 (Scale 2.5) to give more room
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Ground_" + mapName;
        floor.transform.localScale = new Vector3(2.5f, 1, 2.5f);
        
        // Color it differently
        Renderer ren = floor.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.grey;
        ren.sharedMaterial = mat;

        // SpawnPoints (3-5 random locations)
        int spawnCount = Random.Range(3, 6);
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject spawn = new GameObject("SpawnPoint_" + i);
            spawn.tag = "SpawnPoint";
            // Random position on the larger floor (-10 to 10 safe range). Y=0.5 to touch ground.
            spawn.transform.position = new Vector3(Random.Range(-10f, 10f), 0.5f, Random.Range(-10f, 10f));
        }

        // Enemies (Fixed Count passed from Manager)
        int mapID = 0;
        if (mapName.StartsWith("Map")) int.TryParse(mapName.Substring(3), out mapID);

        for (int j = 0; j < enemyCount; j++)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemy.name = "Enemy_" + j;
            enemy.tag = "Enemy"; // Ensure Tag is set
            // Random position
            enemy.transform.position = new Vector3(Random.Range(-10f, 10f), 0.5f, Random.Range(-10f, 10f));
            
            // Visuals
            Renderer r = enemy.GetComponent<Renderer>();
            Material m = new Material(Shader.Find("Standard"));
            m.color = Color.black;
            r.sharedMaterial = m;

            // Logic
            EnemyAI ai = enemy.AddComponent<EnemyAI>();
            ai.originMapID = mapID;
            
            // Physics
            Rigidbody rb = enemy.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation; // Slide, don't roll
        }

        // Wormholes (3-5)
        int count = Random.Range(3, 6);
        for (int k = 0; k < count; k++)
        {
            // Random position on the larger floor (-10 to 10 safe range)
            Vector3 pos = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
            CreateWormhole(pos);
        }
    }

    private static void CreateWormhole(Vector3 position)
    {
        GameObject wh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        wh.name = "Wormhole";
        // Position at y=0.5 (radius) so it sits ON the floor, not in the air
        wh.transform.position = new Vector3(position.x, 0.5f, position.z);
        wh.GetComponent<Collider>().isTrigger = true;
        wh.AddComponent<Wormhole>();
        
        // Visuals: make it look like a portal
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.magenta;
        wh.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static void EnsureTagExists(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(tag)) { found = true; break; }
        }

        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
            n.stringValue = tag;
            tagManager.ApplyModifiedProperties();
        }
    }

    private static void CreateRankingUI()
    {
        // Canvas (No EventSystem needed? LoadingUI probably has one? Add one just in case if persistent)
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            new GameObject("EventSystem").AddComponent<UnityEngine.EventSystems.EventSystem>().gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        if (GameObject.Find("RankingCanvas") != null)
        {
            Debug.Log("RankingCanvas already exists. Skipping creation to preserve manual changes.");
            return;
        }

        GameObject canvasGO = new GameObject("RankingCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Handled in RankingUI.cs Awake()
        // Object.DontDestroyOnLoad(canvasGO); 

        // RankingUI Script
        RankingUI rankingUI = canvasGO.AddComponent<RankingUI>();
        rankingUI.rows = new List<RankingUI.RankingRow>();

        // Container Panel (Top-Left)
        GameObject panelGO = new GameObject("RankingPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(10, -10); // Smaller Padding

        VerticalLayoutGroup vlg = panelGO.AddComponent<VerticalLayoutGroup>();
        vlg.childControlHeight = false; 
        vlg.childControlWidth = false; 
        vlg.spacing = 5; // Smaller spacing
        vlg.padding = new RectOffset(5, 5, 5, 5); // Smaller padding
        
        ContentSizeFitter csf = panelGO.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Map Colors (approximate based on screenshot)
        Color[] mapColors = { 
            new Color(0.1f, 0.4f, 0.5f), // Map 3 (Teal/Blue)
            new Color(0.1f, 0.5f, 0.1f), // Map 4 (Green)
            new Color(0.4f, 0.2f, 0.2f), // Map 1 (Brown)
            new Color(0.5f, 0.3f, 0.1f)  // Map 2 (Orange/Brown)
        };
        
        for (int i = 0; i < 4; i++)
        {
            int mapID = i + 1;
            
            GameObject rowGO = new GameObject($"Row_Map{mapID}");
            rowGO.transform.SetParent(panelGO.transform, false);
            
            Image bg = rowGO.AddComponent<Image>();
            // Use specific color for each Map ID
            if (mapID == 1) bg.color = mapColors[2];
            else if (mapID == 2) bg.color = mapColors[3];
            else if (mapID == 3) bg.color = mapColors[0];
            else if (mapID == 4) bg.color = mapColors[1];
            
            RectTransform rowRect = rowGO.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(160, 35); // Smaller Row Size

            // Row Horizontal Layout
            HorizontalLayoutGroup hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 5, 2, 2); // Compact padding
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false; 
            hlg.childForceExpandWidth = true;
            hlg.spacing = 5;

            // Map Name Text
            GameObject textGO = new GameObject("MapName");
            textGO.transform.SetParent(rowGO.transform, false);
            Text nameText = textGO.AddComponent<Text>();
            nameText.text = $"Map {mapID}";
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 14; // Smaller Font
            nameText.color = new Color(0.9f, 0.9f, 1f); 
            nameText.alignment = TextAnchor.MiddleLeft;
            
            LayoutElement textLe = textGO.AddComponent<LayoutElement>();
            textLe.minWidth = 60; 
            textLe.flexibleWidth = 1; // Pushes content to the right

            // Count Text (Directly in Row)
            GameObject countGO = new GameObject("CountText");
            countGO.transform.SetParent(rowGO.transform, false);
            Text countText = countGO.AddComponent<Text>();
            countText.text = "0";
            countText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            countText.fontSize = 14; 
            countText.color = Color.white; 
            countText.alignment = TextAnchor.MiddleRight;
            
            LayoutElement countLe = countGO.AddComponent<LayoutElement>();
            countLe.minWidth = 30;

            // Add to list
            RankingUI.RankingRow rowData = new RankingUI.RankingRow();
            rowData.mapID = mapID;
            rowData.rowTransform = rowRect;
            rowData.countText = countText;
            rowData.mapNameText = nameText;
            rankingUI.rows.Add(rowData);
            
            // Add Layout Element to Row to ensure height
            LayoutElement rowLe = rowGO.AddComponent<LayoutElement>();
            rowLe.minHeight = 35;
            rowLe.preferredWidth = 160;
        }
    }

    private static void CreateMainMenu()
    {
        // Canvas
        GameObject canvasGO = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // EventSystem
        new GameObject("EventSystem").AddComponent<UnityEngine.EventSystems.EventSystem>().gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // Controller
        MainMenuController controller = canvasGO.AddComponent<MainMenuController>();

        // Background
        GameObject panelGO = new GameObject("BackgroundPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        Image bg = panelGO.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.2f); // Dark Blue
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;

        // Title
        GameObject titleGO = new GameObject("TitleText");
        titleGO.transform.SetParent(panelGO.transform, false);
        Text title = titleGO.AddComponent<Text>();
        title.text = "Wormhole Game";
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 60;
        title.alignment = TextAnchor.MiddleCenter;
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 100);
        titleRect.sizeDelta = new Vector2(500, 100);

        // Play Button
        GameObject playBtnGO = new GameObject("PlayButton");
        playBtnGO.transform.SetParent(panelGO.transform, false);
        Image playImg = playBtnGO.AddComponent<Image>();
        playImg.color = Color.green;
        Button playBtn = playBtnGO.AddComponent<Button>();
        controller.playButton = playBtn;
        RectTransform playRect = playBtnGO.GetComponent<RectTransform>();
        playRect.sizeDelta = new Vector2(200, 50);
        playRect.anchoredPosition = new Vector2(0, 0);
        
        GameObject playTextGO = new GameObject("Text");
        playTextGO.transform.SetParent(playBtnGO.transform, false);
        Text playText = playTextGO.AddComponent<Text>();
        playText.text = "PLAY";
        playText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        playText.fontSize = 24;
        playText.alignment = TextAnchor.MiddleCenter;
        playText.color = Color.black;
        RectTransform playTextRect = playTextGO.GetComponent<RectTransform>();
        playTextRect.anchorMin = Vector2.zero;
        playTextRect.anchorMax = Vector2.one;
        playTextRect.offsetMin = Vector2.zero;
        playTextRect.offsetMax = Vector2.zero;

        // Quit Button
        GameObject quitBtnGO = new GameObject("QuitButton");
        quitBtnGO.transform.SetParent(panelGO.transform, false);
        Image quitImg = quitBtnGO.AddComponent<Image>();
        quitImg.color = Color.red;
        Button quitBtn = quitBtnGO.AddComponent<Button>();
        controller.quitButton = quitBtn;
        RectTransform quitRect = quitBtnGO.GetComponent<RectTransform>();
        quitRect.sizeDelta = new Vector2(200, 50);
        quitRect.anchoredPosition = new Vector2(0, -70);

        GameObject quitTextGO = new GameObject("Text");
        quitTextGO.transform.SetParent(quitBtnGO.transform, false);
        Text quitText = quitTextGO.AddComponent<Text>();
        quitText.text = "QUIT";
        quitText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        quitText.fontSize = 24;
        quitText.alignment = TextAnchor.MiddleCenter;
        quitText.color = Color.black;
        RectTransform quitTextRect = quitTextGO.GetComponent<RectTransform>();
        quitTextRect.anchorMin = Vector2.zero;
        quitTextRect.anchorMax = Vector2.one;
        quitTextRect.offsetMin = Vector2.zero;
        quitTextRect.offsetMax = Vector2.zero;
    }

    private static void CreateHealthUI()
    {
        GameObject canvasGO = GameObject.Find("RankingCanvas"); 
        // We can reuse the Ranking Canvas or create a new one. 
        // Let's create a "HUDCanvas" to keep it separate or just attach to Ranking.
        // RankingCanvas is persistent. That's good.
        
        if (canvasGO == null) return; // Should exist

        HealthUI healthUI = canvasGO.AddComponent<HealthUI>();

        // Health Bar (Top Right)
        GameObject barBgGO = new GameObject("HealthBarBG");
        barBgGO.transform.SetParent(canvasGO.transform, false);
        Image barBg = barBgGO.AddComponent<Image>();
        barBg.color = Color.gray;
        RectTransform barRect = barBgGO.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(1, 1);
        barRect.anchorMax = new Vector2(1, 1);
        barRect.pivot = new Vector2(1, 1);
        barRect.anchoredPosition = new Vector2(-10, -10);
        barRect.sizeDelta = new Vector2(200, 20);

        GameObject barFillGO = new GameObject("HealthBarFill");
        barFillGO.transform.SetParent(barBgGO.transform, false);
        Image barFill = barFillGO.AddComponent<Image>();
        barFill.color = Color.green;
        healthUI.healthBarFill = barFill;
        RectTransform fillRect = barFillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        // Game Over Panel (Centered, Hidden by default)
        GameObject goPanelGO = new GameObject("GameOverPanel");
        goPanelGO.transform.SetParent(canvasGO.transform, false);
        Image goBg = goPanelGO.AddComponent<Image>();
        goBg.color = new Color(0, 0, 0, 0.8f);
        RectTransform goRect = goPanelGO.GetComponent<RectTransform>();
        goRect.anchorMin = Vector2.zero;
        goRect.anchorMax = Vector2.one;
        goRect.offsetMin = Vector2.zero;
        goRect.offsetMax = Vector2.zero;
        healthUI.gameOverPanel = goPanelGO;
        goPanelGO.SetActive(false); // Hide initially

        // Game Over Text
        GameObject goTextGO = new GameObject("GOText");
        goTextGO.transform.SetParent(goPanelGO.transform, false);
        Text goText = goTextGO.AddComponent<Text>();
        goText.text = "GAME OVER";
        goText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        goText.fontSize = 60;
        goText.color = Color.red;
        goText.alignment = TextAnchor.MiddleCenter;
        RectTransform goTextRect = goTextGO.GetComponent<RectTransform>();
        goTextRect.anchoredPosition = new Vector2(0, 50);
        goTextRect.sizeDelta = new Vector2(400, 100);

        // Main Menu Button
        GameObject mmBtnGO = new GameObject("MainMenuButton");
        mmBtnGO.transform.SetParent(goPanelGO.transform, false);
        Image mmBtnImg = mmBtnGO.AddComponent<Image>();
        mmBtnImg.color = Color.white;
        Button mmBtn = mmBtnGO.AddComponent<Button>();
        healthUI.mainMenuButton = mmBtn;
        RectTransform mmBtnRect = mmBtnGO.GetComponent<RectTransform>();
        mmBtnRect.sizeDelta = new Vector2(200, 50);
        mmBtnRect.anchoredPosition = new Vector2(0, -50);

        GameObject mmTextGO = new GameObject("Text");
        mmTextGO.transform.SetParent(mmBtnGO.transform, false);
        Text mmText = mmTextGO.AddComponent<Text>();
        mmText.text = "Main Menu";
        mmText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        mmText.fontSize = 24;
        mmText.color = Color.black;
        mmText.alignment = TextAnchor.MiddleCenter;
        RectTransform mmTextRect = mmTextGO.GetComponent<RectTransform>();
        mmTextRect.anchorMin = Vector2.zero;
        mmTextRect.anchorMax = Vector2.one;
        mmTextRect.offsetMin = Vector2.zero;
        mmTextRect.offsetMax = Vector2.zero;
    }
}
