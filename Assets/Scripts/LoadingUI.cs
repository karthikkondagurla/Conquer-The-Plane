using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadingUI : MonoBehaviour
{
    public static LoadingUI Instance { get; private set; }
    
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.3f;

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
        GameObject canvasObj = new GameObject("LoadingCanvas");
        DontDestroyOnLoad(canvasObj);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = Color.black;
        
        RectTransform rt = imageObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
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
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeImage.color = Color.black;
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
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 0);
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

}
