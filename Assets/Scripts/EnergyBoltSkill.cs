using UnityEngine;

public class EnergyBoltSkill : MonoBehaviour
{
    [Header("Energy Bolt Settings")]
    public float cooldown = 2f;
    public float boltSpeed = 25f;
    public float pushForce = 15f;
    public float lifetime = 3f;

    private float lastUseTime = -99f;

    // Public property for UI
    public float CooldownRemaining => Mathf.Max(0f, cooldown - (Time.time - lastUseTime));
    public float CooldownTotal => cooldown;

    void Start()
    {
        if (DifficultyConfig.Instance != null)
            cooldown = DifficultyConfig.Instance.BoltCooldown;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryFireBolt();
        }
    }

    void TryFireBolt()
    {
        if (Time.time - lastUseTime < cooldown)
        {
            Debug.Log($"Energy Bolt on cooldown! {CooldownRemaining:F1}s remaining");
            return;
        }

        lastUseTime = Time.time;

        // Get aim direction from movement input or camera forward
        Vector3 aimDir;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
        {
            aimDir = new Vector3(h, 0, v).normalized;
        }
        else
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                aimDir = cam.transform.forward;
                aimDir.y = 0;
                aimDir.Normalize();
            }
            else
            {
                aimDir = Vector3.forward;
            }
        }

        // Create the bolt projectile
        GameObject bolt = CreateBoltObject();
        bolt.transform.position = transform.position + aimDir * 1.2f + Vector3.up * 0.3f;

        // Add Rigidbody and launch
        Rigidbody boltRb = bolt.AddComponent<Rigidbody>();
        boltRb.useGravity = false;
        boltRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        boltRb.linearVelocity = aimDir * boltSpeed;
        boltRb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

        // Add bolt behavior
        EnergyBoltProjectile boltScript = bolt.AddComponent<EnergyBoltProjectile>();
        boltScript.pushForce = pushForce;

        // Auto-destroy after lifetime
        Object.Destroy(bolt, lifetime);

        Debug.Log("🔫 Energy Bolt fired!");
    }

    private GameObject CreateBoltObject()
    {
        GameObject bolt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bolt.name = "EnergyBolt";
        bolt.transform.localScale = Vector3.one * 0.4f;
        bolt.layer = 0; // Default layer

        // Remove default collider, add trigger
        Object.Destroy(bolt.GetComponent<Collider>());
        SphereCollider trigger = bolt.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 0.5f;

        // Glowing green material
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");
        Material mat = new Material(litShader);
        Color boltColor = new Color(0.2f, 1f, 0.4f); // Green
        mat.SetColor("_BaseColor", boltColor);
        mat.color = boltColor;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", boltColor * 5f);
        mat.SetFloat("_Smoothness", 1f);
        mat.SetFloat("_Metallic", 0.8f);
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        bolt.GetComponent<Renderer>().sharedMaterial = mat;

        // Glow light
        GameObject glow = new GameObject("BoltGlow");
        glow.transform.SetParent(bolt.transform);
        glow.transform.localPosition = Vector3.zero;
        Light gl = glow.AddComponent<Light>();
        gl.type = LightType.Point;
        gl.color = boltColor;
        gl.range = 6f;
        gl.intensity = 3f;

        return bolt;
    }
}

/// <summary>
/// Attached to each projectile to handle collision with enemies.
/// </summary>
public class EnergyBoltProjectile : MonoBehaviour
{
    public float pushForce = 15f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.gameObject.name.Contains("Enemy"))
        {
            // Push enemy
            Rigidbody enemyRb = other.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                Vector3 pushDir = (other.transform.position - transform.position).normalized;
                pushDir.y = 0.3f;
                enemyRb.AddForce(pushDir * pushForce, ForceMode.Impulse);
            }

            Debug.Log($"🔫 Energy Bolt hit enemy: {other.name}!");

            // Spawn small impact flash
            StartCoroutine(ImpactFlash());
        }
        else if (!other.CompareTag("Player") && !other.isTrigger)
        {
            // Hit a wall or obstacle — destroy
            Object.Destroy(gameObject);
        }
    }

    private System.Collections.IEnumerator ImpactFlash()
    {
        // Small green flash at impact
        GameObject flash = new GameObject("BoltImpact");
        flash.transform.position = transform.position;
        Light fl = flash.AddComponent<Light>();
        fl.type = LightType.Point;
        fl.color = new Color(0.2f, 1f, 0.4f);
        fl.range = 5f;
        fl.intensity = 6f;

        // Destroy the bolt
        Renderer ren = GetComponent<Renderer>();
        if (ren != null) ren.enabled = false;

        float elapsed = 0f;
        float duration = 0.2f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fl.intensity = Mathf.Lerp(6f, 0f, elapsed / duration);
            yield return null;
        }

        Object.Destroy(flash);
        Object.Destroy(gameObject);
    }
}
