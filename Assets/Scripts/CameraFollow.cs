using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    private Vector3 offset;

    void Start()
    {
        if (target != null)
        {
            // Calculate the initial offset based on current positions in the scene
            offset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Maintain the same offset as the ball moves
            transform.position = target.position + offset;
            transform.LookAt(target);
        }
    }
}
