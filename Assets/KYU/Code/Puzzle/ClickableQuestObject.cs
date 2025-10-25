// ClickableQuestObject.cs
using UnityEngine;

public class ClickableQuestObject : CameraManager
{
    [Header("Quest Object Settings")]
    
    // ตัวแปรสำหรับเปิด/ปิดฟังก์ชันนี้ (ติกถูกใน Inspector)
    [Tooltip("เปิด/ปิดการทำงานของการเปลี่ยนมุมกล้องเมื่อคลิก")]
    public bool enableCameraSwitch = true; 

    // Transform ของ Object ที่เป็นจุดตั้งกล้องที่เราต้องการไป
    [Tooltip("HandleCamera ที่กล้องจะเคลื่อนที่ไปหา")]
    [SerializeField]
    private Transform handleCameraTarget;

    // เราจะใช้ Tag เพื่อยืนยันว่าเป็นวัตถุที่ถูกต้อง
    private const string QUEST_TAG = "Quest"; 
    
    // ตรวจสอบว่า Object มี Tag ที่ถูกต้องหรือไม่
    private void Start()
    {
        if (!gameObject.CompareTag(QUEST_TAG))
        {
            Debug.LogWarning($"Object {gameObject.name} does not have the required Tag: {QUEST_TAG}.");
        }
        
        // ตรวจสอบ HandleCameraTarget
        if (handleCameraTarget == null)
        {
             Debug.LogError($"'{gameObject.name}' is missing a 'Handle Camera Target' Transform. Please assign one in the Inspector.");
        }
    }

    // เมธอดที่ถูกเรียกเมื่อมีการคลิก (ต้องมี Collider บน Object นี้)
    private void OnMouseDown()
    {
        // 1. ตรวจสอบ Tag (Polymorphism)
        if (!gameObject.CompareTag(QUEST_TAG))
        {
            return; // ไม่ใช่ Object ที่ต้องการ
        }
        
        // 2. ตรวจสอบสถานะการเปิด/ปิด (ติกถูก)
        if (!enableCameraSwitch)
        {
            Debug.Log("Camera Switch is disabled for this object.");
            return; 
        }

        // 3. ตรวจสอบ CameraManager (Encapsulation/Singleton Call)
        if (CameraManager.Instance == null)
        {
            Debug.LogError("CameraManager Instance is not found. Make sure it's in the scene.");
            return;
        }

        // 4. เปลี่ยนมุมกล้อง
        if (handleCameraTarget != null)
        {
            CameraManager.Instance.MoveCameraTo(handleCameraTarget);
        }
    }
}