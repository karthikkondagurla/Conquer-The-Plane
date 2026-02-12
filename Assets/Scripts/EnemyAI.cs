using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 3.0f;
    public float chaseDistance = 10.0f;
    public int currentMapID = 0; // Managed by EnemyManager
    
    private Transform playerTarget;
    private Vector3 roamTarget;

    void Start()
    {
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

    // OnDestroy logic removed as Manager handles lifecycle

    void Update()
    {
        // Ensure we have a player target
        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTarget = playerObj.transform;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer < chaseDistance)
        {
            // CHASE STATE
            ChasePlayer();
        }
        else
        {
            // ROAM STATE
            Roam();
        }
    }

    void ChasePlayer()
    {
        // Rotate to look at player
        transform.LookAt(playerTarget);
        // Move forward
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    void Roam()
    {
        // Move towards roam target
        transform.LookAt(roamTarget);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        // Check if reached destination
        if (Vector3.Distance(transform.position, roamTarget) < 1.0f)
        {
            // Wait a bit or pick new one immediately? 
            // Simple: Pick new one
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
