// SceneFaderManager.cs (Final Fix)
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using System.Collections;
using System; 

public class SceneFaderManager : MonoBehaviour
{
    public static SceneFaderManager Instance { get; private set; }

    [Header("Fade UI")]
    [Tooltip("ลาก Image Component ที่ใช้เป็นหน้ากากสีดำมาใส่")]
    public Image fadeImage;

    [Header("Settings")]
    public float fadeDuration = 0.5f;

    [Header("Next Scene Settings")]
    [SerializeField] 
    private string defaultTargetSceneName = "MainMenu"; 
    
    private bool isFading = false; 

    private void Awake()
    {
        // 1. Singleton Setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 2. Initial Setup (ทำแค่ถ้า Image ถูกกำหนดแล้ว)
        if (fadeImage != null)
        {
            // ตั้งค่าให้ Image Active เสมอ
            fadeImage.gameObject.SetActive(true);
            // ตั้งค่าสีดำทึบใน Awake() เพื่อป้องกันฉากโผล่มาก่อน FadeIn
            fadeImage.color = new Color(0, 0, 0, 1); 
        }
    }

    void Start()
    {
        // 3. เริ่ม Fade In ใน Start()
        if (fadeImage != null)
        {
            // ตรวจสอบสถานะการเริ่มต้น: ถ้าเป็นสีดำทึบอยู่ ให้เริ่ม Fade In
            if (fadeImage.color.a == 1f)
            {
                StartCoroutine(FadeIn());
                Debug.Log("✅ Fader: Starting initial Fade In from black.");
            }
            else
            {
                // กรณีที่ Scene ถูกโหลดแบบ Additive หรือตั้งค่า Alpha เป็น 0 มาแต่แรก
                fadeImage.color = new Color(0, 0, 0, 0);
            }
        }
        else
        {
            Debug.LogError("🚨 FADE IMAGE MISSING: Image is not assigned. Cannot start initial FadeIn.");
        }
    }
    
    // ... (OnDestroy() ถูกเรียก) ...
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 4. On Scene Loaded Event: เริ่ม Fade In ใหม่
        if (fadeImage != null)
        {
            // ตรวจสอบ: ให้แน่ใจว่ามันดำทึบก่อนที่จะเริ่ม Fade In อีกครั้ง
            fadeImage.color = new Color(0, 0, 0, 1);
            StartCoroutine(FadeIn());
        }
        isFading = false;
    }
    
    // ----------------------------------------------------------------------
    // I. ฟังก์ชัน Fade 
    // ----------------------------------------------------------------------

    private IEnumerator FadeOutCoroutine()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            // เปลี่ยน Alpha จาก 0 (ใส) -> 1 (ทึบ)
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 1);
    }
    
    private IEnumerator FadeIn()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
            // เปลี่ยน Alpha จาก 1 (ทึบ) -> 0 (ใส)
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 0);
    }

    // ----------------------------------------------------------------------
    // II. ฟังก์ชันเปลี่ยนฉาก (ผูกกับปุ่ม OnClick)
    // ----------------------------------------------------------------------

    public void ChangeScene(string sceneName = null)
    {
        if (fadeImage == null)
        {
             Debug.LogError("🚨 FADE IMAGE MISSING: Please assign the Fade Image in the Inspector before calling ChangeScene.");
             return;
        }
        if (isFading) return; 

        string targetScene = string.IsNullOrEmpty(sceneName) ? defaultTargetSceneName : sceneName;

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("Cannot change scene: Target Scene Name is null or empty.");
            return;
        }
        
        StartCoroutine(LoadSceneWithFade(targetScene));
    }

    private IEnumerator LoadSceneWithFade(string sceneToLoad)
    {
        isFading = true;

        yield return FadeOutCoroutine();

        SceneManager.LoadScene(sceneToLoad);
    }
}