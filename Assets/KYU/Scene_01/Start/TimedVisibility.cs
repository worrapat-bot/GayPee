using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class TimedVisibility : MonoBehaviour
{
    [Header("Behavior Toggle")]
    [Tooltip("ติ๊กเครื่องหมายนี้ ถ้าคุณต้องการให้ UI นี้ทำงานอัตโนมัติ")]
    [SerializeField] private bool enableTimedBehavior = true;
    
    // *** [เพิ่มใหม่] ติ๊กเพื่อไม่ให้หายไป ***
    [Tooltip("ติ๊กเครื่องหมายนี้ ถ้าคุณต้องการให้ UI นี้ 'ค้างอยู่' และ 'ไม่หายไป' หลังจากแสดงผลแล้ว")]
    [SerializeField] private bool preventFadeOut = false; // ค่าเริ่มต้นคือ "หายไป"

    [Header("Timing Settings")]
    [Tooltip("ระยะเวลาที่ 'หายไป' ตอนเริ่มเกม (วินาที)")]
    [SerializeField] private float initialDelay = 10f; // 10 วิ

    [Tooltip("ระยะเวลาที่ 'ปรากฏ' ให้เห็น (วินาที)")]
    [SerializeField] private float visibleDuration = 5f; // 5 วิ

    [Tooltip("ความเร็วในการ Fade In และ Fade Out (วินาที)")]
    [SerializeField] private float fadeDuration = 1f;

    private CanvasGroup canvasGroup;
    private Coroutine runningCoroutine;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    void Start()
    {
        if (enableTimedBehavior)
        {
            StartVisibilitySequence();
        }
    }

    public void StartVisibilitySequence()
    {
        if (runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
        }
        runningCoroutine = StartCoroutine(RunVisibilitySequence());
    }

    private IEnumerator RunVisibilitySequence()
    {
        // --- 1. หายไป 10 วินาที (ตอนเริ่ม) ---
        yield return new WaitForSeconds(initialDelay);

        // --- 2. ปรากฏ (Fade In) ---
        yield return StartCoroutine(Fade(1f, fadeDuration));

        // --- 3. ค้างไว้ 5 วินาที ---
        yield return new WaitForSeconds(visibleDuration);

        // --- 4. หายไป (Fade Out) ---
        // *** [แก้ไข] ตรวจสอบติ๊กก่อนที่จะ Fade Out ***
        if (!preventFadeOut) // ถ้า "ไม่ได้" ติ๊ก preventFadeOut
        {
            Debug.Log(gameObject.name + ": Fading Out");
            yield return StartCoroutine(Fade(0f, fadeDuration)); // 0f = Alpha 0%
        }
        else
        {
            Debug.Log(gameObject.name + ": Staying visible (Fade Out prevented).");
            // ไม่ต้องทำอะไร ปล่อยให้มันค้างที่ Alpha 1
        }
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float timer = 0f;

        while (timer < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            timer += Time.deltaTime;
            yield return null; 
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = (targetAlpha > 0.5f);
        canvasGroup.blocksRaycasts = (targetAlpha > 0.5f);
    }
}