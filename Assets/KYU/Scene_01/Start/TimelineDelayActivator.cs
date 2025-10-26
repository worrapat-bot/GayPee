using UnityEngine;
using UnityEngine.Playables; // สำคัญ: ต้องใช้สำหรับ PlayableDirector (Timeline)
using System.Collections; // สำคัญ: ต้องใช้สำหรับ Coroutine

// คลาสที่จัดการการหน่วงเวลาและเริ่มเล่น Timeline
public class TimelineDelayActivator : MonoBehaviour
{
    [Header("Timeline & Timer Settings")]
    [Tooltip("Playable Director Component ที่ควบคุม Timeline")]
    // ต้องลาก PlayableDirector มาใส่ใน Inspector
    [SerializeField] private PlayableDirector targetTimeline;
    
    [Tooltip("เวลาหน่วงเป็นวินาที (ตั้งค่าเป็น 30.0f ตามที่ต้องการ)")]
    [SerializeField] private float delayTime = 30f; 

    // เมธอด Start ถูกเรียกครั้งเดียวเมื่อ Game Object เริ่มทำงาน
    private void Start()
    {
        // 1. ตรวจสอบความถูกต้องของ Component
        if (targetTimeline == null)
        {
            Debug.LogError("Target PlayableDirector (Timeline) is not assigned in the Inspector!");
            return; // หยุดการทำงานถ้าหา Timeline ไม่เจอ
        }
        
        // 2. ให้แน่ใจว่า Timeline อยู่ในสถานะหยุดก่อนเริ่ม (Optional: ป้องกันการเล่นเอง)
        targetTimeline.Stop();

        // 3. เริ่ม Coroutine เพื่อหน่วงเวลา
        StartCoroutine(StartTimelineAfterDelay(delayTime));
    }

    // Coroutine: ฟังก์ชันสำหรับรอเวลาโดยไม่บล็อกเกม
    private IEnumerator StartTimelineAfterDelay(float delay)
    {
        Debug.Log($"Timeline timer started. Waiting for {delay} seconds...");
        
        // รอเป็นระยะเวลา delay ที่กำหนด
        yield return new WaitForSeconds(delay);
        
        // เมื่อรอครบ 30 วินาทีแล้ว...
        
        // 4. สั่งให้ Timeline (PlayableDirector) เริ่มเล่น
        targetTimeline.Play();
        
        Debug.Log("30 seconds passed. Starting Timeline playback!");
    }
    
    // หลักการ OOP: Public method เผื่อคลาสอื่นต้องการเรียกให้เริ่ม Timeline ทันที
    public void PlayTimelineImmediately()
    {
        if (targetTimeline != null)
        {
            targetTimeline.Play();
            StopAllCoroutines(); // หยุด Coroutine การหน่วงเวลาถ้ามี
        }
    }
}