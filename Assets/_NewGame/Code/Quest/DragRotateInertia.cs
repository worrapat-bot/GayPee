using UnityEngine;

public class DragMoveRotatePro : MonoBehaviour
{
    [Header("=== ROTATION SETTINGS ===")]
    public bool enableRotation = true;
    public float rotationSpeed = 5f;
    public float rotationInertia = 5f;

    [Header("Rotation Axes")]
    public bool rotateX = true;
    public bool rotateY = true;
    public bool rotateZ = false;

    [Header("Rotation Limits")]
    public bool limitRotation = false;
    public Vector3 minRotation = new Vector3(-80, -180, -45);
    public Vector3 maxRotation = new Vector3(80, 180, 45);

    [Header("=== MOVEMENT SETTINGS ===")]
    public bool enableMovement = true;
    public float moveSpeed = 0.01f;
    public float moveInertia = 5f;

    [Header("Movement Axes")]
    public bool moveX = true;
    public bool moveY = false;
    public bool moveZ = true;

    [Header("Movement Limits")]
    public bool limitMovement = false;
    public Vector3 minPosition = new Vector3(-5, 0, -5);
    public Vector3 maxPosition = new Vector3(5, 5, 5);

    // --- Private fields ---
    private Vector3 lastInputPosition;
    private Vector3 rotationVelocity;
    private Vector3 moveVelocity;
    private Vector3 currentRotation;
    private bool isDragging = false;
    private bool isPointerOver = false; // ตรวจว่าคลิกโดน object หรือไม่
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        currentRotation = transform.rotation.eulerAngles;
    }

    void Update()
    {
        HandleInput();
        ApplyRotation();
        ApplyMovement();
    }

    // -----------------------------------------------------------
    void HandleInput()
    {
        // ตรวจจับ "เริ่มคลิก"
        if (Input.GetMouseButtonDown(0))
        {
            isPointerOver = CheckPointerOverObject();
            if (isPointerOver)
            {
                isDragging = true;
                lastInputPosition = Input.mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            isPointerOver = false;
        }

        // --- ทัช (มือถือ) ---
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                isPointerOver = CheckPointerOverObject(touch.position);
                if (isPointerOver)
                {
                    isDragging = true;
                    lastInputPosition = touch.position;
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isDragging = false;
                isPointerOver = false;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector3 delta = touch.deltaPosition;
                ProcessDrag(delta);
            }
        }

        // --- เมาส์ลาก ---
        if (isDragging && Input.touchCount == 0)
        {
            Vector3 delta = Input.mousePosition - lastInputPosition;
            ProcessDrag(delta);
            lastInputPosition = Input.mousePosition;
        }

        // --- inertia ---
        rotationVelocity = Vector3.Lerp(rotationVelocity, Vector3.zero, rotationInertia * Time.deltaTime);
        moveVelocity = Vector3.Lerp(moveVelocity, Vector3.zero, moveInertia * Time.deltaTime);
    }

    // -----------------------------------------------------------
    void ProcessDrag(Vector3 delta)
    {
        if (enableRotation)
            rotationVelocity = new Vector3(-delta.y, delta.x, delta.x * 0.5f) * rotationSpeed * Time.deltaTime;

        if (enableMovement)
            moveVelocity = new Vector3(delta.x, delta.y, delta.y) * moveSpeed * Time.deltaTime;
    }

    // -----------------------------------------------------------
    bool CheckPointerOverObject(Vector3? position = null)
    {
        Vector3 inputPos = position ?? Input.mousePosition;
        Ray ray = mainCam.ScreenPointToRay(inputPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.transform == transform;
        }
        return false;
    }

    // -----------------------------------------------------------
    void ApplyRotation()
    {
        if (!enableRotation) return;

        if (rotateX) currentRotation.x += rotationVelocity.x;
        if (rotateY) currentRotation.y += rotationVelocity.y;
        if (rotateZ) currentRotation.z += rotationVelocity.z;

        if (limitRotation)
        {
            currentRotation.x = Mathf.Clamp(currentRotation.x, minRotation.x, maxRotation.x);
            currentRotation.y = Mathf.Clamp(currentRotation.y, minRotation.y, maxRotation.y);
            currentRotation.z = Mathf.Clamp(currentRotation.z, minRotation.z, maxRotation.z);
        }

        transform.rotation = Quaternion.Euler(currentRotation);
    }

    // -----------------------------------------------------------
    void ApplyMovement()
    {
        if (!enableMovement) return;

        Vector3 newPos = transform.position;

        if (moveX) newPos.x += moveVelocity.x;
        if (moveY) newPos.y += moveVelocity.y;
        if (moveZ) newPos.z += moveVelocity.z;

        if (limitMovement)
        {
            newPos.x = Mathf.Clamp(newPos.x, minPosition.x, maxPosition.x);
            newPos.y = Mathf.Clamp(newPos.y, minPosition.y, maxPosition.y);
            newPos.z = Mathf.Clamp(newPos.z, minPosition.z, maxPosition.z);
        }

        transform.position = newPos;
    }
}
