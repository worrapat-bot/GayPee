// CameraManager.cs
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Encapsulation: ใช้ private field และ public property หรือ method
    [SerializeField]
    private Camera mainCamera; // กล้องหลักในฉาก

    // Static instance เพื่อให้ Class อื่นเรียกใช้ได้ง่าย (Singleton Pattern แบบง่าย)
    public static CameraManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main; // ตรวจสอบและกำหนดกล้องหลักโดยอัตโนมัติ
        }
    }

    /// <summary>
    /// เปลี่ยนตำแหน่งกล้องหลักไปยังตำแหน่งของ Transform ที่กำหนด
    /// </summary>
    /// <param name="targetTransform">Transform ของ Object ที่ต้องการให้กล้องไปหา</param>
    public void MoveCameraTo(Transform targetTransform)
    {
        if (mainCamera != null)
        {
            // ใช้ Lerp เพื่อให้การเปลี่ยนมุมกล้องดูนุ่มนวล
            // ในการใช้งานจริง คุณอาจต้องการใช้ Coroutine เพื่อควบคุมการเคลื่อนที่
            mainCamera.transform.position = targetTransform.position;
            mainCamera.transform.rotation = targetTransform.rotation;

            // Debug Log สำหรับตรวจสอบการทำงาน
            Debug.Log("Camera moved to: " + targetTransform.gameObject.name);
        }
    }

    // *คุณสามารถเพิ่มเมธอดสำหรับการเปลี่ยนกลับ หรือ Animation อื่นๆ ที่นี่*
}