using UnityEngine;

public class PersistentCamera : MonoBehaviour
{
    public static PersistentCamera Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If a persistent camera already exists, destroy this new one to avoid duplicates
            Destroy(gameObject);
        }
    }
}
