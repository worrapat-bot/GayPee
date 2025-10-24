using UnityEngine;
using System.Collections; // *** ต้องเพิ่มบรรทัดนี้เพื่อแก้ Error CS0246 ***

// *******************************************************************
// คำแนะนำ: ติดสคริปต์นี้กับ Player GameObject
// *******************************************************************

public class PlayerCameraControl : MonoBehaviour
{
    // *** NEW: Field สำหรับกำหนดมุมกล้องใหม่เมื่อโดนตี ***
    [Header("Camera Control Settings")]
    [Tooltip("ตำแหน่งและมุมกล้องที่ต้องการเปลี่ยนไปเมื่อโดนศัตรูชน (เช่น Empty GameObject)")]
    public Transform hitCameraView;

    [Tooltip("ระยะเวลาที่ใช้ในการเปลี่ยนมุมกล้อง (Fade/Transition Time)")]
    public float cameraTransitionDuration = 0.5f;

    // ตัวแปรสำหรับเช็คสถานะ
    public bool isHitByEnemy { get; private set; } = false;

    // ตัวแปรที่เก็บกล้องหลักของ Scene
    private Camera mainCamera;
    
    // ตั้งค่าเริ่มต้น
    private void Start()
    {
        mainCamera = Camera.main; // อ้างอิงถึงกล้องหลัก
        
        // ตรวจสอบให้แน่ใจว่า Time.timeScale ถูกรีเซ็ตเมื่อ Scene โหลด
        if (Time.timeScale != 1.0f)
        {
            Time.timeScale = 1.0f;
        }
    }

    // เมธอดนี้จะถูกเรียกเมื่อ Collider ผู้เล่น (ที่เป็น Trigger) ชนกับ Collider อื่น
    private void OnTriggerEnter(Collider other)
    {
        // 1. ตรวจสอบว่าชนกับศัตรู (สมมติว่า Enemy GameObject มี Tag เป็น "Enemy")
        if (other.CompareTag("Enemy") && !isHitByEnemy)
        {
            // 2. ล็อกสถานะว่าโดนโจมตีแล้ว
            isHitByEnemy = true;
            Debug.Log("Player was hit by enemy! Switching camera view.");
            
            // 3. เริ่ม Coroutine เพื่อเปลี่ยนมุมกล้องและรอการ Freeze Time
            StartCoroutine(HitSequence());
        }
    }
    
    // Coroutine ลำดับการโดนตี
    private IEnumerator HitSequence()
    {
        // 1. เปลี่ยนมุมกล้องไปยังตำแหน่งที่กำหนด
        if (mainCamera != null && hitCameraView != null)
        {
            // ใช้ Lerp เพื่อให้การเปลี่ยนมุมกล้องราบรื่น
            yield return StartCoroutine(TransitionCameraView(hitCameraView.position, hitCameraView.rotation, cameraTransitionDuration));
        }

        // 2. จัดการ Freeze Time Effects
        // รอนานพอที่ EnemyPatrol จะสั่ง Time.timeScale = 0f (รอ 1 เฟรม)
        yield return null; 

        if (Time.timeScale == 0f)
        {
            Debug.Log("Game time is frozen. Camera view locked and ready for Jumpscare/Fade.");
            // ณ จุดนี้ กล้องจะถูกล็อคตามตำแหน่ง hitCameraView และเกมจะถูก Freeze
            
            // หากมี UI Fade Screen ต้องเริ่ม Fade ใน Unscaled Time ที่นี่
        }
    }

    // Coroutine สำหรับการเปลี่ยนมุมกล้องอย่างราบรื่น
    private IEnumerator TransitionCameraView(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;
        float startTime = Time.time;

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            // ใช้ SmoothStep เพื่อให้การเคลื่อนไหวดูเป็นธรรมชาติมากขึ้น
            t = t * t * (3f - 2f * t); 
            
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        // ตั้งค่าให้เป๊ะๆ เมื่อจบ
        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;
    }
}