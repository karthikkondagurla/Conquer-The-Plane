using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float speed = 3.0f;
    private Transform target;

    void Start()
    {
        // Find the player automatically since they persist
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    void Update()
    {
        if (target != null)
        {
            // Rotate to look at player
            transform.LookAt(target);
            
            // Move forward
            transform.position += transform.forward * speed * Time.deltaTime;
        }
        else
        {
            // Retry finding player if null (in case of sync issues)
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) target = playerObj.transform;
        }
    }
}
