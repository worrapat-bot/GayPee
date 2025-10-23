using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Ground Detection")]
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private float maxSlopeAngle = 45f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private Transform player;
    private Rigidbody rb;
    private Collider col;
    private bool isGrounded;
    private Vector3 groundNormal;

    void Start()
    {
        // Get required components
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody component is missing!");
            enabled = false;
            return;
        }

        if (col == null)
        {
            Debug.LogError("Collider component is missing!");
            enabled = false;
            return;
        }

        // Find player in scene
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player object not found in scene!");
            enabled = false;
            return;
        }

        // Get or add Animator
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Configure Rigidbody
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        // Check ground every frame
        CheckGround();

        // Update animator parameters
        if (animator != null)
        {
            float speed = rb.linearVelocity.magnitude;
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsGrounded", isGrounded);
        }
    }

    void FixedUpdate()
    {
        if (player == null || !isGrounded) return;

        // Calculate direction to player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // Project direction onto ground plane
        directionToPlayer = Vector3.ProjectOnPlane(directionToPlayer, groundNormal).normalized;

        // Move towards player
        Vector3 targetVelocity = directionToPlayer * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y; // Preserve vertical velocity

        rb.linearVelocity = targetVelocity;

        // Rotate towards player
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void CheckGround()
    {
        // Calculate feet position using collider bounds
        Vector3 feetPosition = transform.position;

        // Use collider height and center offset to calculate exact feet position
        if (col is BoxCollider boxCol)
        {
            feetPosition.y = transform.position.y + boxCol.center.y - (boxCol.size.y * 0.5f);
        }
        else if (col is CapsuleCollider capCol)
        {
            feetPosition.y = transform.position.y + capCol.center.y - (capCol.height * 0.5f);
        }
        else if (col is SphereCollider sphereCol)
        {
            feetPosition.y = transform.position.y + sphereCol.center.y - sphereCol.radius;
        }

        // Cast ray downward from feet position
        RaycastHit hit;
        int layerMask = ~0; // All layers

        if (Physics.Raycast(feetPosition, Vector3.down, out hit, groundCheckDistance, layerMask))
        {
            isGrounded = true;
            groundNormal = hit.normal;

            // Check if slope is too steep
            float slopeAngle = Vector3.Angle(Vector3.up, hit.normal);
            if (slopeAngle > maxSlopeAngle)
            {
                isGrounded = false;
            }
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
        }

        // Debug visualization
        Debug.DrawRay(feetPosition, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if we touched the player
        if (collision.gameObject.name == "Player")
        {
            Debug.Log("You Died");
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // Continuous check while touching player
        if (collision.gameObject.name == "Player")
        {
            Debug.Log("You Died");
        }
    }
}