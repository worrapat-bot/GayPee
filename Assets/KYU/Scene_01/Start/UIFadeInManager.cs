using UnityEngine;
using System.Collections;
using UnityEngine.UI; // ต้องใช้สำหรับ Image

public class UIFadeInManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Canvas Group ของรูปภาพแรก (รูปเก่า)")]
    [SerializeField] private CanvasGroup firstImageGroup;
    [Tooltip("Image Component ของรูปภาพที่สอง (รูปใหม่)")]
    [SerializeField] private Image secondImage;
    [Tooltip("Canvas Group ของรูปภาพที่สอง (รูปใหม่)")]
    [SerializeField] private CanvasGroup secondImageGroup;

    [Header("Timing Settings")]
    [Tooltip("ระยะเวลาหน่วงก่อนเริ่ม Fade แรก (วินาที)")]
    [SerializeField] private float initialDelay = 10f; // 10 วินาที
    [Tooltip("ระยะเวลาที่ใช้ในการ Fade In ครั้งแรก (วินาที)")]
    [SerializeField] private float initialFadeDuration = 2f;
    
    [Space]
    [Tooltip("ระยะเวลาหน่วงก่อนเปลี่ยนรูป (วินาที)")]
    [SerializeField] private float swapDelay = 5f; // 5 วินาที
    [Tooltip("ระยะเวลาที่ใช้ในการ Fade-Swap (จางออกและจางเข้า) (วินาที)")]
    [SerializeField] private float swapFadeDuration = 1.5f;

    void Awake()
    {
        // กำหนดสถานะเริ่มต้น: รูปภาพแรก (เก่า) พร้อมแสดง รูปภาพสอง (ใหม่) ซ่อนอยู่
        if (firstImageGroup != null)
        {
            firstImageGroup.alpha = 0f;
            SetGroupInteraction(firstImageGroup, false);
        }
        if (secondImageGroup != null)
        {
            secondImageGroup.alpha = 0f;
            SetGroupInteraction(secondImageGroup, false);
        }
    }

    void Start()
    {
        // เริ่ม Coroutine หลัก
        StartCoroutine(ExecuteFullSequence());
    }

    /// <summary>
    /// กำหนดการโต้ตอบและ Raycast ให้กับ CanvasGroup
    /// </summary>
    private void SetGroupInteraction(CanvasGroup group, bool interactable)
    {
        group.interactable = interactable;
        group.blocksRaycasts = interactable;
    }

    /// <summary>
    /// Coroutine ลำดับเหตุการณ์ทั้งหมด
    /// </summary>
    private IEnumerator ExecuteFullSequence()
    {
        // 1. หน่วงเวลาเริ่มต้น (10 วินาที)
        yield return new WaitForSeconds(initialDelay);

        // 2. Fade In รูปภาพแรก
        yield return StartCoroutine(FadeGroup(firstImageGroup, 0f, 1f, initialFadeDuration));
        
        // เปิดการโต้ตอบสำหรับรูปภาพแรก
        SetGroupInteraction(firstImageGroup, true);

        // ------------------------------------------------------------------

        // 3. หน่วงเวลาระหว่างการแสดงรูป (5 วินาที)
        yield return new WaitForSeconds(swapDelay);

        // 4. Fade Swap: จางรูปแรกออก พร้อมกับจางรูปที่สองเข้า
        yield return StartCoroutine(FadeOutAndIn(firstImageGroup, secondImageGroup, swapFadeDuration));

        // 5. จัดการสถานะการโต้ตอบสุดท้าย
        SetGroupInteraction(firstImageGroup, false); // ปิดการโต้ตอบของรูปเก่า
        SetGroupInteraction(secondImageGroup, true);  // เปิดการโต้ตอบของรูปใหม่
    }

    /// <summary>
    /// Coroutine สำหรับการ Fade In/Out ทั่วไป
    /// </summary>
    private IEnumerator FadeGroup(CanvasGroup group, float startAlpha, float endAlpha, float duration)
    {
        if (group == null || duration <= 0) yield break;

        float timer = 0f;
        while (timer < duration)
        {
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        group.alpha = endAlpha; // ตั้งค่าสุดท้ายให้แน่ใจ
    }

    /// <summary>
    /// Coroutine สำหรับการจางออกของกลุ่มหนึ่ง และจางเข้าของอีกกลุ่มหนึ่งพร้อมกัน
    /// </summary>
    private IEnumerator FadeOutAndIn(CanvasGroup fadeOutGroup, CanvasGroup fadeInGroup, float duration)
    {
        if (fadeOutGroup == null || fadeInGroup == null || duration <= 0) yield break;

        float timer = 0f;
        while (timer < duration)
        {
            float progress = timer / duration;
            
            // Fade Out (จาก 1 เป็น 0)
            fadeOutGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            
            // Fade In (จาก 0 เป็น 1)
            fadeInGroup.alpha = Mathf.Lerp(0f, 1f, progress);

            timer += Time.deltaTime;
            yield return null;
        }
        
        // ตั้งค่าสุดท้าย
        fadeOutGroup.alpha = 0f;
        fadeInGroup.alpha = 1f;
    }
}