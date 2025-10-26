using UnityEngine;
using TMPro; // สำคัญ: ต้องใช้สำหรับ TextMeshPro
using System.Collections;

public class FontZoomEffect : MonoBehaviour
{
    [Header("Component Reference")]
    [Tooltip("TextMeshProUGUI Component ที่จะทำการซูม")]
    [SerializeField] private TextMeshProUGUI targetText;
    
    [Header("Zoom Settings")]
    [Tooltip("ขนาด Font ที่จะเริ่มต้นซูมจาก")]
    [SerializeField] private float startSize = 20f; 
    
    [Tooltip("ขนาด Font ที่จะสิ้นสุดการซูม")]
    [SerializeField] private float endSize = 80f; 
    
    [Tooltip("ระยะเวลาทั้งหมดในการซูม (วินาที)")]
    [SerializeField] private float duration = 1.5f; 

    private void Start()
    {
        // 1. ตรวจสอบ Component และตั้งค่าเริ่มต้น
        if (targetText == null)
        {
            // ลองหา Component ใน GameObject นี้
            targetText = GetComponent<TextMeshProUGUI>();
            if (targetText == null)
            {
                Debug.LogError("TextMeshProUGUI component is missing! Cannot start zoom effect.");
                return;
            }
        }
        
        // ตั้งค่าขนาดเริ่มต้นทันที
        targetText.fontSize = startSize;

        // 2. เริ่ม Coroutine เพื่อทำการซูม
        StartCoroutine(ZoomInText());
    }

    // Coroutine สำหรับทำการซูม Font Size อย่างนุ่มนวล
    private IEnumerator ZoomInText()
    {
        float elapsedTime = 0f; // เวลาที่ผ่านไป
        
        Debug.Log("Starting Font Zoom Effect...");

        // ลูปนี้จะทำงานไปเรื่อยๆ จนกว่าเวลาจะถึง duration
        while (elapsedTime < duration)
        {
            // 1. คำนวณอัตราส่วนความก้าวหน้า (0.0 ถึง 1.0)
            float progress = elapsedTime / duration;
            
            // 2. ใช้ Mathf.Lerp เพื่อหาค่าระหว่าง startSize และ endSize ตาม progress
            // Lerp (Linear Interpolation) ทำให้การเปลี่ยนแปลงเป็นไปอย่างราบรื่น
            float currentSize = Mathf.Lerp(startSize, endSize, progress);
            
            // 3. กำหนดขนาด Font ใหม่
            targetText.fontSize = currentSize;
            
            // 4. อัปเดตเวลาที่ผ่านไป
            elapsedTime += Time.deltaTime;
            
            // 5. หยุดรอ 1 เฟรม (ประมาณ 0.01-0.02 วินาที) ก่อนลูปซ้ำ
            yield return null; 
        }

        // 6. เพื่อความมั่นใจ ให้ตั้งค่าขนาดสุดท้ายเมื่อ Coroutine จบ
        targetText.fontSize = endSize;
        Debug.Log("Font Zoom Effect finished. Final size: " + endSize);
    }
    
    // หลักการ OOP: เมธอด Public สำหรับให้คลาสอื่นสามารถเริ่ม Effect ซ้ำได้
    public void ReplayZoom()
    {
        StopAllCoroutines(); // หยุด Effect เก่าถ้ากำลังทำงาน
        targetText.fontSize = startSize; // รีเซ็ตกลับไปขนาดเริ่มต้น
        StartCoroutine(ZoomInText()); // เริ่ม Effect ใหม่
    }
}