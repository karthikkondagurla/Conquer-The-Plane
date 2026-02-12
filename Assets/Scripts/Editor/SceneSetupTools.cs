using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using System.IO;
using System.Collections.Generic;

public class SceneSetupTools : EditorWindow
{
    // Helper: find the correct Lit shader for URP or Built-in pipeline
    private static Shader GetLitShader()
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s != null) return s;
        s = Shader.Find("Standard");
        if (s != null) return s;
        return Shader.Find("Diffuse");
    }

    private static Material CreateLitMaterial(Color color)
    {
        Material mat = new Material(GetLitShader());
        mat.SetColor("_BaseColor", color); // URP uses _BaseColor
        mat.color = color; // Fallback for built-in
        return mat;
    }
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
        string[] sceneNames = { "Bootstrap", "Map1", "Map2", "Map3", "Map4" }; // "MainMenu" removed
        EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[sceneNames.Length];

        // Enemy distribution is now handled dynamically by EnemyManager

        for (int i = 0; i < sceneNames.Length; i++)
        {
            string scenePath = $"{sceneDir}/{sceneNames[i]}.unity";
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Add Basic Lighting (warm, well-lit like reference)
            Light light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
            light.color = new Color(1f, 0.92f, 0.8f); // Warm white
            light.intensity = 1.1f; // Well-lit surfaces
            light.shadows = LightShadows.Soft;

            // Add Camera (Only for Maps, Bootstrap has special setup)
            if (sceneNames[i].StartsWith("Map"))
            {
                // Maps need spawn points and wormholes
                // sceneNames: 0=MainMenu, 1=Bootstrap, 2=Map1
                // We need enemyDist[0] for Map1. So index is i - 2.
                SetupMapEnvironment(sceneNames[i]);
            }
            else if (sceneNames[i] == "MainMenu")
            {
                // CreateMainMenu();
                Debug.Log("MainMenu scene creation skipped (deleted by user request).");
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

        // 2. Create Map Status UI
        GameObject mapStatusGO = new GameObject("MapStatusUI");
        mapStatusGO.AddComponent<MapStatusUI>();

        // 3. Create Health Bar UI
        GameObject healthBarGO = new GameObject("HealthBarUI");
        healthBarGO.AddComponent<HealthBarUI>();

        // 4. Create Game Over UI
        GameObject gameOverGO = new GameObject("GameOverUI");
        gameOverGO.AddComponent<GameOverUI>();

        // 5. Create Game Win Manager
        new GameObject("GameWinManager").AddComponent<GameWinManager>();

        // 6. Create Victory UI
        new GameObject("VictoryUI").AddComponent<VictoryUI>();

        // 5. Create Ranking UI
        // CreateRankingUI(); // Removed by user request

        // 6. Create Health UI (Top Right + Game Over)
        // CreateHealthUI(); // Removed by user request

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
        player.AddComponent<SpikeSkill>();   // Add Spike Skill (K to plant crystals)
        
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
        MapTheme defaultTheme = GetMapTheme("default");
        CreateWormhole(new Vector3(3, 0, 3), defaultTheme);

        // Walls (Size 10 for Bootstrap)
        CreateWalls(10f, new Color(0.4f, 0.4f, 0.4f), new Color(0f, 1.5f, 2f));
    }

    // ===================== MAP THEMES (FUTURISTIC) =====================
    private struct MapTheme
    {
        public Color floorColor;
        public Color wallColor;
        public Color neonColor;      // Primary neon accent
        public Color ambientColor;
        public Color fogColor;
        public Color crystalEmission;
        public Color glassTint;      // Glass shader tint
        public Color fresnelColor;   // Glass fresnel glow
        public float fogDensity;
        public string themeName;
    }

    private static MapTheme GetMapTheme(string mapName)
    {
        MapTheme theme = new MapTheme();
        switch (mapName)
        {
            case "Map1": // Warm Industrial
                theme.themeName = "Warm Industrial";
                theme.floorColor = new Color(0.35f, 0.28f, 0.2f);    // Warm brown
                theme.wallColor = new Color(0.5f, 0.45f, 0.38f);     // Light warm grey
                theme.neonColor = new Color(0f, 2f, 1.5f);           // Teal accent
                theme.ambientColor = new Color(0.15f, 0.12f, 0.08f);
                theme.fogColor = new Color(0.12f, 0.1f, 0.07f);
                theme.crystalEmission = new Color(0f, 3f, 2f);
                theme.glassTint = new Color(0.3f, 0.9f, 0.8f, 0.25f);
                theme.fresnelColor = new Color(0f, 1f, 0.8f, 1f);
                theme.fogDensity = 0.012f;
                break;
            case "Map2": // Amber Workshop
                theme.themeName = "Amber Workshop";
                theme.floorColor = new Color(0.4f, 0.3f, 0.18f);     // Warm tan
                theme.wallColor = new Color(0.55f, 0.42f, 0.28f);    // Sandy beige
                theme.neonColor = new Color(2f, 0.4f, 1.5f);         // Magenta accent
                theme.ambientColor = new Color(0.18f, 0.12f, 0.06f);
                theme.fogColor = new Color(0.14f, 0.1f, 0.06f);
                theme.crystalEmission = new Color(3f, 0.4f, 2f);
                theme.glassTint = new Color(0.9f, 0.3f, 0.8f, 0.25f);
                theme.fresnelColor = new Color(1f, 0.1f, 0.8f, 1f);
                theme.fogDensity = 0.01f;
                break;
            case "Map3": // Steel Lab
                theme.themeName = "Steel Lab";
                theme.floorColor = new Color(0.28f, 0.3f, 0.35f);    // Cool steel grey
                theme.wallColor = new Color(0.4f, 0.42f, 0.48f);     // Light slate
                theme.neonColor = new Color(0.3f, 0.5f, 3f);         // Blue accent
                theme.ambientColor = new Color(0.08f, 0.1f, 0.15f);
                theme.fogColor = new Color(0.06f, 0.08f, 0.12f);
                theme.crystalEmission = new Color(0.5f, 0.8f, 4f);
                theme.glassTint = new Color(0.4f, 0.6f, 1f, 0.25f);
                theme.fresnelColor = new Color(0.2f, 0.5f, 1f, 1f);
                theme.fogDensity = 0.01f;
                break;
            case "Map4": // Forge
                theme.themeName = "Forge";
                theme.floorColor = new Color(0.3f, 0.2f, 0.15f);     // Dark warm stone
                theme.wallColor = new Color(0.5f, 0.32f, 0.2f);      // Terracotta
                theme.neonColor = new Color(3f, 1f, 0f);             // Orange accent
                theme.ambientColor = new Color(0.12f, 0.06f, 0.03f);
                theme.fogColor = new Color(0.1f, 0.05f, 0.02f);
                theme.crystalEmission = new Color(4f, 1.5f, 0f);
                theme.glassTint = new Color(1f, 0.6f, 0.2f, 0.25f);
                theme.fresnelColor = new Color(1f, 0.4f, 0f, 1f);
                theme.fogDensity = 0.015f;
                break;
            default:
                theme.themeName = "Default";
                theme.floorColor = new Color(0.3f, 0.28f, 0.25f);    // Warm grey
                theme.wallColor = new Color(0.45f, 0.42f, 0.38f);
                theme.neonColor = new Color(0f, 1.5f, 2f);
                theme.ambientColor = new Color(0.1f, 0.08f, 0.06f);
                theme.fogColor = new Color(0.08f, 0.07f, 0.05f);
                theme.crystalEmission = new Color(0f, 2f, 3f);
                theme.glassTint = new Color(0.5f, 0.8f, 1f, 0.25f);
                theme.fresnelColor = new Color(0f, 0.8f, 1f, 1f);
                theme.fogDensity = 0.01f;
                break;
        }
        return theme;
    }

    // ===================== MAP SETUP =====================
    private static void SetupMapEnvironment(string mapName)
    {
        MapTheme theme = GetMapTheme(mapName);

        // === Lighting & Atmosphere ===
        SetupMapLighting(theme);

        // === Ground (dark metallic) ===
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Ground_" + mapName;
        floor.transform.localScale = new Vector3(2.5f, 1, 2.5f);
        
        Renderer ren = floor.GetComponent<Renderer>();
        Material floorMat = CreateLitMaterial(theme.floorColor);
        floorMat.SetFloat("_Smoothness", 0.7f); // Reflective metallic floor
        floorMat.SetFloat("_Metallic", 0.6f);
        ren.sharedMaterial = floorMat;

        // === SpawnPoints ===
        int spawnCount = Random.Range(3, 6);
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject spawn = new GameObject("SpawnPoint_" + i);
            spawn.tag = "SpawnPoint";
            spawn.transform.position = new Vector3(Random.Range(-10f, 10f), 0.5f, Random.Range(-10f, 10f));
        }

        // === Wormholes (3-5), with glass shader ===
        int wormholeCount = Random.Range(3, 6);
        for (int k = 0; k < wormholeCount; k++)
        {
            Vector3 pos = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
            CreateWormhole(pos, theme);
        }

        // === Walls with neon trim ===
        CreateWalls(25f, theme.wallColor, theme.neonColor);

        // === Decorations ===
        // Corner Pillars with neon accent
        CreateCornerPillars(theme.wallColor, theme.neonColor);

        // Glowing Neon Crystals (4-6 scattered)
        int crystalCount = Random.Range(4, 7);
        for (int c = 0; c < crystalCount; c++)
        {
            Vector3 pos = new Vector3(Random.Range(-9f, 9f), 0, Random.Range(-9f, 9f));
            CreateCrystal(pos, theme.crystalEmission);
        }

        // Scattered obstacles (4-6)
        int obstacleCount = Random.Range(4, 7);
        for (int o = 0; o < obstacleCount; o++)
        {
            Vector3 pos = new Vector3(Random.Range(-9f, 9f), 0, Random.Range(-9f, 9f));
            CreateObstacle(pos, theme.wallColor);
        }

        // === Neon floor edge strips ===
        CreateNeonFloorEdges(theme.neonColor);

        // === Warm Point Lights (for glass shader illumination) ===
        CreateScenePointLights(theme.neonColor);
    }

    // ===================== LIGHTING =====================
    private static void SetupMapLighting(MapTheme theme)
    {
        // Ambient light (low for dark feel)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = theme.ambientColor;

        // Fog (subtle, dark)
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = theme.fogColor;
        RenderSettings.fogDensity = theme.fogDensity;

        // Dark sky
        RenderSettings.ambientSkyColor = theme.fogColor;

        // Enable Opaque Texture on URP pipeline for glass shader
        var urpAsset = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset != null)
        {
            urpAsset.supportsCameraOpaqueTexture = true;
        }
    }

    // ===================== WORMHOLE (PORTAL GATE) =====================
    private static void CreateWormhole(Vector3 position, MapTheme theme)
    {
        float portalWidth = 1.8f;
        float portalHeight = 2.5f;
        float frameThickness = 0.15f;

        // Parent object with trigger collider
        GameObject portal = new GameObject("Wormhole");
        portal.transform.position = new Vector3(position.x, portalHeight / 2f, position.z);
        BoxCollider trigger = portal.AddComponent<BoxCollider>();
        trigger.size = new Vector3(portalWidth, portalHeight, 0.5f);
        trigger.isTrigger = true;
        portal.AddComponent<Wormhole>();

        // === Dark portal surface (flat quad) ===
        GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Quad);
        surface.name = "PortalSurface";
        surface.transform.SetParent(portal.transform);
        surface.transform.localPosition = Vector3.zero;
        surface.transform.localScale = new Vector3(portalWidth, portalHeight, 1f);
        Object.DestroyImmediate(surface.GetComponent<Collider>());
        Material surfaceMat = CreateLitMaterial(new Color(0.02f, 0.02f, 0.02f)); // Near black
        surfaceMat.SetFloat("_Smoothness", 0.9f);
        surfaceMat.SetFloat("_Metallic", 1f);
        surface.GetComponent<Renderer>().sharedMaterial = surfaceMat;

        // === Neon frame material ===
        Material frameMat = CreateLitMaterial(Color.black);
        frameMat.EnableKeyword("_EMISSION");
        frameMat.SetColor("_EmissionColor", theme.fresnelColor * 3f);
        frameMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        float halfW = portalWidth / 2f;
        float halfH = portalHeight / 2f;

        // Left bar
        GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
        left.name = "Frame_Left";
        left.transform.SetParent(portal.transform);
        left.transform.localPosition = new Vector3(-halfW, 0, 0);
        left.transform.localScale = new Vector3(frameThickness, portalHeight + frameThickness, frameThickness);
        Object.DestroyImmediate(left.GetComponent<Collider>());
        left.GetComponent<Renderer>().sharedMaterial = frameMat;

        // Right bar
        GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
        right.name = "Frame_Right";
        right.transform.SetParent(portal.transform);
        right.transform.localPosition = new Vector3(halfW, 0, 0);
        right.transform.localScale = new Vector3(frameThickness, portalHeight + frameThickness, frameThickness);
        Object.DestroyImmediate(right.GetComponent<Collider>());
        right.GetComponent<Renderer>().sharedMaterial = frameMat;

        // Top bar
        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
        top.name = "Frame_Top";
        top.transform.SetParent(portal.transform);
        top.transform.localPosition = new Vector3(0, halfH, 0);
        top.transform.localScale = new Vector3(portalWidth + frameThickness, frameThickness, frameThickness);
        Object.DestroyImmediate(top.GetComponent<Collider>());
        top.GetComponent<Renderer>().sharedMaterial = frameMat;

        // Bottom bar
        GameObject bottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bottom.name = "Frame_Bottom";
        bottom.transform.SetParent(portal.transform);
        bottom.transform.localPosition = new Vector3(0, -halfH, 0);
        bottom.transform.localScale = new Vector3(portalWidth + frameThickness, frameThickness, frameThickness);
        Object.DestroyImmediate(bottom.GetComponent<Collider>());
        bottom.GetComponent<Renderer>().sharedMaterial = frameMat;

        // Point light for glow
        GameObject glow = new GameObject("PortalGlow");
        glow.transform.SetParent(portal.transform);
        glow.transform.localPosition = new Vector3(0, 0, -0.5f);
        Light gl = glow.AddComponent<Light>();
        gl.type = LightType.Point;
        gl.color = theme.fresnelColor;
        gl.range = 4f;
        gl.intensity = 1.5f;
    }

    // ===================== WALLS =====================
    private static void CreateWalls(float size, Color wallColor, Color neonColor)
    {
        float offset = size / 2.0f + 0.5f;
        float height = 1.5f;
        GameObject wallParent = new GameObject("Walls");

        Material wallMat = CreateLitMaterial(wallColor);
        wallMat.SetFloat("_Smoothness", 0.4f);
        wallMat.SetFloat("_Metallic", 0.3f);

        // Neon trim material
        Material trimMat = CreateLitMaterial(Color.black);
        trimMat.EnableKeyword("_EMISSION");
        trimMat.SetColor("_EmissionColor", neonColor);
        trimMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        // North
        GameObject n = GameObject.CreatePrimitive(PrimitiveType.Cube);
        n.name = "Wall_North";
        n.transform.SetParent(wallParent.transform);
        n.transform.position = new Vector3(0, height / 2, offset);
        n.transform.localScale = new Vector3(size + 2, height, 1);
        n.GetComponent<Renderer>().sharedMaterial = wallMat;

        // South
        GameObject s = GameObject.CreatePrimitive(PrimitiveType.Cube);
        s.name = "Wall_South";
        s.transform.SetParent(wallParent.transform);
        s.transform.position = new Vector3(0, height / 2, -offset);
        s.transform.localScale = new Vector3(size + 2, height, 1);
        s.GetComponent<Renderer>().sharedMaterial = wallMat;

        // East
        GameObject e = GameObject.CreatePrimitive(PrimitiveType.Cube);
        e.name = "Wall_East";
        e.transform.SetParent(wallParent.transform);
        e.transform.position = new Vector3(offset, height / 2, 0);
        e.transform.localScale = new Vector3(1, height, size);
        e.GetComponent<Renderer>().sharedMaterial = wallMat;

        // West
        GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cube);
        w.name = "Wall_West";
        w.transform.SetParent(wallParent.transform);
        w.transform.position = new Vector3(-offset, height / 2, 0);
        w.transform.localScale = new Vector3(1, height, size);
        w.GetComponent<Renderer>().sharedMaterial = wallMat;

        // === Neon Trim Strips (along the top of each wall) ===
        float trimHeight = 0.1f;
        // North trim
        GameObject nt = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nt.name = "NeonTrim_North";
        nt.transform.SetParent(wallParent.transform);
        nt.transform.position = new Vector3(0, height + trimHeight/2, offset);
        nt.transform.localScale = new Vector3(size + 2, trimHeight, 1.05f);
        nt.GetComponent<Renderer>().sharedMaterial = trimMat;
        Object.DestroyImmediate(nt.GetComponent<Collider>());

        // South trim
        GameObject st = GameObject.CreatePrimitive(PrimitiveType.Cube);
        st.name = "NeonTrim_South";
        st.transform.SetParent(wallParent.transform);
        st.transform.position = new Vector3(0, height + trimHeight/2, -offset);
        st.transform.localScale = new Vector3(size + 2, trimHeight, 1.05f);
        st.GetComponent<Renderer>().sharedMaterial = trimMat;
        Object.DestroyImmediate(st.GetComponent<Collider>());

        // East trim
        GameObject et = GameObject.CreatePrimitive(PrimitiveType.Cube);
        et.name = "NeonTrim_East";
        et.transform.SetParent(wallParent.transform);
        et.transform.position = new Vector3(offset, height + trimHeight/2, 0);
        et.transform.localScale = new Vector3(1.05f, trimHeight, size);
        et.GetComponent<Renderer>().sharedMaterial = trimMat;
        Object.DestroyImmediate(et.GetComponent<Collider>());

        // West trim
        GameObject wt = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wt.name = "NeonTrim_West";
        wt.transform.SetParent(wallParent.transform);
        wt.transform.position = new Vector3(-offset, height + trimHeight/2, 0);
        wt.transform.localScale = new Vector3(1.05f, trimHeight, size);
        wt.GetComponent<Renderer>().sharedMaterial = trimMat;
        Object.DestroyImmediate(wt.GetComponent<Collider>());
    }

    // ===================== DECORATIONS =====================
    private static void CreateCornerPillars(Color pillarColor, Color neonColor)
    {
        float offset = 11f;
        float height = 5f; // Taller futuristic pillars
        Material pillarMat = CreateLitMaterial(pillarColor);
        pillarMat.SetFloat("_Smoothness", 0.6f);
        pillarMat.SetFloat("_Metallic", 0.5f);

        // Neon top material
        Material neonMat = CreateLitMaterial(Color.black);
        neonMat.EnableKeyword("_EMISSION");
        neonMat.SetColor("_EmissionColor", neonColor * 1.5f);
        neonMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        Vector3[] corners = {
            new Vector3(offset, height / 2, offset),
            new Vector3(-offset, height / 2, offset),
            new Vector3(offset, height / 2, -offset),
            new Vector3(-offset, height / 2, -offset)
        };

        GameObject pillarParent = new GameObject("Pillars");
        for (int i = 0; i < corners.Length; i++)
        {
            // Main pillar body
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Pillar_" + i;
            pillar.transform.SetParent(pillarParent.transform);
            pillar.transform.position = corners[i];
            pillar.transform.localScale = new Vector3(0.8f, height / 2, 0.8f);
            pillar.GetComponent<Renderer>().sharedMaterial = pillarMat;

            // Glowing neon ring at top
            GameObject neonRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            neonRing.name = "PillarNeon_" + i;
            neonRing.transform.SetParent(pillar.transform);
            neonRing.transform.localPosition = new Vector3(0, 0.95f, 0);
            neonRing.transform.localScale = new Vector3(1.5f, 0.05f, 1.5f);
            neonRing.GetComponent<Renderer>().sharedMaterial = neonMat;
            Object.DestroyImmediate(neonRing.GetComponent<Collider>());

            // Neon ring at base
            GameObject baseRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseRing.name = "PillarBase_" + i;
            baseRing.transform.SetParent(pillar.transform);
            baseRing.transform.localPosition = new Vector3(0, -0.95f, 0);
            baseRing.transform.localScale = new Vector3(1.3f, 0.05f, 1.3f);
            baseRing.GetComponent<Renderer>().sharedMaterial = neonMat;
            Object.DestroyImmediate(baseRing.GetComponent<Collider>());
        }
    }

    private static void CreateNeonFloorEdges(Color neonColor)
    {
        float edgeSize = 12f; // floor edge position
        float stripWidth = 0.15f;
        float stripY = 0.02f; // Just above floor

        Material edgeMat = CreateLitMaterial(Color.black);
        edgeMat.EnableKeyword("_EMISSION");
        edgeMat.SetColor("_EmissionColor", neonColor * 0.8f);
        edgeMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        GameObject edgeParent = new GameObject("NeonFloorEdges");

        // 4 edge strips
        string[] dirs = { "North", "South", "East", "West" };
        Vector3[] positions = {
            new Vector3(0, stripY, edgeSize),
            new Vector3(0, stripY, -edgeSize),
            new Vector3(edgeSize, stripY, 0),
            new Vector3(-edgeSize, stripY, 0)
        };
        Vector3[] scales = {
            new Vector3(edgeSize * 2, stripWidth, stripWidth),
            new Vector3(edgeSize * 2, stripWidth, stripWidth),
            new Vector3(stripWidth, stripWidth, edgeSize * 2),
            new Vector3(stripWidth, stripWidth, edgeSize * 2)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = "NeonEdge_" + dirs[i];
            strip.transform.SetParent(edgeParent.transform);
            strip.transform.position = positions[i];
            strip.transform.localScale = scales[i];
            strip.GetComponent<Renderer>().sharedMaterial = edgeMat;
            Object.DestroyImmediate(strip.GetComponent<Collider>());
        }
    }

    // ===================== POINT LIGHTS (for glass effect) =====================
    private static void CreateScenePointLights(Color accentColor)
    {
        GameObject lightParent = new GameObject("PointLights");

        // Warm orange lights (like the reference video)
        Color[] lightColors = {
            new Color(1f, 0.6f, 0.2f),    // Warm orange
            new Color(1f, 0.9f, 0.7f),    // Warm white
            accentColor * 0.3f,            // Theme accent (dimmed)
            new Color(1f, 0.3f, 0.1f),    // Deep orange
            new Color(0.9f, 0.8f, 0.5f),  // Golden
        };

        Vector3[] lightPositions = {
            new Vector3(-5f, 2.5f, 5f),
            new Vector3(6f, 3f, -4f),
            new Vector3(0f, 4f, 0f),
            new Vector3(-7f, 1.5f, -6f),
            new Vector3(8f, 2f, 7f),
        };

        float[] lightRanges = { 8f, 10f, 12f, 7f, 9f };
        float[] lightIntensities = { 2f, 1.5f, 1.8f, 2.5f, 1.2f };

        for (int i = 0; i < lightColors.Length; i++)
        {
            // Point light
            GameObject lightObj = new GameObject("PointLight_" + i);
            lightObj.transform.SetParent(lightParent.transform);
            lightObj.transform.position = lightPositions[i];
            Light pl = lightObj.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.color = lightColors[i];
            pl.range = lightRanges[i];
            pl.intensity = lightIntensities[i];
            pl.shadows = LightShadows.Soft;

            // Visible emissive orb at light position
            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "LightOrb_" + i;
            orb.transform.SetParent(lightObj.transform);
            orb.transform.localPosition = Vector3.zero;
            orb.transform.localScale = Vector3.one * 0.25f;
            Object.DestroyImmediate(orb.GetComponent<Collider>());
            Material orbMat = CreateLitMaterial(Color.black);
            orbMat.EnableKeyword("_EMISSION");
            orbMat.SetColor("_EmissionColor", lightColors[i] * 3f);
            orbMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            orb.GetComponent<Renderer>().sharedMaterial = orbMat;
        }
    }

    private static void CreateCrystal(Vector3 position, Color emissionColor)
    {
        // Tall diamond-like shape using a stretched cube rotated 45 degrees
        GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crystal.name = "Crystal";
        crystal.transform.position = new Vector3(position.x, 1f, position.z);
        crystal.transform.localScale = new Vector3(0.4f, 1.5f, 0.4f);
        crystal.transform.rotation = Quaternion.Euler(0, 45, 0); // Rotated for visual interest

        Material crystalMat = CreateLitMaterial(emissionColor * 0.5f);
        crystalMat.EnableKeyword("_EMISSION");
        crystalMat.SetColor("_EmissionColor", emissionColor * 0.8f);
        crystalMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        crystalMat.SetFloat("_Smoothness", 0.85f);
        crystalMat.SetFloat("_Metallic", 0.3f);
        crystal.GetComponent<Renderer>().sharedMaterial = crystalMat;
    }

    private static void CreateObstacle(Vector3 position, Color baseColor)
    {
        // Random obstacle type
        int type = Random.Range(0, 3);
        GameObject obstacle;

        switch (type)
        {
            case 0: // Box
                obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obstacle.name = "Obstacle_Box";
                float boxSize = Random.Range(0.8f, 1.8f);
                obstacle.transform.localScale = new Vector3(boxSize, boxSize * 0.6f, boxSize);
                obstacle.transform.position = new Vector3(position.x, boxSize * 0.3f, position.z);
                break;
            case 1: // Ramp
                obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obstacle.name = "Obstacle_Ramp";
                obstacle.transform.localScale = new Vector3(2f, 0.5f, 3f);
                obstacle.transform.position = new Vector3(position.x, 0.25f, position.z);
                obstacle.transform.rotation = Quaternion.Euler(15, Random.Range(0f, 360f), 0);
                break;
            default: // Cylinder
                obstacle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                obstacle.name = "Obstacle_Barrel";
                float radius = Random.Range(0.4f, 0.8f);
                obstacle.transform.localScale = new Vector3(radius, 0.5f, radius);
                obstacle.transform.position = new Vector3(position.x, 0.5f, position.z);
                break;
        }

        Material obsMat = CreateLitMaterial(baseColor * 0.8f + Color.grey * 0.2f);
        obsMat.SetFloat("_Smoothness", 0.2f);
        obstacle.GetComponent<Renderer>().sharedMaterial = obsMat;
    }

    // ===================== UTILITIES =====================
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
}
