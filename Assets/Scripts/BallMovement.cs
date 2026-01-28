using UnityEngine;

public class BallMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Ground Detection")]
    public float groundCheckDistance = 0.6f; // Radius (0.5) + 0.1 buffer

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Jump input using the default Unity "Jump" button (Space)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        Move();
        CheckGround();
    }

    void Move()
    {
        // Get input from WASD or Arrow keys
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Calculate movement direction relative to the world
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        // Apply force to the ball
        rb.AddForce(movement * moveSpeed);
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false; // Prevent double jumping until ground check passes
    }

    void CheckGround()
    {
        // Simple raycast downwards to detect ground
        // Origin is center of the ball
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
    }
}
