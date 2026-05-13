using UnityEngine;

public class PlayerPersistent : MonoBehaviour
{
    public static PlayerPersistent Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSkills();
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Ensure frame rate cap as requested
        Application.targetFrameRate = 30;
    }

    /// <summary>
    /// Ensures all player skills are attached at runtime.
    /// This guarantees skills work even on scenes that weren't regenerated.
    /// </summary>
    private void EnsureSkills()
    {
        if (GetComponent<PlayerHealth>() == null) gameObject.AddComponent<PlayerHealth>();
        if (GetComponent<SpikeSkill>() == null) gameObject.AddComponent<SpikeSkill>();
        if (GetComponent<ShockwaveSkill>() == null) gameObject.AddComponent<ShockwaveSkill>();
        if (GetComponent<DashStrikeSkill>() == null) gameObject.AddComponent<DashStrikeSkill>();
        if (GetComponent<EnergyBoltSkill>() == null) gameObject.AddComponent<EnergyBoltSkill>();
        if (GetComponent<BallMovement>() == null) gameObject.AddComponent<BallMovement>();

        // Fix for primitive player not rendering in standalone builds
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");
            if (litShader == null) litShader = Shader.Find("Diffuse");

            if (litShader != null)
            {
                Material mat = new Material(litShader);
                Color playerColor = new Color(0.1f, 0.6f, 1.0f); // Bright blue
                mat.color = playerColor;
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", playerColor);
                }
                renderer.material = mat;
            }
        }
    }
}
