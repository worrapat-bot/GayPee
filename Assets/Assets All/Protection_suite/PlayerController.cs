using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2.5f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrain = 20f;
    [SerializeField] private float staminaRegen = 15f;

    [Header("Combat")]
    [SerializeField] private float punchCooldown = 0.5f;

    [Header("Head Bob")]
    [SerializeField] private float bobSpeed = 10f;
    [SerializeField] private float bobAmount = 0.05f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;
    [SerializeField] private float bodyRotationSpeed = 5f;

    private Rigidbody rb;
    private CapsuleCollider col;
    private Camera cam;
    private float stamina;
    private float normalHeight;
    private Vector3 camStartPos;
    private float bobTimer;
    private bool isCrouching;
    private bool isBlocking;
    private float lastPunch;
    private float rotationX;

    void Awake()
    {
        // Auto setup Rigidbody
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Auto setup Collider
        col = GetComponent<CapsuleCollider>();
        normalHeight = col.height;

        // Setup Camera
        cam = GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam != null) camStartPos = cam.transform.localPosition;

        stamina = maxStamina;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleCrouch();
        HandleCombat();
        UpdateStamina();
        UpdateHeadBob();

        // Toggle cursor lock
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player left/right
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera up/down
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -maxLookAngle, maxLookAngle);

        if (cam != null)
        {
            Vector3 currentRot = cam.transform.localEulerAngles;
            cam.transform.localEulerAngles = new Vector3(rotationX, 0, 0);
        }
    }

    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = (transform.right * h + transform.forward * v).normalized;

        // Calculate speed
        bool wantRun = Input.GetKey(KeyCode.LeftShift) && stamina > 1f && !isCrouching;
        float speed = isCrouching ? crouchSpeed : (wantRun ? runSpeed : walkSpeed);
        if (isBlocking) speed *= 0.3f;

        // Apply movement
        Vector3 vel = rb.linearVelocity;
        vel.x = dir.x * speed;
        vel.z = dir.z * speed;
        rb.linearVelocity = vel;
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
            col.height = isCrouching ? normalHeight * 0.5f : normalHeight;

            // Adjust camera position
            if (cam != null)
            {
                Vector3 pos = camStartPos;
                pos.y = isCrouching ? camStartPos.y * 0.5f : camStartPos.y;
                cam.transform.localPosition = pos;
            }
        }
    }

    void HandleCombat()
    {
        // Punch
        if (Input.GetMouseButtonDown(0) && Time.time > lastPunch + punchCooldown && !isBlocking)
        {
            Debug.Log("PUNCH!");
            lastPunch = Time.time;
        }

        // Block
        isBlocking = Input.GetMouseButton(1);
    }

    void UpdateStamina()
    {
        bool running = Input.GetKey(KeyCode.LeftShift) && rb.linearVelocity.magnitude > 1f && !isCrouching;

        if (running)
            stamina = Mathf.Max(0, stamina - staminaDrain * Time.deltaTime);
        else
            stamina = Mathf.Min(maxStamina, stamina + staminaRegen * Time.deltaTime);
    }

    void UpdateHeadBob()
    {
        if (cam == null) return;

        Vector3 targetPos = isCrouching ? camStartPos * 0.5f : camStartPos;
        float speed = rb.linearVelocity.magnitude;

        if (speed > 0.1f)
        {
            bool running = Input.GetKey(KeyCode.LeftShift) && stamina > 1f;
            float mult = running ? 1.5f : 1f;

            bobTimer += Time.deltaTime * bobSpeed * mult;
            targetPos.y += Mathf.Sin(bobTimer) * bobAmount * mult;
            targetPos.x += Mathf.Cos(bobTimer * 0.5f) * bobAmount * 0.5f * mult;
        }
        else
        {
            bobTimer = 0;
        }

        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, targetPos, Time.deltaTime * 10f);
    }

    public float GetStamina() => stamina / maxStamina;

    void OnGUI()
    {
        // Simple Stamina Bar
        GUI.Box(new Rect(10, 10, 200, 30), $"Stamina: {stamina:F0}/{maxStamina}");
        GUI.Box(new Rect(10, 50, 200, 20), "");
        GUI.Box(new Rect(10, 50, 200 * GetStamina(), 20), "");
    }
}