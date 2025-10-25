// CameraParentingOnClick.cs (Version แก้ไข)
using UnityEngine;

public class CameraParentingOnClick : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("ชื่อของ GameObject ที่จะใช้เป็นจุดจับกล้อง (Handle) ภายใน Object นี้")]
    private const string CAMERA_HANDLE_NAME = "CamHandle";

    private Transform mainCameraTransform;

    void Start()
    {
        // ค้นหา Transform ของกล้องหลักเพียงครั้งเดียว
        Camera mainCam = Camera.main;

        if (mainCam != null)
        {
            mainCameraTransform = mainCam.transform;
        }
        else
        {
            // Error ที่ชัดเจนถ้าไม่พบกล้องหลัก
            Debug.LogError("⚠️ ERROR: Main Camera not found. Ensure one Camera is tagged 'MainCamera' in the scene.");
        }
    }

    /// <summary>
    /// เมธอดนี้ถูกเรียกเมื่อมีการคลิกที่ Collider ของ Object นี้
    /// </summary>
    private void OnMouseDown()
    {
        if (mainCameraTransform == null)
        {
            Debug.LogError("❌ Camera is not set up correctly (mainCameraTransform is null).");
            return;
        }

        // 1. ค้นหา 'CamHandle' ในฐานะลูก (Child) ของ Object นี้
        Transform camHandle = transform.Find(CAMERA_HANDLE_NAME);

        if (camHandle != null)
        {
            // 2. ย้ายกล้องหลัก (Main Camera) ไปเป็นลูก (Child) ของ CamHandle
            mainCameraTransform.SetParent(camHandle);

            // 3. รีเซ็ตตำแหน่งและการหมุนของกล้องให้ตรงกับ CamHandle
            mainCameraTransform.localPosition = Vector3.zero;
            mainCameraTransform.localRotation = Quaternion.identity;

            Debug.Log($"✅ SUCCESS: Camera moved and parented to: {camHandle.name}");
        }
        else
        {
            Debug.LogError($"❌ ERROR: Cannot find a child GameObject named '{CAMERA_HANDLE_NAME}' inside: {gameObject.name}.");
        }
    }
}