using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController), typeof(AudioSource))]
public class AdvancedFirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float runSpeed = 8f;
    public float crouchSpeed = 2f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f;
    public float staminaRecoveryRate = 15f;
    public float lowStaminaThreshold = 25f;
    public Slider staminaBar;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;
    public Transform cameraTransform;

    [Header("Head Bob Settings")]
    public float bobSpeed = 10f;
    public float bobAmount = 0.05f;

    [Header("Audio Settings")]
    public AudioClip footstepSound;
    public AudioClip tiredBreathSound;
    public float stepInterval = 0.5f;

    private CharacterController controller;
    private AudioSource audioSource;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;

    private bool isRunning = false;
    private bool isCrouching = false;
    private float currentStamina;
    private float defaultHeight;
    private float crouchHeight = 1f;

    private float bobTimer;
    private float stepTimer;
    private Vector3 cameraStartPos;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentStamina = maxStamina;
        defaultHeight = controller.height;
        cameraStartPos = cameraTransform.localPosition;

        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = currentStamina;
        }
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleHeadBob();
        HandleStamina();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        // หมอบ
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
            controller.height = isCrouching ? crouchHeight : defaultHeight;
        }

        // วิ่ง
        isRunning = Input.GetKey(KeyCode.LeftShift) && currentStamina > 0 && !isCrouching;

        float currentSpeed = walkSpeed;
        if (isRunning)
            currentSpeed = runSpeed;
        else if (isCrouching)
            currentSpeed = crouchSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        // กระโดด
        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // แรงโน้มถ่วง
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // เสียงฝีเท้า
        if (isGrounded && move.magnitude > 0.1f)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                audioSource.PlayOneShot(footstepSound);
                stepTimer = stepInterval / (isRunning ? 1.5f : 1f);
            }
        }
    }

    void HandleHeadBob()
    {
        if (!controller.isGrounded) return;

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (move.magnitude > 0.1f)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float bobOffset = Mathf.Sin(bobTimer) * bobAmount;
            cameraTransform.localPosition = cameraStartPos + new Vector3(0, bobOffset, 0);
        }
        else
        {
            bobTimer = 0;
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, cameraStartPos, Time.deltaTime * bobSpeed);
        }
    }

    void HandleStamina()
    {
        if (isRunning)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina < 0) currentStamina = 0;
        }
        else
        {
            currentStamina += staminaRecoveryRate * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
        }

        if (staminaBar != null)
            staminaBar.value = Mathf.Lerp(staminaBar.value, currentStamina, Time.deltaTime * 10f);

        // เล่นเสียงหอบเมื่อ stamina ต่ำ
        if (currentStamina < lowStaminaThreshold && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(tiredBreathSound);
        }
    }
}
