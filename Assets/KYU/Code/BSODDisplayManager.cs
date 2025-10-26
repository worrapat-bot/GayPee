using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// จัดการการแสดงผลหน้าจอฟ้า (BSOD) และการหยุด/คืนค่าเวลาของเกม
/// ควรแนบกับ GameObject ของผู้เล่น (Player)
/// </summary>
public class BSODDisplayManager : MonoBehaviour
{
    // Encapsulation: ใช้ [SerializeField] เพื่อให้ตั้งค่าใน Inspector ได้
    [Header("BSOD UI Setup")]
    [SerializeField] private GameObject bsodCanvas; // ลาก Canvas หรือ Raw Image ที่มีข้อความ BSOD มาใส่
    [SerializeField] private float displayDuration = 5f; // ระยะเวลาที่หน้าจอ BSOD จะแสดง (วินาที)
    [SerializeField] private float timeScaleToResume = 1f; // ค่า Time.timeScale ที่จะกลับไปใช้
    
    [Header("Game Over Settings (Optional)")]
    public string nextSceneNameOnFinish = ""; // ชื่อฉากที่จะโหลดต่อ
    
    // Property เพื่อใช้เช็คสถานะจากภายนอก (OOP)
    public bool IsBSODActive { get; private set; } = false;

    /// <summary>
    /// เริ่มลำดับการแสดงหน้าจอฟ้า
    /// </summary>
    public void StartBSODSequence()
    {
        if (!IsBSODActive)
        {
            StartCoroutine(SimulateCrashSequence());
        }
    }

    private IEnumerator SimulateCrashSequence()
    {
        IsBSODActive = true;
        // 1. Freeze Time/Game
        Time.timeScale = 0f; // หยุดการเคลื่อนไหวทั้งหมดในเกม

        // 2. Show the BSOD Screen
        if (bsodCanvas != null)
        {
            bsodCanvas.SetActive(true);
        }
        
        // 3. Wait for the specified duration (ใช้ Unscaled time เพราะ Time.timeScale = 0)
        float startTime = Time.unscaledTime;
        while (Time.unscaledTime < startTime + displayDuration)
        {
            yield return null; 
        }

        // 4. Restore หรือ Load Scene
        if (!string.IsNullOrEmpty(nextSceneNameOnFinish))
        {
            // โหลดฉากต่อ
            Time.timeScale = timeScaleToResume; // คืนค่า Time Scale ก่อนโหลดฉาก
            SceneManager.LoadScene(nextSceneNameOnFinish);
        }
        else
        {
            // กลับมาเป็นปกติ (หากไม่โหลดฉาก)
            DeactivateBSOD();
        }
    }

    /// <summary>
    /// คืนค่าเกมให้เป็นปกติ
    /// </summary>
    public void DeactivateBSOD()
    {
        if (bsodCanvas != null)
        {
            bsodCanvas.SetActive(false); // ซ่อนจอ BSOD
        }

        Time.timeScale = timeScaleToResume; // คืนค่าเวลาให้เกมเดินต่อ
        IsBSODActive = false;
    }
}