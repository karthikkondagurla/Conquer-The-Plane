using UnityEngine;

public class DashStrikeSkill : MonoBehaviour
{
    [Header("Dash Settings")]
    public float cooldown = 4f;
    public float dashForce = 40f;
    public float dashDuration = 0.3f;
    public float hitPushForce = 20f;
    public float hitDamage = 35f;
    public float hitRadius = 1.5f;

    private float lastUseTime = -99f;
    private bool isDashing = false;
    private Rigidbody rb;

    // Public property for UI
    public float CooldownRemaining => Mathf.Max(0f, cooldown - (Time.time - lastUseTime));
    public float CooldownTotal => cooldown;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (DifficultyConfig.Instance != null)
            cooldown = DifficultyConfig.Instance.DashCooldown;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            TryDash();
        }
    }

    void TryDash()
    {
        if (isDashing) return;

        if (Time.time - lastUseTime < cooldown)
        {
            Debug.Log($"Dash on cooldown! {CooldownRemaining:F1}s remaining");
            return;
        }

        lastUseTime = Time.time;
        StartCoroutine(DashSequence());
    }

    private System.Collections.IEnumerator DashSequence()
    {
        isDashing = true;

        // Get movement direction or fallback to camera forward
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 dashDir = new Vector3(h, 0, v).normalized;

        if (dashDir.sqrMagnitude < 0.1f)
        {
            // If no input, dash forward relative to camera
            Camera cam = Camera.main;
            if (cam != null)
            {
                dashDir = cam.transform.forward;
                dashDir.y = 0;
                dashDir.Normalize();
            }
            else
            {
                dashDir = Vector3.forward;
            }
        }

        // Apply instant dash force
        rb.linearVelocity = Vector3.zero; // Reset current velocity
        rb.AddForce(dashDir * dashForce, ForceMode.Impulse);

        // Visual trail effect
        GameObject trail = CreateDashTrail();

        // During dash, check for enemy collisions
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;

            // Update trail position
            if (trail != null)
                trail.transform.position = transform.position;

            // Check for enemies in dash path
            Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Enemy") || hit.gameObject.name.Contains("Enemy"))
                {
                    Rigidbody enemyRb = hit.GetComponent<Rigidbody>();
                    if (enemyRb != null)
                    {
                        Vector3 pushDir = (hit.transform.position - transform.position).normalized;
                        pushDir.y = 0.3f;
                        enemyRb.AddForce(pushDir * hitPushForce, ForceMode.Impulse);
                    }
                    Debug.Log($"🏃 Dash hit enemy: {hit.name}!");
                }
            }

            yield return null;
        }

        // Cleanup trail
        if (trail != null)
        {
            // Fade out trail
            yield return StartCoroutine(FadeAndDestroy(trail, 0.3f));
        }

        isDashing = false;
    }

    private GameObject CreateDashTrail()
    {
        // Stretched trail behind the ball
        GameObject trail = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        trail.name = "DashTrail";
        Object.Destroy(trail.GetComponent<Collider>());
        trail.transform.position = transform.position;
        trail.transform.localScale = new Vector3(1.5f, 0.5f, 1.5f);

        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");
        Material mat = new Material(litShader);
        Color dashColor = new Color(1f, 0.5f, 0f); // Orange
        mat.SetColor("_BaseColor", new Color(dashColor.r, dashColor.g, dashColor.b, 0.6f));
        mat.color = new Color(dashColor.r, dashColor.g, dashColor.b, 0.6f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", dashColor * 4f);
        mat.SetFloat("_Surface", 1);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        trail.GetComponent<Renderer>().sharedMaterial = mat;

        // Glow light
        GameObject glow = new GameObject("DashGlow");
        glow.transform.SetParent(trail.transform);
        glow.transform.localPosition = Vector3.zero;
        Light gl = glow.AddComponent<Light>();
        gl.type = LightType.Point;
        gl.color = dashColor;
        gl.range = 5f;
        gl.intensity = 4f;

        return trail;
    }

    private System.Collections.IEnumerator FadeAndDestroy(GameObject obj, float duration)
    {
        Renderer ren = obj.GetComponent<Renderer>();
        Light childLight = obj.GetComponentInChildren<Light>();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (ren != null)
            {
                Color c = ren.sharedMaterial.color;
                c.a = Mathf.Lerp(0.6f, 0f, t);
                ren.sharedMaterial.color = c;
                ren.sharedMaterial.SetColor("_BaseColor", c);
            }

            if (childLight != null)
                childLight.intensity = Mathf.Lerp(4f, 0f, t);

            yield return null;
        }

        Object.Destroy(obj);
    }
}
