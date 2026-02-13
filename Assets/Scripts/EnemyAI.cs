using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 1.0f;
    public float chaseDistance = 8.0f;
    public int currentMapID = 0; // Managed by EnemyManager
    
    private Transform playerTarget;
    private Vector3 roamTarget;

    private Animator animator;
    private Rigidbody rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.isKinematic = false; // Ensure we interact with physics

        // Apply difficulty settings
        if (DifficultyConfig.Instance != null)
        {
            moveSpeed = DifficultyConfig.Instance.EnemySpeed;
            chaseDistance = DifficultyConfig.Instance.EnemyChaseDistance;
        }

        // Find the player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }
        
        // Start roaming initially
        PickNewRoamTarget();
    }

    void FixedUpdate()
    {
        // Ensure we have a player target
        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTarget = playerObj.transform;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        bool isMoving = false;

        if (distanceToPlayer < chaseDistance)
        {
            // CHASE STATE
            ChasePlayer();
            isMoving = true;
        }
        else
        {
            // ROAM STATE
            Roam();
            // check if we are still far from target
            if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(roamTarget.x, 0, roamTarget.z)) > 1.0f)
            {
                isMoving = true;
            }
        }

        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
            
            // Sync animation speed with movement speed
            // Assuming default animation matches a speed of ~3.0f, scale accordingly.
            // But ensure it doesn't get too slow (min 0.5f) or fast.
            if (isMoving)
            {
                 float targetAnimSpeed = moveSpeed / 3.0f; 
                 // Clamp to reasonable values so it doesn't look like time stop
                 animator.speed = Mathf.Clamp(targetAnimSpeed, 0.4f, 2.0f);
            }
            else
            {
                 animator.speed = 1.0f; // Idle speed normal
            }

            if (distanceToPlayer < 2.0f)
            {
                 animator.SetBool("IsAttacking", true);
                 animator.speed = 1.0f; // Attack at normal speed
            }
            else
            {
                 animator.SetBool("IsAttacking", false);
            }
        }
    }

    void ChasePlayer()
    {
        Vector3 lookPos = playerTarget.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);
        
        // rb.MovePosition for physics-based movement
        Vector3 nextPos = transform.position + transform.forward * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPos);
    }

    void Roam()
    {
        Vector3 lookPos = roamTarget;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);
        
        Vector3 nextPos = transform.position + transform.forward * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPos);

        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(roamTarget.x, 0, roamTarget.z)) < 1.0f)
        {
            PickNewRoamTarget();
        }
    }

    public void PickNewRoamTarget()
    {
        // Pick random point -10 to 10 on X/Z
        float x = Random.Range(-10f, 10f);
        float z = Random.Range(-10f, 10f);
        roamTarget = new Vector3(x, 0.5f, z);
    }
}
