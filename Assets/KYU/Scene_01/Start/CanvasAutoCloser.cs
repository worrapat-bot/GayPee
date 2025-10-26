using UnityEngine;
using System.Collections; // สำคัญ: ต้องใช้สำหรับ Coroutine

// คลาสที่จัดการการหน่วงเวลาเพื่อปิด Game Object (Canvas/Panel)
public class CanvasAutoCloser : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("เวลาหน่วงเป็นวินาที (ตั้งค่าเป็น 30.0f ตามที่ต้องการ)")]
    [SerializeField] private float delayTime = 30f; 

    // เมธอด Start ถูกเรียกครั้งเดียวเมื่อ Game Object เริ่มทำงาน
    private void Start()
    {
        // 1. เริ่ม Coroutine เพื่อหน่วงเวลา
        StartCoroutine(CloseCanvasAfterDelay(delayTime));
    }

    // Coroutine: ฟังก์ชันสำหรับรอเวลาโดยไม่บล็อกเกม
    private IEnumerator CloseCanvasAfterDelay(float delay)
    {
        Debug.Log($"Canvas close timer started. Will close in {delay} seconds.");
        
        // รอเป็นระยะเวลา delay ที่กำหนด (30 วินาที)
        yield return new WaitForSeconds(delay);
        
        // เมื่อรอครบ 30 วินาทีแล้ว...
        
        // 2. ปิด (ซ่อน) Game Object ที่ Script นี้แนบอยู่
        // การใช้ gameObject.SetActive(false) จะซ่อนทั้ง Canvas/Panel นั้นๆ
        gameObject.SetActive(false);
        
        Debug.Log("30 seconds passed. Canvas/Panel has been deactivated.");
    }

    // หลักการ OOP: Public method สำหรับให้คลาสอื่นสั่งเปิดใช้งาน Canvas นี้ได้
    public void OpenCanvas()
    {
        gameObject.SetActive(true);
        // หากต้องการให้เริ่มนับเวลาใหม่ทันทีที่เปิด
        StopAllCoroutines();
        StartCoroutine(CloseCanvasAfterDelay(delayTime));
    }
}