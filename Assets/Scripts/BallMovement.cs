using UnityEngine;

public class BallMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 12f; // Reduced for Velocity-based movement
    public float jumpForce = 10f;

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
        moveSpeed = 12f; // Force correct speed
        Debug.Log($"Initial Move Speed: {moveSpeed}");

        rb = GetComponent<Rigidbody>();
        
        // Physics Setup
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearDamping = 5f;
        rb.angularDamping = 0.05f;

        SetupCharacterVisuals();
    }

    void SetupCharacterVisuals()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) meshRenderer.enabled = false;

        if (visualModel == null)
        {
            Transform existing = transform.Find("robotSphere");
            if (existing != null)
            {
                visualModel = existing;
            }
            else
            {
                GameObject robotPrefab = Resources.Load<GameObject>("robotSphere");
                if (robotPrefab != null)
                {
                    GameObject robot = Instantiate(robotPrefab, transform);
                    robot.name = "robotSphere";
                    robot.transform.localPosition = new Vector3(0, -0.5f, 0); 
                    robot.transform.localScale = Vector3.one * robotScale;
                    robot.transform.localRotation = Quaternion.identity;
                    visualModel = robot.transform;
                }
                else
                {
                    Debug.LogError("Robot Sphere prefab not found in Resources!");
                }
            }
        }

        if (visualModel != null)
        {
            visualModel.localScale = Vector3.one * robotScale;
            anim = visualModel.GetComponent<Animator>();
            if (anim == null) anim = visualModel.GetComponentInChildren<Animator>();
            
            if (anim != null) anim.applyRootMotion = false; // Disable root motion
        }
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
        
        UpdateVisuals();
    }

    void FixedUpdate()
    {
        Move();
        CheckGround();
        Debug.Log($"Current Speed: {rb.linearVelocity.magnitude}");
    }

    void Move()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;

        if (movement.magnitude >= 0.1f)
        {
            // Direct Velocity Control for "Walking" feel (No sliding)
            Vector3 targetVelocity = movement * moveSpeed;
            targetVelocity.y = rb.linearVelocity.y; // Preserve gravity
            rb.linearVelocity = targetVelocity;

            if (visualModel != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                visualModel.rotation = Quaternion.Lerp(visualModel.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
            }
        }
        else
        {
            // Stop immediately when no input
            Vector3 stopVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            rb.linearVelocity = stopVelocity;
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
            visualModel.localScale = Vector3.one * robotScale;
        }
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
    }

    void CheckGround()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
    }
}
