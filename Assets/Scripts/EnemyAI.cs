using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 3.0f;
    public float chaseDistance = 10.0f;
    public int originMapID = 0; // Assigned by spawner
    
    private Transform playerTarget;
    private Vector3 roamTarget;
    private bool isRoaming;
    private bool isPersistent = false; // Flag to prevent duplicate DontDestroy

    void Start()
    {
        // Persistence
        DontDestroyOnLoad(gameObject);
        
        // Register counts
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RegisterEnemy(originMapID);
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

    void OnDestroy()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemy(originMapID);
        }
    }

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
