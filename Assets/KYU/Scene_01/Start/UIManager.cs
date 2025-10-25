using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // =================================================================
    // 1. REFERENCES & CONFIGURATION
    // =================================================================

    [Header("Web Browser (IE)")]
    // *** [แก้ไข] เปลี่ยนจาก GameObject เป็น CanvasGroup ***
    [Tooltip("Panel หลักของ Internet Explorer (ต้องมี CanvasGroup)")]
    [SerializeField] private CanvasGroup ieWebPagePanelGroup;

    [Header("Email App - Main Containers")]
    [Tooltip("Panel หลักของ Email Application (Parent)")]
    [SerializeField] private GameObject emailAppPanel;
    
    // ... (References ส่วนที่เหลือของ Email เหมือนเดิม) ...
    [Header("Email App - State UI References")]
    [SerializeField] private CanvasGroup state1Group; 
    [SerializeField] private GameObject state2Panel;
    [SerializeField] private GameObject state3Panel; 
    [Header("Email App - State 3 Content")]
    [SerializeField] private TextMeshProUGUI emailBodyText;
    [SerializeField] private Button nextPageButton;
    [TextArea(3, 10)] 
    [SerializeField] private string emailMessage = "Welcome to your new inbox!";
    [Header("Email App - Timing and Animation")]
    [SerializeField] private float delayBeforeFade = 10f;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float delayBeforeLineNotif = 5f;
    [SerializeField] private float lineNotifDuration = 3f;

    private Coroutine emailSequenceCoroutine;

    // =================================================================
    // 2. AWAKE / START
    // =================================================================

    void Awake()
    {
        // *** [แก้ไข] ปิด IE Panel Group ด้วย ***
        if (ieWebPagePanelGroup != null)
        {
            ieWebPagePanelGroup.alpha = 0f;
            ieWebPagePanelGroup.interactable = false;
            ieWebPagePanelGroup.blocksRaycasts = false;
            ieWebPagePanelGroup.gameObject.SetActive(false); // ปิด GameObject ไปเลย
        }
        
        // ... (โค้ด Awake() ส่วนที่เหลือของ Email เหมือนเดิม) ...
        if (emailAppPanel != null) emailAppPanel.SetActive(false);
        if (state1Group != null) state1Group.alpha = 0f;
        if (state2Panel != null) state2Panel.SetActive(false);
        if (state3Panel != null) state3Panel.SetActive(false);
        if (nextPageButton != null) nextPageButton.gameObject.SetActive(false);
    }
    
    void Start()
    {
        OpenEmailApp();
    }
    
    // =================================================================
    // 3. PUBLIC METHODS FOR BUTTONS (ส่วนที่แก้ไข)
    // =================================================================

    // --- Internet Explorer ---
    public void OpenInternetExplorer()
    {
        if (ieWebPagePanelGroup == null) {
             Debug.LogError("IE Web Page Panel Group is NOT assigned!");
             return;
        }

        // *** [แก้ไข] 1. ปิด Email ก่อนเปิด Web (แก้ปัญหาซ้อนกัน) ***
        CloseEmailApp(); 

        // *** [แก้ไข] 2. เริ่ม Coroutine เพื่อ Fade In หน้า Web ***
        ieWebPagePanelGroup.gameObject.SetActive(true);
        StartCoroutine(FadeGroup(ieWebPagePanelGroup, 0f, 1f, fadeDuration)); // ใช้ FadeGroup() ซ้ำได้
        ieWebPagePanelGroup.interactable = true;
        ieWebPagePanelGroup.blocksRaycasts = true;
        
        Debug.Log("Internet Explorer Opened.");
    }

    public void CloseInternetExplorer()
    {
        if (ieWebPagePanelGroup != null)
        {
            // ปิดทันที (ถ้าอยากให้ Fade Out ต้องใช้ Coroutine)
            ieWebPagePanelGroup.alpha = 0f;
            ieWebPagePanelGroup.interactable = false;
            ieWebPagePanelGroup.blocksRaycasts = false;
            ieWebPagePanelGroup.gameObject.SetActive(false);
        }
    }

    // --- Email App Control ---
    public void OpenEmailApp()
    {
        if (emailAppPanel == null) {
            Debug.LogError("FATAL ERROR: Email App Panel is NOT assigned!");
            return;
        }

        // *** [แก้ไข] ปิด IE ก่อนเปิด Email (แก้ปัญหาซ้อนกัน) ***
        CloseInternetExplorer(); 

        emailAppPanel.SetActive(true);
        if (emailSequenceCoroutine != null) StopCoroutine(emailSequenceCoroutine);
        
        // ... (Reset State ของ Email) ...
        if (state1Group != null) state1Group.alpha = 0f;
        if (state2Panel != null) state2Panel.SetActive(false);
        if (state3Panel != null) state3Panel.SetActive(false);
        if (nextPageButton != null) nextPageButton.gameObject.SetActive(false);
        
        emailSequenceCoroutine = StartCoroutine(RunEmailSequence());
    }

    public void CloseEmailApp()
    {
        if (emailSequenceCoroutine != null) StopCoroutine(emailSequenceCoroutine);
        if (emailAppPanel != null) emailAppPanel.SetActive(false);
    }
    
    public void GoToNextPage()
    {
        CloseEmailApp();
    }

    // ... (ส่วนที่ 4. EMAIL STATE COROUTINE - เหมือนเดิม) ...
    private IEnumerator RunEmailSequence()
    {
        Debug.Log("STATE 1: Waiting to Fade In (" + delayBeforeFade + "s)");
        yield return new WaitForSeconds(delayBeforeFade);

        if (state1Group != null)
        {
            yield return StartCoroutine(FadeGroup(state1Group, 0f, 1f, fadeDuration));
            Debug.Log("STATE 1: Fade In Complete.");
        }

        Debug.Log("STATE 2: Waiting for Line Notification (" + delayBeforeLineNotif + "s)");
        yield return new WaitForSeconds(delayBeforeLineNotif);
        
        if (state1Group != null) state1Group.alpha = 0f; 

        if (state2Panel != null) 
        {
            state2Panel.SetActive(true);
            yield return new WaitForSeconds(lineNotifDuration);
            state2Panel.SetActive(false);
        }

        Debug.Log("STATE 3: Displaying Email Content.");
        if (state3Panel != null) state3Panel.SetActive(true);
        if (emailBodyText != null) emailBodyText.text = emailMessage;
        if (nextPageButton != null) nextPageButton.gameObject.SetActive(true);
    }
    
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
        group.alpha = endAlpha;
        // เปิด/ปิดการคลิกที่นี่ (ถ้าต้องการ)
        group.interactable = (endAlpha > 0.5f);
        group.blocksRaycasts = (endAlpha > 0.5f);
    }
}