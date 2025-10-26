using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject pausePanel;
    public GameObject savePanel;

    [Header("Buttons")]
    public Button resumeButton;
    public Button saveButton;
    public Button leaveButton;
    public Button yesSaveButton;
    public Button noSaveButton;

    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("Animation Settings")]
    public float hoverScale = 1.15f;
    public float animSpeed = 10f;

    [Header("Scene Settings")]
    public string menuSceneName = "MainMenu";

    private bool isPaused = false;
    private Vector3 normalScale = Vector3.one;

    void Start()
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (savePanel) savePanel.SetActive(false);

        // เชื่อมปุ่มทั้งหมด
        if (resumeButton) resumeButton.onClick.AddListener(ResumeGame);
        if (saveButton) saveButton.onClick.AddListener(SaveGame); // ✅ ปุ่ม Save
        if (leaveButton) leaveButton.onClick.AddListener(OpenSavePanel); // ✅ ปุ่ม Leave
        if (yesSaveButton) yesSaveButton.onClick.AddListener(ConfirmLeaveWithSave);
        if (noSaveButton) noSaveButton.onClick.AddListener(LeaveWithoutSave);

        // เพิ่มเอฟเฟกต์ Hover ให้ทุกปุ่ม
        AddButtonEffects(resumeButton);
        AddButtonEffects(saveButton);
        AddButtonEffects(leaveButton);
        AddButtonEffects(yesSaveButton);
        AddButtonEffects(noSaveButton);

        // เริ่มเกมด้วยเวลาปกติ
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Update()
    {
        // ✅ ตรวจสอบว่า pause panel หรือ save panel เปิดอยู่หรือไม่
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ถ้าเปิด save panel อยู่ ให้กลับไปที่ pause panel
            if (savePanel != null && savePanel.activeSelf)
            {
                savePanel.SetActive(false);
                pausePanel.SetActive(true);
                return;
            }

            // ไม่งั้นก็ toggle pause ปกติ
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    // -------------------- MENU CONTROL --------------------

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;

        // ✅ เปิด cursor และปลดล็อก
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // แสดง UI
        if (pausePanel) pausePanel.SetActive(true);
        if (savePanel) savePanel.SetActive(false);

        // ✅ บอก PlayerController ว่าเข้าโหมด dialog (ห้ามขยับ + ห้ามจัดการ cursor)
        PlayerController.dialog = true;
    }

    public void ResumeGame()
    {
        PlayClick();

        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // ซ่อน UI
        if (pausePanel) pausePanel.SetActive(false);
        if (savePanel) savePanel.SetActive(false);

        // ✅ ล็อก cursor กลับ
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ✅ บอก PlayerController ว่าออกจาก dialog แล้ว
        PlayerController.dialog = false;
    }

    void SaveGame()
    {
        PlayClick();

        // 🔜 ภายหลังจะใส่ระบบ Save ที่นี่
        Debug.Log("💾 Game Saved!");

        // ✅ Save เสร็จแล้ว กลับไปหน้า Pause Menu
        if (savePanel) savePanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(true);
    }

    void OpenSavePanel()
    {
        PlayClick();

        // ✅ ยังคงหยุดเกมอยู่ เปิดเมาส์ต่อ
        isPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlayerController.dialog = true;

        // แสดงหน้า Save
        if (pausePanel) pausePanel.SetActive(false);
        if (savePanel) savePanel.SetActive(true);
    }

    void ConfirmLeaveWithSave()
    {
        PlayClick();

        // ✅ ปลดล็อกเกมก่อนเปลี่ยน scene
        Time.timeScale = 1f;
        AudioListener.pause = false;
        PlayerController.dialog = false;

        if (!string.IsNullOrEmpty(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            Debug.LogWarning("⚠️ menuSceneName ยังไม่ได้ตั้งค่าใน Inspector!");
        }
    }

    void LeaveWithoutSave()
    {
        PlayClick();

        // ✅ ปลดล็อกเกมก่อนเปลี่ยน scene
        Time.timeScale = 1f;
        AudioListener.pause = false;
        PlayerController.dialog = false;

        if (!string.IsNullOrEmpty(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            Debug.LogWarning("⚠️ menuSceneName ยังไม่ได้ตั้งค่าใน Inspector!");
        }
    }

    // -------------------- HOVER EFFECTS --------------------

    private void AddButtonEffects(Button button)
    {
        if (button == null) return;

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener((data) => { OnHoverEnter(button); });
        trigger.triggers.Add(enter);

        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener((data) => { OnHoverExit(button); });
        trigger.triggers.Add(exit);
    }

    private void OnHoverEnter(Button button)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleButton(button.transform, hoverScale));
        PlayHover();
    }

    private void OnHoverExit(Button button)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleButton(button.transform, 1f));
    }

    private System.Collections.IEnumerator ScaleButton(Transform target, float targetScale)
    {
        Vector3 start = target.localScale;
        Vector3 end = normalScale * targetScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * animSpeed;
            target.localScale = Vector3.Lerp(start, end, t);
            yield return null;
        }
    }

    // -------------------- AUDIO --------------------

    private void PlayHover()
    {
        if (uiAudioSource && hoverSound)
            uiAudioSource.PlayOneShot(hoverSound);
    }

    private void PlayClick()
    {
        if (uiAudioSource && clickSound)
            uiAudioSource.PlayOneShot(clickSound);
    }
}