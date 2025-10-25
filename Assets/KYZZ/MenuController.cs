using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour
{
    [Header("📋 Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    [Header("🎮 Main Menu Buttons")]
    public Button startButton;
    public Button settingsButton;
    public Button exitButton;

    [Header("⚙️ Settings Buttons")]
    public Button backButton;
    public Slider volumeSlider;
    public Toggle fullscreenToggle;

    [Header("🎯 Scene Settings")]
    public string gameSceneName = "GameScene";

    [Header("🎵 Audio")]
    public AudioSource menuMusicSource;
    public AudioClip buttonClickSound;
    public AudioClip buttonHoverSound;

    [Header("👻 Horror Effects")]
    public float buttonHoverScale = 1.1f;
    public float buttonScaleSpeed = 10f;
    public Color normalTextColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    public Color hoverTextColor = new Color(1f, 0.2f, 0.2f, 1f); // แดงเลือด
    public bool enableGlitchEffect = true;
    public bool enableShakeEffect = true;

    void Start()
    {
        ShowMainMenu();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartGame);
            AddHoverEffects(startButton);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnOpenSettings);
            AddHoverEffects(settingsButton);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitGame);
            AddHoverEffects(exitButton);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackToMenu);
            AddHoverEffects(backButton);
        }

        SetupSettings();
    }

    void ShowMainMenu()
    {
        // รีเซ็ตปุ่มทั้งหมดก่อนเปลี่ยน Panel
        ResetAllButtons();

        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void OnStartGame()
    {
        PlayButtonClick();
        Debug.Log("🎮 Starting Game...");

        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("❌ Game Scene Name is not set!");
        }
    }

    void OnOpenSettings()
    {
        PlayButtonClick();

        // รีเซ็ตปุ่มทั้งหมดก่อนเปลี่ยน Panel
        ResetAllButtons();

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    void OnExitGame()
    {
        PlayButtonClick();
        Debug.Log("👋 Exiting Game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    void SetupSettings()
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", AudioListener.volume);
            AudioListener.volume = volumeSlider.value;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
        }
    }

    void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    void OnFullscreenToggled(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    void OnBackToMenu()
    {
        PlayButtonClick();

        // รีเซ็ตปุ่ม Back ก่อนกลับ Main Menu
        if (backButton != null)
        {
            HorrorButtonEffect effect = backButton.GetComponent<HorrorButtonEffect>();
            if (effect != null)
            {
                effect.ForceReset();
            }
        }

        ShowMainMenu();
    }

    // รีเซ็ตปุ่มทั้งหมด
    void ResetAllButtons()
    {
        Button[] allButtons = new Button[] { startButton, settingsButton, exitButton, backButton };

        foreach (Button btn in allButtons)
        {
            if (btn != null)
            {
                HorrorButtonEffect effect = btn.GetComponent<HorrorButtonEffect>();
                if (effect != null)
                {
                    effect.ForceReset();
                }
            }
        }
    }

    void PlayButtonClick()
    {
        if (buttonClickSound != null && menuMusicSource != null)
        {
            menuMusicSource.PlayOneShot(buttonClickSound, 0.7f);
        }
    }

    void PlayButtonHover()
    {
        if (buttonHoverSound != null && menuMusicSource != null)
        {
            menuMusicSource.PlayOneShot(buttonHoverSound, 0.4f);
        }
    }


    // ==================== 👻 HORROR HOVER EFFECTS ====================
    void AddHoverEffects(Button button)
    {
        if (button == null) return;

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        // Pointer Enter (เมื่อเมาส์เข้า)
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { OnButtonHoverEnter(button); });
        trigger.triggers.Add(entryEnter);

        // Pointer Exit (เมื่อเมาส์ออก)
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { OnButtonHoverExit(button); });
        trigger.triggers.Add(entryExit);

        // เพิ่ม Component สำหรับ Animation
        HorrorButtonEffect effect = button.gameObject.GetComponent<HorrorButtonEffect>();
        if (effect == null)
            effect = button.gameObject.AddComponent<HorrorButtonEffect>();

        effect.Initialize(this);
    }

    void OnButtonHoverEnter(Button button)
    {
        PlayButtonHover();

        HorrorButtonEffect effect = button.GetComponent<HorrorButtonEffect>();
        if (effect != null)
        {
            effect.OnHoverEnter();
        }
    }

    void OnButtonHoverExit(Button button)
    {
        HorrorButtonEffect effect = button.GetComponent<HorrorButtonEffect>();
        if (effect != null)
        {
            effect.OnHoverExit();
        }
    }
}

