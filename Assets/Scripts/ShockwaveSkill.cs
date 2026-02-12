using UnityEngine;

public class ShockwaveSkill : MonoBehaviour
{
    [Header("Shockwave Settings")]
    public float cooldown = 5f;
    public float radius = 8f;
    public float pushForce = 30f;
    public float damage = 40f;
    public float upwardModifier = 0.4f;

    private float lastUseTime = -99f;

    // Public property for UI
    public float CooldownRemaining => Mathf.Max(0f, cooldown - (Time.time - lastUseTime));
    public float CooldownTotal => cooldown;

    void Start()
    {
        if (DifficultyConfig.Instance != null)
            cooldown = DifficultyConfig.Instance.ShockwaveCooldown;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryShockwave();
        }
    }

    void TryShockwave()
    {
        if (Time.time - lastUseTime < cooldown)
        {
            Debug.Log($"Shockwave on cooldown! {CooldownRemaining:F1}s remaining");
            return;
        }

        lastUseTime = Time.time;

        // Find all colliders in radius
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        int enemiesHit = 0;
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy") || hit.gameObject.name.Contains("Enemy"))
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Explosive force pushes enemies away and upward
                    rb.AddExplosionForce(pushForce, transform.position, radius, upwardModifier, ForceMode.Impulse);
                }
                enemiesHit++;
            }
        }

        // Visual effect — expanding ring
        StartCoroutine(ShockwaveVisual());

        Debug.Log($"🌊 SHOCKWAVE! Hit {enemiesHit} enemies in {radius}m radius!");
    }

    private System.Collections.IEnumerator ShockwaveVisual()
    {
        // Create expanding ring effect
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "ShockwaveEffect";
        Object.Destroy(ring.GetComponent<Collider>());
        ring.transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);

        // Emissive cyan material
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");
        Material mat = new Material(litShader);
        Color shockColor = new Color(0f, 0.9f, 1f);
        mat.SetColor("_BaseColor", new Color(shockColor.r, shockColor.g, shockColor.b, 0.5f));
        mat.color = new Color(shockColor.r, shockColor.g, shockColor.b, 0.5f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", shockColor * 3f);
        // Enable transparency
        mat.SetFloat("_Surface", 1); // Transparent
        mat.SetFloat("_Blend", 0);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        ring.GetComponent<Renderer>().sharedMaterial = mat;

        // Point light flash
        GameObject lightObj = new GameObject("ShockwaveLight");
        lightObj.transform.position = transform.position;
        Light flash = lightObj.AddComponent<Light>();
        flash.type = LightType.Point;
        flash.color = shockColor;
        flash.range = radius * 1.5f;
        flash.intensity = 8f;

        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Expand ring
            float currentRadius = Mathf.Lerp(0.5f, radius, t);
            ring.transform.localScale = new Vector3(currentRadius * 2f, 0.05f, currentRadius * 2f);

            // Fade out
            Color c = mat.color;
            c.a = Mathf.Lerp(0.6f, 0f, t);
            mat.color = c;
            mat.SetColor("_BaseColor", c);

            // Fade light
            flash.intensity = Mathf.Lerp(8f, 0f, t);

            yield return null;
        }

        Object.Destroy(ring);
        Object.Destroy(lightObj);
    }
}
