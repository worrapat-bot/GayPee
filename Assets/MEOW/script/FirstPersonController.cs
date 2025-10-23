using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float airControl = 0.3f;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float lookXLimit = 80f;
    [SerializeField] private Transform cameraTransform;

    [Header("Ground Detection")]
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundMask = ~0; // ~0 means all layers

    // Components
    private Rigidbody rb;
    private CapsuleCollider col;
    private Animator anim;

    // State
    private Vector3 moveDirection;
    private float rotationX = 0f;
    private bool isGrounded;
    private bool canJump;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        anim = GetComponent<Animator>();

        // Setup Rigidbody
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Find camera if not assigned
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // Ground check using collider height and offset
        CheckGround();

        // Handle input
        HandleMouseLook();
        HandleMovementInput();
        HandleJumpInput();

        // Update animator
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        ApplyMovement();
        ApplyGravity();
    }

    void CheckGround()
    {
        // Calculate feet position using collider height and offset
        Vector3 feetPosition = transform.position + col.center - Vector3.up * (col.height * 0.5f);

        // Cast ray from feet downward
        RaycastHit hit;
        isGrounded = Physics.Raycast(feetPosition, Vector3.down, out hit, groundCheckDistance, groundMask);

        // Allow jumping only when grounded
        canJump = isGrounded;
    }

    void HandleMouseLook()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player body (Y-axis)
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera (X-axis) with limit
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }

    void HandleMovementInput()
    {
        // Get input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Calculate movement direction relative to player rotation
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        moveDirection = (forward * moveZ + right * moveX).normalized;
    }

    void HandleJumpInput()
    {
        if (Input.GetButtonDown("Jump") && canJump)
        {
            // Apply jump force
            Vector3 vel = rb.linearVelocity;
            vel.y = jumpForce;
            rb.linearVelocity = vel;

            // Trigger jump animation
            if (anim != null)
            {
                anim.SetTrigger("Jump");
            }
        }
    }

    void ApplyMovement()
    {
        // Determine current speed
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Apply air control when not grounded
        if (!isGrounded)
        {
            currentSpeed *= airControl;
        }

        // Calculate target velocity (preserve Y velocity)
        Vector3 targetVelocity = moveDirection * currentSpeed;
        targetVelocity.y = rb.linearVelocity.y;

        // Apply velocity
        rb.linearVelocity = targetVelocity;
    }

    void ApplyGravity()
    {
        // Apply additional gravity when falling for better feel
        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * gravity * Time.fixedDeltaTime;
        }
    }

    void UpdateAnimator()
    {
        if (anim == null) return;

        // Calculate speed for animation
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float speed = horizontalVelocity.magnitude;

        // Set animator parameters
        anim.SetFloat("Speed", speed);
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetBool("IsRunning", Input.GetKey(KeyCode.LeftShift) && speed > 0.1f);
    }

    // Debug visualization
    void OnDrawGizmos()
    {
        if (col == null) return;

        // Draw ground check ray
        Vector3 feetPosition = transform.position + col.center - Vector3.up * (col.height * 0.5f);
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(feetPosition, feetPosition + Vector3.down * groundCheckDistance);
        Gizmos.DrawWireSphere(feetPosition + Vector3.down * groundCheckDistance, 0.1f);
    }
}