// ==================== 👻 HORROR BUTTON EFFECT COMPONENT ====================
public class HorrorButtonEffect : MonoBehaviour
{
    private MainMenuController menuController;
    private TextMeshProUGUI buttonText;
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private bool isHovering = false;
    private float glitchTimer = 0f;
    private float shakeTimer = 0f;

    public void Initialize(MainMenuController controller)
    {
        menuController = controller;
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        originalPosition = rectTransform.localPosition;

        if (buttonText != null)
        {
            buttonText.color = menuController.normalTextColor;
        }
    }

    public void OnHoverEnter()
    {
        isHovering = true;
    }

    public void OnHoverExit()
    {
        isHovering = false;
        ForceReset();
    }

    public void ForceReset()
    {
        isHovering = false;

        // รีเซ็ตกลับสู่สภาพเดิม
        if (rectTransform != null)
        {
            rectTransform.localScale = originalScale;
            rectTransform.localPosition = originalPosition;
        }

        if (buttonText != null)
        {
            buttonText.color = menuController.normalTextColor;
            buttonText.alpha = 1f;
        }

        glitchTimer = 0f;
        shakeTimer = 0f;
    }

    void Update()
    {
        if (menuController == null || rectTransform == null) return;

        if (isHovering)
        {
            // 1. ขยายขนาดปุ่ม (Smooth Scale)
            Vector3 targetScale = originalScale * menuController.buttonHoverScale;
            rectTransform.localScale = Vector3.Lerp(
                rectTransform.localScale,
                targetScale,
                Time.deltaTime * menuController.buttonScaleSpeed
            );

            // 2. เปลี่ยนสีตัวอักษรเป็นสีแดงเลือด
            if (buttonText != null)
            {
                buttonText.color = Color.Lerp(
                    buttonText.color,
                    menuController.hoverTextColor,
                    Time.deltaTime * 5f
                );
            }

            // 3. Glitch Effect (กระพริบแบบสุ่ม)
            if (menuController.enableGlitchEffect)
            {
                glitchTimer += Time.deltaTime;
                if (glitchTimer > Random.Range(0.1f, 0.3f))
                {
                    if (buttonText != null)
                    {
                        // สุ่มซ่อน-แสดงตัวอักษร
                        buttonText.alpha = Random.Range(0.7f, 1f);
                    }
                    glitchTimer = 0f;
                }
            }

            // 4. Shake Effect (สั่นเล็กน้อย)
            if (menuController.enableShakeEffect)
            {
                shakeTimer += Time.deltaTime * 20f;
                float shakeX = Mathf.Sin(shakeTimer) * 2f;
                float shakeY = Mathf.Cos(shakeTimer * 1.5f) * 1f;
                rectTransform.localPosition = originalPosition + new Vector3(shakeX, shakeY, 0);
            }
        }
        else
        {
            // รีเซ็ต Alpha เมื่อไม่ Hover
            if (buttonText != null && buttonText.alpha < 1f)
            {
                buttonText.alpha = Mathf.Lerp(buttonText.alpha, 1f, Time.deltaTime * 5f);
            }
        }
    }
}

/* 
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📋 คำแนะนำการใช้งาน (ฟ้อนสไตล์สยองขวัญ)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

✨ ลูกเล่น Horror ที่เพิ่มเข้ามา:
1. 🔴 เปลี่ยนสีตัวอักษรเป็นแดงเลือดตอน Hover
2. 📏 ขยายขนาดปุ่มแบบ Smooth
3. ⚡ Glitch Effect - กระพริบแบบสุ่ม (ซ่อน-แสดง)
4. 💥 Shake Effect - สั่นเล็กน้อย
5. 🎵 เสียงประกอบ Hover และ Click

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

⚙️ ปรับแต่งได้ใน Inspector:
- Button Hover Scale: ขนาดขยาย (1.1 = 110%)
- Button Scale Speed: ความเร็วการขยาย
- Normal Text Color: สีปกติ (เทา)
- Hover Text Color: สีตอน Hover (แดงเลือด)
- Enable Glitch Effect: เปิด/ปิด Glitch
- Enable Shake Effect: เปิด/ปิด Shake

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🎨 แนะนำฟ้อนต์สยองขวัญ:
- "Creepster" (Google Fonts)
- "Nosifer"
- "Butcherman"
- หรือใช้ Font มีเลือดหยด

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🔊 แนะนำเสียงประกอบ:
- Button Click: เสียงกระดูกหัก, ประตูเอี้ยด
- Button Hover: เสียงกระซิบ, ลมพัด
- Background Music: เสียงเปียโน บรรยากาศหลอน

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
*/