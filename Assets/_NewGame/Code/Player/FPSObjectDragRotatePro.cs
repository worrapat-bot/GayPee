using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FPSObjectDragRotatePro : MonoBehaviour
{
    [Header("=== DRAG SETTINGS ===")]
    public float pickupRange = 3f;
    public float moveSmooth = 10f;
    public float rotationSmooth = 10f;
    public float holdDistance = 2.0f;

    [Header("=== ROTATION SETTINGS ===")]
    public float rotationSpeed = 120f;
    public bool rotateX = true;
    public bool rotateY = true;
    public bool rotateZ = false;

    [Header("=== AUDIO SETTINGS ===")]
    public AudioClip pickupSound;
    public AudioClip dropSound;
    private AudioSource audioSource;

    private Camera playerCam;
    private Rigidbody heldObject;
    private Transform holdPoint;
    private Vector3 lastMousePos;
    private bool isHolding = false;

    void Start()
    {
        playerCam = Camera.main;
        if (playerCam == null)
        {
            Debug.LogError("❌ ไม่พบ MainCamera! กรุณาตั้ง Tag ให้กล้อง FPS", this);
            enabled = false;
            return;
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // ✅ จุดถือวัตถุ (อยู่หน้ากล้อง)
        GameObject hold = new GameObject("HoldPoint");
        hold.transform.parent = playerCam.transform;
        hold.transform.localPosition = new Vector3(0, 0, holdDistance);
        holdPoint = hold.transform;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!isHolding)
                TryPickup();
            else
                DropObject();
        }

        if (isHolding && heldObject != null)
        {
            MoveHeldObject();

            if (Input.GetMouseButton(1)) // คลิกขวา = หมุน
                RotateHeldObject();
        }

        lastMousePos = Input.mousePosition;
    }

    // ===========================================================
    void TryPickup()
    {
        Ray ray = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            Rigidbody rb = hit.rigidbody;
            if (rb == null) return;

            // ✅ ตรวจสอบว่าเป็นวัตถุที่ตั้งใจให้หยิบได้เท่านั้น
            bool canPickup = hit.collider.CompareTag("Pickup") || hit.collider.GetComponent<PickupItem>() != null;

            if (!canPickup)
                return;

            heldObject = rb;
            heldObject.useGravity = false;
            heldObject.drag = 10f;
            isHolding = true;
            PlaySound(pickupSound);
        }
    }

    // ===========================================================
    void MoveHeldObject()
    {
        if (heldObject == null) return;

        Vector3 targetPos = holdPoint.position;
        Vector3 newPos = Vector3.Lerp(heldObject.position, targetPos, Time.deltaTime * moveSmooth);
        heldObject.MovePosition(newPos);
    }

    void RotateHeldObject()
    {
        if (heldObject == null) return;

        Vector3 mouseDelta = Input.mousePosition - lastMousePos;
        float rotX = rotateX ? -mouseDelta.y * rotationSpeed * Time.deltaTime : 0f;
        float rotY = rotateY ? mouseDelta.x * rotationSpeed * Time.deltaTime : 0f;
        float rotZ = rotateZ ? (mouseDelta.x + mouseDelta.y) * rotationSpeed * Time.deltaTime * 0.5f : 0f;

        Quaternion targetRot = Quaternion.Euler(rotX, rotY, rotZ) * heldObject.rotation;
        heldObject.MoveRotation(Quaternion.Lerp(heldObject.rotation, targetRot, Time.deltaTime * rotationSmooth));
    }

    void DropObject()
    {
        if (heldObject == null) return;

        heldObject.useGravity = true;
        heldObject.drag = 0f;
        heldObject = null;
        isHolding = false;
        PlaySound(dropSound);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }
}
