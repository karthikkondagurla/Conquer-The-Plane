using UnityEngine;

public class BallMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveForce = 35f; // Reduced for a heavier, slower start
    public float maxSpeed = 7f;   // Reduced top speed
    public float jumpForce = 10f;
    public float decelerationMultiplier = 2.5f; // Increased braking for a heavier feel

    [Header("Visuals")]
    public Transform visualModel;
    public float turnSpeed = 10f;
    public float robotScale = 10.0f; // Default large scale
    private Animator anim;

    [Header("Animation Thresholds")]
    public float walkSpeedThreshold = 0.1f;
    public float rollSpeedThreshold = 6.0f; // Speed to trigger rolling

    [Header("Ground Detection")]
    public float groundCheckDistance = 0.6f;

    private Rigidbody rb;
    private bool isGrounded;

    // Animator Constraints
    // Roll_Anim: Bool -> Triggers Roll Sequence
    // Walk_Anim: Bool -> Triggers Walk Sequence
    // Open_Anim: Bool -> Defaults to True (Open/Idle). False = Closed/Sleep.

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Physics Setup
        rb.mass = 3f; // Added physical weight so it pushes objects harder
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Keep frozen so visual model doesn't tumble
        rb.linearDamping = 1f; // Reduced damping so AddForce works better
        rb.angularDamping = 0.05f;

        SetupCharacterVisuals();
    }

    void SetupCharacterVisuals()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) meshRenderer.enabled = false;

        Transform oldRobot = transform.Find("robotSphere");
        if (oldRobot != null)
        {
            if (visualModel == oldRobot) visualModel = null;
            if (Application.isPlaying) Destroy(oldRobot.gameObject);
            else DestroyImmediate(oldRobot.gameObject);
        }

        if (visualModel == null)
        {
            Transform existing = transform.Find("DefaultSphere");
            if (existing != null)
            {
                visualModel = existing;
            }
            else
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = "DefaultSphere";
                
                // Remove collider since parent handles collisions
                if (Application.isPlaying) Destroy(sphere.GetComponent<Collider>());
                else DestroyImmediate(sphere.GetComponent<Collider>());
                
                sphere.transform.SetParent(transform);
                sphere.transform.localPosition = Vector3.zero; 
                sphere.transform.localScale = Vector3.one * robotScale;
                sphere.transform.localRotation = Quaternion.identity;
                visualModel = sphere.transform;

                // Apply the player sphere material
                ApplySphereMaterial(sphere);
            }
        }

        if (visualModel != null)
        {
            float targetScale = visualModel.name == "DefaultSphere" ? 1.0f : robotScale;
            visualModel.localScale = Vector3.one * targetScale;

            // If it's the sphere, unfreeze rotation so it can physically roll.
            // If it's the robot, freeze rotation so it stays upright.
            bool isSphere = (visualModel.name == "DefaultSphere");
            rb.constraints = isSphere ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeRotation;
            
            anim = visualModel.GetComponent<Animator>();
            if (anim == null) anim = visualModel.GetComponentInChildren<Animator>();
            
            if (anim != null) anim.applyRootMotion = false; // Disable root motion
        }
    }

    void ApplySphereMaterial(GameObject sphere)
    {
        MeshRenderer renderer = sphere.GetComponent<MeshRenderer>();
        if (renderer == null) return;

        // Build the swirling iridescent material from the custom shader
        Shader swirlShader = Shader.Find("Custom/SwirlingSphere");

        Material mat;
        if (swirlShader != null)
        {
            mat = new Material(swirlShader);
            mat.name = "SwirlingSphere_Runtime";

            // Dark near-black deep teal base
            mat.SetColor("_BaseColor",   new Color(0.02f, 0.04f, 0.06f, 1f));

            // Neon teal swirl lines
            mat.SetColor("_SwirlColorA", new Color(0.00f, 1.00f, 0.55f, 1f));
            // Deeper blue-green swirl lines
            mat.SetColor("_SwirlColorB", new Color(0.00f, 0.40f, 1.00f, 1f));

            mat.SetFloat("_SwirlScale",      5.0f);   // density of swirls
            mat.SetFloat("_SwirlSpeed",      0.25f);  // animation speed
            mat.SetFloat("_SwirlWidth",      0.18f);  // line thickness
            mat.SetFloat("_SwirlSharpness",  9.0f);   // how crisp the lines are
            mat.SetFloat("_EmissionPower",   3.5f);   // glow brightness
            mat.SetFloat("_IridPower",       2.5f);   // angle of iridescence falloff
            mat.SetFloat("_IridStrength",    0.9f);   // rainbow shimmer intensity
            mat.SetFloat("_Metallic",        0.85f);
            mat.SetFloat("_Smoothness",      0.92f);
        }
        else
        {
            // Fallback: plain deep metallic teal if shader not found yet
            Debug.LogWarning("Custom/SwirlingSphere shader not found. Using fallback material.");
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = "SwirlingSphere_Fallback";
            mat.SetColor("_BaseColor", new Color(0.02f, 0.08f, 0.15f, 1f));
            mat.SetFloat("_Metallic",   0.9f);
            mat.SetFloat("_Smoothness", 0.95f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0f, 0.8f, 0.6f) * 2f);
        }

        renderer.material = mat;
    }

    [ContextMenu("Reload Character")]
    public void ReloadCharacter()
    {
        if (visualModel != null)
        {
            DestroyImmediate(visualModel.gameObject);
            visualModel = null;
        }
        SetupCharacterVisuals();
    }

    // For landing detection
    private bool wasGrounded = false;

    void Update()
    {
        // Debug: Press R to reload character
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadCharacter();
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        // Landing sound
        if (!wasGrounded && isGrounded)
            AudioManager.Instance?.Play(AudioManager.Sound.Land);
        wasGrounded = isGrounded;
        
        UpdateVisuals();
    }

    void FixedUpdate()
    {
        Move();
        CheckGround();
    }

    void Move()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;

        if (movement.magnitude >= 0.1f)
        {
            // Rolling sound
            AudioManager.Instance?.Play(AudioManager.Sound.Roll);

            // Apply physics force for smooth acceleration
            rb.AddForce(movement * moveForce, ForceMode.Acceleration);

            // Cap the maximum horizontal speed (unless dashing)
            DashStrikeSkill dashSkill = GetComponent<DashStrikeSkill>();
            bool isDashing = dashSkill != null && dashSkill.isDashing;

            if (!isDashing)
            {
                Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                if (horizontalVelocity.magnitude > maxSpeed)
                {
                    Vector3 cappedVelocity = horizontalVelocity.normalized * maxSpeed;
                    rb.linearVelocity = new Vector3(cappedVelocity.x, rb.linearVelocity.y, cappedVelocity.z);
                }
            }

            // Only force look rotation if it is NOT the sphere. 
            // The sphere should roll physically via friction.
            if (visualModel != null && visualModel.name != "DefaultSphere")
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                visualModel.rotation = Quaternion.Lerp(visualModel.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
            }
        }
        else
        {
            // Stop rolling sound when idle
            AudioManager.Instance?.StopRoll();

            // Smoothly decelerate when there is no input
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            
            // Apply an opposing force to act as friction/braking
            if (horizontalVelocity.magnitude > 0.5f)
            {
                rb.AddForce(-horizontalVelocity.normalized * moveForce * decelerationMultiplier, ForceMode.Acceleration);
            }
            else
            {
                // Full stop when moving very slowly to prevent drifting
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }
    }

    void UpdateVisuals()
    {
        if (anim == null) return;

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float speed = horizontalVelocity.magnitude;
        bool hasInput = Input.anyKey;

        // --- STRICT STATE MACHINE LOGIC ---
        
        // 1. Determine State based on Speed & Input
        bool shouldRoll = speed > rollSpeedThreshold && hasInput;
        bool shouldWalk = speed > walkSpeedThreshold && speed <= rollSpeedThreshold && hasInput;
        
        // 2. Open_Anim should always be TRUE to keep robot "Awake" (Idle State)
        // If we set it to false, it goes to "Sleep" state, which we don't want during gameplay.
        anim.SetBool("Open_Anim", true);

        // 3. Apply Mutually Exclusive States
        if (shouldRoll)
        {
            anim.SetBool("Roll_Anim", true);
            anim.SetBool("Walk_Anim", false);
        }
        else if (shouldWalk)
        {
            anim.SetBool("Roll_Anim", false);
            anim.SetBool("Walk_Anim", true);
        }
        else
        {
            // IDLE
            anim.SetBool("Roll_Anim", false);
            anim.SetBool("Walk_Anim", false);
        }
    }

    void OnValidate()
    {
        if (visualModel != null)
        {
            float targetScale = visualModel.name == "DefaultSphere" ? 1.0f : robotScale;
            visualModel.localScale = Vector3.one * targetScale;
        }
    }

    void Jump()
    {
        // Use VelocityChange so jump height remains consistent regardless of the heavier mass
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        isGrounded = false;
        AudioManager.Instance?.Play(AudioManager.Sound.Jump);
    }

    void CheckGround()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
    }
}
