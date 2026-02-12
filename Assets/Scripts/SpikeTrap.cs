using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public float damage = 50f;          // Damage dealt to enemies
    public float lifetime = 8f;         // Seconds before spike disappears (0 = no auto-destroy)
    public float spawnDuration = 0.4f;  // Rise-up animation time

    // Victory spike state
    public bool isVictorySpike = false;
    public int plantedMapID = 0;

    private Vector3 fullScale;
    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
        fullScale = transform.localScale;
        transform.localScale = new Vector3(fullScale.x * 0.1f, 0.01f, fullScale.z * 0.1f);
        StartCoroutine(SpawnAnimation());

        // Victory spikes don't auto-destroy
        if (!isVictorySpike && lifetime > 0)
        {
            Destroy(gameObject, lifetime);
        }
    }

    private System.Collections.IEnumerator SpawnAnimation()
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spawnDuration;
            // Elastic ease-out for dramatic pop
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            transform.localScale = Vector3.Lerp(startScale, fullScale, eased);
            yield return null;
        }
        transform.localScale = fullScale;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.name.Contains("Enemy"))
        {
            // If this is the victory spike, notify GameWinManager
            if (isVictorySpike && GameWinManager.Instance != null)
            {
                GameWinManager.Instance.OnEnemyHitVictorySpike();
                return; // GameWinManager will destroy this spike
            }

            // Regular spike: push enemy away
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 pushDir = (other.transform.position - transform.position).normalized;
                pushDir.y = 0.5f;
                rb.AddForce(pushDir * 15f, ForceMode.Impulse);
            }

            Debug.Log($"Spike hit enemy: {other.name}");
        }
    }

    // Build the crystal spike 3D asset from primitives
    public static GameObject CreateSpikeAsset(Color glowColor)
    {
        GameObject spike = new GameObject("CrystalSpike");

        // Emissive crystal material
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");

        Material crystalMat = new Material(litShader);
        crystalMat.SetColor("_BaseColor", new Color(0.6f, 1f, 0.8f, 0.8f));
        crystalMat.color = new Color(0.6f, 1f, 0.8f, 0.8f);
        crystalMat.EnableKeyword("_EMISSION");
        crystalMat.SetColor("_EmissionColor", glowColor * 2.5f);
        crystalMat.SetFloat("_Smoothness", 0.95f);
        crystalMat.SetFloat("_Metallic", 0.7f);
        crystalMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        // === Main spike (tall stretched cube, rotated 45 degrees) ===
        GameObject mainShard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mainShard.name = "MainShard";
        mainShard.transform.SetParent(spike.transform);
        mainShard.transform.localPosition = new Vector3(0, 0.8f, 0);
        mainShard.transform.localScale = new Vector3(0.25f, 1.6f, 0.25f);
        mainShard.transform.localRotation = Quaternion.Euler(0, 45f, 0);
        Object.Destroy(mainShard.GetComponent<Collider>());
        mainShard.GetComponent<Renderer>().sharedMaterial = crystalMat;

        // === Top point (smaller stretched cube, angled) ===
        GameObject topPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        topPoint.name = "TopPoint";
        topPoint.transform.SetParent(spike.transform);
        topPoint.transform.localPosition = new Vector3(0.05f, 1.7f, 0.05f);
        topPoint.transform.localScale = new Vector3(0.12f, 0.5f, 0.12f);
        topPoint.transform.localRotation = Quaternion.Euler(8f, 45f, -5f);
        Object.Destroy(topPoint.GetComponent<Collider>());
        topPoint.GetComponent<Renderer>().sharedMaterial = crystalMat;

        // === Left shard (lightning jag) ===
        GameObject leftShard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftShard.name = "LeftShard";
        leftShard.transform.SetParent(spike.transform);
        leftShard.transform.localPosition = new Vector3(-0.2f, 1.0f, 0);
        leftShard.transform.localScale = new Vector3(0.35f, 0.15f, 0.18f);
        leftShard.transform.localRotation = Quaternion.Euler(0, 30f, -20f);
        Object.Destroy(leftShard.GetComponent<Collider>());
        leftShard.GetComponent<Renderer>().sharedMaterial = crystalMat;

        // === Right shard (lightning jag) ===
        GameObject rightShard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightShard.name = "RightShard";
        rightShard.transform.SetParent(spike.transform);
        rightShard.transform.localPosition = new Vector3(0.2f, 0.55f, 0);
        rightShard.transform.localScale = new Vector3(0.35f, 0.15f, 0.18f);
        rightShard.transform.localRotation = Quaternion.Euler(0, -30f, 15f);
        Object.Destroy(rightShard.GetComponent<Collider>());
        rightShard.GetComponent<Renderer>().sharedMaterial = crystalMat;

        // === Lower point (buried into ground) ===
        GameObject lowerPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lowerPoint.name = "LowerPoint";
        lowerPoint.transform.SetParent(spike.transform);
        lowerPoint.transform.localPosition = new Vector3(-0.03f, -0.05f, -0.03f);
        lowerPoint.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
        lowerPoint.transform.localRotation = Quaternion.Euler(5f, 45f, 3f);
        Object.Destroy(lowerPoint.GetComponent<Collider>());
        lowerPoint.GetComponent<Renderer>().sharedMaterial = crystalMat;

        // === Small accent shard ===
        GameObject accentShard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        accentShard.name = "AccentShard";
        accentShard.transform.SetParent(spike.transform);
        accentShard.transform.localPosition = new Vector3(0.12f, 1.3f, -0.1f);
        accentShard.transform.localScale = new Vector3(0.08f, 0.35f, 0.08f);
        accentShard.transform.localRotation = Quaternion.Euler(-10f, 60f, 15f);
        Object.Destroy(accentShard.GetComponent<Collider>());
        accentShard.GetComponent<Renderer>().sharedMaterial = crystalMat;

        // === Trigger collider on parent ===
        BoxCollider triggerCol = spike.AddComponent<BoxCollider>();
        triggerCol.size = new Vector3(0.8f, 2f, 0.8f);
        triggerCol.center = new Vector3(0, 0.8f, 0);
        triggerCol.isTrigger = true;

        // === Point light for glow ===
        GameObject glowLight = new GameObject("SpikeGlow");
        glowLight.transform.SetParent(spike.transform);
        glowLight.transform.localPosition = new Vector3(0, 1f, 0);
        Light pl = glowLight.AddComponent<Light>();
        pl.type = LightType.Point;
        pl.color = glowColor;
        pl.range = 5f;
        pl.intensity = 2f;

        return spike;
    }
}
