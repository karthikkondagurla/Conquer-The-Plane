using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;

public class MainMenuSetup : EditorWindow
{
    [MenuItem("Tools/Conquer The Plane/Create Main Menu")]
    public static void CreateMainMenu()
    {
        string scenePath = "Assets/Scenes/MainMenu.unity";

        // 1. Create or Open Scene
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // Ensure Scenes directory exists
        if (!Directory.Exists("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        // 2. Setup Camera and Light
        GameObject cameraObj = new GameObject("Main Camera");
        Camera cam = cameraObj.AddComponent<Camera>();
        cameraObj.tag = "MainCamera";
        cameraObj.transform.position = new Vector3(0, 1, -10);
        cameraObj.AddComponent<AudioListener>();

        // 3. Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>(); // Default scaler
        canvasObj.AddComponent<GraphicRaycaster>();

        // 4. Create EventSystem (Critical for UI interaction)
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // 5. Create Panel
        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f); // Semi-transparent black
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // 6. Create Manager Script
        GameObject managerObj = new GameObject("MainMenuManager");
        MainMenu managerScript = managerObj.AddComponent<MainMenu>();

        // 7. Create Buttons
        CreateButton("ResumeButton", "Resume", 50, panelObj.transform, managerScript, "ResumeGame");
        CreateButton("NewGameButton", "New Game", -50, panelObj.transform, managerScript, "NewGame");
        CreateButton("QuitButton", "Quit", -150, panelObj.transform, managerScript, "QuitGame");

        // 8. Save Scene
        EditorSceneManager.SaveScene(scene, scenePath);

        // 9. Add to Build Settings
        AddSceneToBuildSettings(scenePath, 0);

        // Ensure GameScene is also in build settings (placeholder check)
        // We do not overwrite index 1 blindly, but make sure it exists if possible.
        
        Debug.Log("Main Menu created successfully at " + scenePath);
        EditorUtility.DisplayDialog("Success", "Main Menu Scene Created!\n\nButtons are wired to load 'Bootstrap'. Ensure the 'Bootstrap' scene is in Build Settings at Index 1 (or later).", "OK");
    }

    private static void CreateButton(string objName, string labelText, float yOffset, Transform parent, MainMenu targetScript, string methodName)
    {
        // Standard UI Button
        GameObject buttonObj = new GameObject(objName);
        buttonObj.transform.SetParent(parent, false);
        
        Image buttonImage = buttonObj.AddComponent<Image>(); // Background
        Button button = buttonObj.AddComponent<Button>();

        // RectTransform
        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 80); // Large touch target
        rect.anchoredPosition = new Vector2(0, yOffset);

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text textComp = textObj.AddComponent<Text>();
        textComp.text = labelText;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Fallback
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.color = Color.black;
        textComp.fontSize = 32;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        // Wiring
        // In editor scripts, we can use UnityEventTools to make persistent connections
        // Note: UnityEventTools requires method info.
        
        // Alternative: Runtime binding isn't what we want for Editor creation.
        // We use persistent listener.
        
        UnityAction action = null;
        switch (methodName)
        {
            case "ResumeGame": action = new UnityAction(targetScript.ResumeGame); break;
            case "NewGame": action = new UnityAction(targetScript.NewGame); break;
            case "QuitGame": action = new UnityAction(targetScript.QuitGame); break;
        }

        if (action != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, action);
        }
    }

    private static void AddSceneToBuildSettings(string scenePath, int index)
    {
        EditorBuildSettingsScene[] originalScenes = EditorBuildSettings.scenes;
        EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[originalScenes.Length + 1];

        bool exists = false;
        for (int i = 0; i < originalScenes.Length; i++)
        {
            if (originalScenes[i].path == scenePath)
            {
                exists = true;
                break;
            }
        }

        if (!exists)
        {
            // If we want to force it to index 0, we need to shift.
            // For simplicity, we just add it to the list and let user organize if needed, 
            // OR strictly overwrite if empty.
            
            // Re-fetch clean list logic:
            System.Collections.Generic.List<EditorBuildSettingsScene> scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            
            // Remove if exists to re-add at 0
            scenes.RemoveAll(s => s.path == scenePath);
            
            // Insert at 0
            scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
            
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
