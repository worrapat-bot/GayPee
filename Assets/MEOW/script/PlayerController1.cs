using UnityEngine;
using System.Collections; // เพิ่มเพื่อให้รองรับ Coroutines หากต้องการใช้ในอนาคต

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(AudioSource))]
public class PlayerController1 : MonoBehaviour
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

    [Header("Throw Settings")]
    [SerializeField] private float minThrowForce = 5f;
    [SerializeField] private float maxThrowForce = 20f;
    [SerializeField] private float maxChargeTime = 2f;

    [Header("Head Bob")]
    [SerializeField] private float bobSpeed = 10f;
    [SerializeField] private float bobAmount = 0.05f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("UI Settings")]
    [SerializeField] private Color staminaBarBg = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color staminaBarColor = new Color(0.3f, 0.9f, 0.3f, 1f);
    [SerializeField] private Color throwBarBg = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color throwBarColorStart = Color.yellow;
    [SerializeField] private Color throwBarColorMax = Color.red;

    [Header("Inventory Reference")]
    [SerializeField] private RadialInventoryVertical inventory;

    [Header("🆕 Ragdoll Settings")]
    [SerializeField] private Rigidbody[] ragdollBodies;

    [Header("🔊 Audio Clips")]
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip runClip;
    [SerializeField] private AudioClip punchClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;

    private Rigidbody rb;
    private CapsuleCollider col;
    private Camera cam;
    private Animator anim;
    private AudioSource audioSource;
    private float stamina;
    private float normalHeight;
    private Vector3 camStartPos;
    private float bobTimer;
    private bool isCrouching;
    private bool isBlocking;
    private float lastPunch;
    private float rotationX;
    private float throwChargeStart = -1f;
    private bool isChargingThrow = false;
    private bool isRagdollActive = false;
    public static bool dialog = false;
    private bool wasGrounded = true;
    private float stepTimer = 0f;
    
    // ✅ NEW FIELD: สถานะควบคุมทั้งหมด
    private bool canControl = true; // เริ่มต้นให้ควบคุมได้

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        col = GetComponent<CapsuleCollider>();
        normalHeight = col.height;

        cam = GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam != null) camStartPos = cam.transform.localPosition;

        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        stamina = maxStamina;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (inventory == null)
            inventory = FindObjectOfType<RadialInventoryVertical>();
    }

    void Update()
    {
        // 💡 MODIFIED: ถ้า Ragdoll Active หรือควบคุมไม่ได้ หรืออยู่ใน Dialog ให้จัดการแค่ Mouse Look เพื่อล็อคเมาส์แล้วออก
        if (isRagdollActive || !canControl || PlayerController.dialog)
        {
            HandleMouseLook(); 
            return;
        }

        // ❌ โค้ดเดิมจะทำงานเมื่อ canControl เป็น true เท่านั้น
        HandleMouseLook();
        HandleCrouch();
        HandleCombat();
        HandleDropItem();
        UpdateStamina();
        UpdateHeadBob();
        HandleFootsteps();
        HandleJumpLandSound();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlaySound(jumpClip);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }

        // จัดการ Cursor (ซ้ำกับ logic ใน HandleMouseLook แต่เก็บไว้เพื่อความสมบูรณ์ของโค้ดเดิม)
        if (!PlayerController.dialog)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void FixedUpdate()
    {
        // 💡 MODIFIED: ถ้า Ragdoll Active หรือควบคุมไม่ได้ หรืออยู่ใน Dialog ให้หยุด Rigidbody ทันที
        if (isRagdollActive || !canControl || PlayerController.dialog)
        {
            rb.linearVelocity = Vector3.zero; // หยุดการเคลื่อนที่ Rigidbody ทันที
            return;
        }
        
        Move();
    }

    void HandleMouseLook()
    {
        // 💡 MODIFIED: ถ้าควบคุมไม่ได้ ให้จัดการแค่ Cursor Lock
        if (!canControl)
        {
             if (!PlayerController.dialog)
             {
                 Cursor.lockState = CursorLockMode.Locked;
                 Cursor.visible = false;
             }
             return; // หยุดการหมุนกล้อง
        }
        
        // ❌ โค้ดหมุนกล้องเดิม
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -maxLookAngle, maxLookAngle);

        if (cam != null)
        {
            cam.transform.localEulerAngles = new Vector3(rotationX, 0, 0);
        }
    }

    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = (transform.right * h + transform.forward * v).normalized;

        bool wantRun = Input.GetKey(KeyCode.LeftShift) && stamina > 1f && !isCrouching;
        float speed = isCrouching ? crouchSpeed : (wantRun ? runSpeed : walkSpeed);
        if (isBlocking) speed *= 0.3f;

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
        if (Input.GetMouseButtonDown(0) && !isBlocking)
        {
            if (inventory != null && inventory.HasItemInHand())
            {
                isChargingThrow = true;
                throwChargeStart = Time.time;
            }
            else
            {
                if (Time.time > lastPunch + punchCooldown)
                {
                    PlaySound(punchClip);
                    lastPunch = Time.time;
                }
            }
        }

        if (Input.GetMouseButtonUp(0) && isChargingThrow)
        {
            isChargingThrow = false;

            float chargeTime = Time.time - throwChargeStart;
            chargeTime = Mathf.Clamp(chargeTime, 0f, maxChargeTime);
            float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, chargeTime / maxChargeTime);

            if (inventory != null)
            {
                inventory.ThrowCurrentItem(throwForce);
            }
        }

        isBlocking = Input.GetMouseButton(1);
    }

    void HandleDropItem()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (inventory != null && inventory.HasItemInHand())
            {
                inventory.DropCurrentItem();
            }
        }
    }

    void UpdateStamina()
    {
        bool running = Input.GetKey(KeyCode.LeftShift) && rb.linearVelocity.magnitude > 1f && !isCrouching;

        if (running)
            stamina = Mathf.Max(0, stamina - staminaDrain * Time.deltaTime);
        else
            stamina = Mathf.Min(maxStamina, stamina + staminaRegen * Time.deltaTime);
    }

    void HandleFootsteps()
    {
        if (!IsGrounded()) return;

        float speed = rb.linearVelocity.magnitude;
        if (speed < 0.3f) return;

        bool running = Input.GetKey(KeyCode.LeftShift) && stamina > 1f && !isCrouching;
        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            stepTimer = running ? 0.3f : 0.5f;
            PlaySound(running ? runClip : walkClip);
        }
    }

    void HandleJumpLandSound()
    {
        bool groundedNow = IsGrounded();
        if (groundedNow && !wasGrounded)
        {
            PlaySound(landClip);
        }
        wasGrounded = groundedNow;
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, col.bounds.extents.y + 0.1f);
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

    void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(clip);
    }

    public float GetStamina() => stamina / maxStamina;

    void OnGUI()
    {
        // Stamina Bar
        GUI.color = staminaBarBg;
        GUI.Box(new Rect(10, 10, 200, 20), "");
        GUI.color = staminaBarColor;
        GUI.Box(new Rect(10, 10, 200 * GetStamina(), 20), "");

        // Throw Power Bar
        if (isChargingThrow)
        {
            float chargeTime = Time.time - throwChargeStart;
            chargeTime = Mathf.Clamp(chargeTime, 0f, maxChargeTime);
            float chargePercent = chargeTime / maxChargeTime;

            GUI.color = throwBarBg;
            GUI.Box(new Rect(10, 40, 200, 20), "");
            GUI.color = Color.Lerp(throwBarColorStart, throwBarColorMax, chargePercent);
            GUI.Box(new Rect(10, 40, 200 * chargePercent, 20), "");
        }

        // Crosshair (.)
        GUI.color = Color.white;
        GUI.Label(
            new Rect((Screen.width / 2) - 5, (Screen.height / 2) - 8, 20, 20),
            "<size=25><b>.</b></size>"
        );
    }
    
    // ✅ PUBLIC METHOD: เมธอดนี้ถูกเรียกจาก EnemyPatrol เพื่อสั่งหยุดผู้เล่น
    /// <summary>
    /// เปิดหรือปิดการควบคุมทั้งหมดของผู้เล่น (การเดิน, การมอง, Combat)
    /// </summary>
    public void SetControlEnabled(bool state)
    {
        canControl = state;
        
        // ถ้าสั่งหยุด (state = false) ให้ตั้ง Rigidbody Velocity เป็นศูนย์ทันที
        if (!state && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // ถ้าสั่งเริ่ม (state = true) ให้ล็อคเมาส์กลับไป
        if (state)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}