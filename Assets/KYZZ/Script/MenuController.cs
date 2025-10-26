using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("📋 Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject startSubMenuPanel; // ✅ เมนูย่อย New Game / Continue
    public GameObject settingsPanel;

    [Header("🎮 Main Menu Buttons")]
    public Button startButton;
    public Button settingsButton;
    public Button exitButton;

    [Header("🎮 Start SubMenu Buttons")]
    public Button newGameButton;
    public Button continueButton;
    public Button backFromStartButton;

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
    public AudioClip errorSound; // เสียงตอนกด Continue แต่ไม่มีเซฟ

    [Header("👻 Horror Effects")]
    public float buttonHoverScale = 1.1f;
    public float buttonScaleSpeed = 10f;
    public Color normalTextColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    public Color hoverTextColor = new Color(1f, 0.2f, 0.2f, 1f);
    public Color disabledTextColor = new Color(0.3f, 0.3f, 0.3f, 0.5f); // ✅ สีเทาสำหรับปุ่มปิด
    public bool enableGlitchEffect = true;
    public bool enableShakeEffect = true;

    void Start()
    {
        ShowMain();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Main Menu Buttons
        if (startButton != null)
        {
            startButton.onClick.AddListener(OpenStartSubMenu);
            AddHoverEffects(startButton);
        }
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OpenSettings);
            AddHoverEffects(settingsButton);
        }
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
            AddHoverEffects(exitButton);
        }

        // Start SubMenu Buttons
        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(StartNewGame);
            AddHoverEffects(newGameButton);
        }
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueGame);
            AddHoverEffects(continueButton);
        }
        if (backFromStartButton != null)
        {
            backFromStartButton.onClick.AddListener(BackToMain);
            AddHoverEffects(backFromStartButton);
        }

        // Settings Button
        if (backButton != null)
        {
            backButton.onClick.AddListener(BackToMainFromSettings);
            AddHoverEffects(backButton);
        }

        SetupSettings();
    }

    void ShowMain()
    {
        ResetAllButtons();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (startSubMenuPanel != null) startSubMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void OpenStartSubMenu()
    {
        PlayButtonClick();
        ResetAllButtons();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (startSubMenuPanel != null) startSubMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // ✅ เช็คว่ามีเซฟหรือไม่
        UpdateContinueButton();
    }

    void UpdateContinueButton()
    {
        bool hasSave = GameSaveSystem.HasSaveFile();
        Debug.Log($"🔍 Checking save data: {hasSave}");

        if (continueButton != null)
        {
            TextMeshProUGUI buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            HorrorButtonEffect effect = continueButton.GetComponent<HorrorButtonEffect>();

            if (hasSave)
            {
                continueButton.interactable = true;
                if (buttonText != null)
                    buttonText.color = normalTextColor;
                if (effect != null)
                    effect.isDisabled = false;
                Debug.Log("✅ Continue button ENABLED");
            }
            else
            {
                continueButton.interactable = false;
                if (buttonText != null)
                    buttonText.color = disabledTextColor;
                if (effect != null)
                    effect.isDisabled = true;
                Debug.Log("❌ Continue button DISABLED");
            }
        }
    }

    void StartNewGame()
    {
        PlayButtonClick();
        Debug.Log("🎮 Starting New Game...");

        if (!string.IsNullOrEmpty(gameSceneName))
        {
            // ✅ ลบเซฟเก่าก่อนเริ่มเกมใหม่
            GameSaveSystem.DeleteSave();
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("❌ Game Scene Name is not set!");
        }
    }

    void ContinueGame()
    {
        if (!GameSaveSystem.HasSaveFile())
        {
            PlayErrorSound();
            StartCoroutine(ShakeButton(continueButton));
            StartCoroutine(FlashRedText(continueButton));
            return;
        }

        PlayButtonClick();
        Debug.Log("📂 Loading saved game...");
        GameSaveSystem.LoadGame();
    }

    void BackToMain()
    {
        PlayButtonClick();
        ShowMain();
    }

    void OpenSettings()
    {
        PlayButtonClick();
        ResetAllButtons();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (startSubMenuPanel != null) startSubMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    void ExitGame()
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

    void BackToMainFromSettings()
    {
        PlayButtonClick();
        if (backButton != null)
        {
            HorrorButtonEffect effect = backButton.GetComponent<HorrorButtonEffect>();
            if (effect != null)
            {
                effect.ForceReset();
            }
        }
        ShowMain();
    }

    void ResetAllButtons()
    {
        Button[] allButtons = new Button[] {
            startButton, settingsButton, exitButton,
            backButton, newGameButton, continueButton, backFromStartButton
        };

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

    void PlayErrorSound()
    {
        if (errorSound != null && menuMusicSource != null)
        {
            menuMusicSource.PlayOneShot(errorSound, 0.5f);
        }
    }

    IEnumerator ShakeButton(Button button)
    {
        if (button == null) yield break;

        RectTransform rect = button.GetComponent<RectTransform>();
        Vector3 originalPos = rect.localPosition;

        float duration = 0.5f;
        float elapsed = 0f;
        float shakeIntensity = 15f;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float strength = shakeIntensity * (1f - progress);
            float x = Mathf.Sin(elapsed * 50f) * strength;
            float y = Mathf.Cos(elapsed * 50f) * strength;
            rect.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.localPosition = originalPos;
    }

    IEnumerator FlashRedText(Button button)
    {
        if (button == null) yield break;
        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text == null) yield break;

        Color originalColor = text.color;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = Mathf.PingPong(elapsed * 10f, 1f);
            text.color = Color.Lerp(disabledTextColor, Color.red, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        text.color = originalColor;
    }

    void AddHoverEffects(Button button)
    {
        if (button == null) return;

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { ButtonHoverEnter(button); });
        trigger.triggers.Add(entryEnter);

        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { ButtonHoverExit(button); });
        trigger.triggers.Add(entryExit);

        HorrorButtonEffect effect = button.gameObject.GetComponent<HorrorButtonEffect>();
        if (effect == null) effect = button.gameObject.AddComponent<HorrorButtonEffect>();
        effect.Initialize(this);
    }

    void ButtonHoverEnter(Button button)
    {
        HorrorButtonEffect effect = button.GetComponent<HorrorButtonEffect>();
        if (effect != null && effect.isDisabled) return;

        PlayButtonHover();
        if (effect != null) effect.OnHoverEnter();
    }

    void ButtonHoverExit(Button button)
    {
        HorrorButtonEffect effect = button.GetComponent<HorrorButtonEffect>();
        if (effect != null) effect.OnHoverExit();
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
    public bool isDisabled = false;

    public void Initialize(MainMenuController controller)
    {
        menuController = controller;
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        originalPosition = rectTransform.localPosition;

        if (buttonText != null)
            buttonText.color = menuController.normalTextColor;
    }

    public void OnHoverEnter()
    {
        if (isDisabled) return;
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
        if (rectTransform != null)
        {
            rectTransform.localScale = originalScale;
            rectTransform.localPosition = originalPosition;
        }

        if (buttonText != null && !isDisabled)
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

        if (isHovering && !isDisabled)
        {
            Vector3 targetScale = originalScale * menuController.buttonHoverScale;
            rectTransform.localScale = Vector3.Lerp(
                rectTransform.localScale,
                targetScale,
                Time.deltaTime * menuController.buttonScaleSpeed
            );

            if (buttonText != null)
            {
                buttonText.color = Color.Lerp(
                    buttonText.color,
                    menuController.hoverTextColor,
                    Time.deltaTime * 5f
                );
            }

            if (menuController.enableGlitchEffect)
            {
                glitchTimer += Time.deltaTime;
                if (glitchTimer > Random.Range(0.1f, 0.3f))
                {
                    if (buttonText != null)
                        buttonText.alpha = Random.Range(0.7f, 1f);
                    glitchTimer = 0f;
                }
            }

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
            if (buttonText != null && buttonText.alpha < 1f && !isDisabled)
            {
                buttonText.alpha = Mathf.Lerp(buttonText.alpha, 1f, Time.deltaTime * 5f);
            }
        }
    }
}
