using UnityEngine;
using UnityEngine.SceneManagement; // *** สำคัญมาก: ต้องใช้ตัวนี้เพื่อจัดการ Scene ***

/// <summary>
/// Script สำหรับจัดการการเปลี่ยนฉาก เมื่อถูกเรียกใช้จากปุ่ม (OnClick)
/// </summary>
public class SceneChanger : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("ใส่ชื่อ Scene ที่ต้องการให้เปลี่ยนไป")]
    [SerializeField] 
    private string targetSceneName = "MainMenu"; // ตั้งชื่อเริ่มต้นไว้เผื่อลืม

    // Awake() ใช้เพื่อตรวจสอบว่าผู้ใช้ได้ใส่ชื่อ Scene แล้ว
    void Awake()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("Error: Target Scene Name is empty! Please assign a Scene name in the Inspector on " + gameObject.name);
        }
    }

    /// <summary>
    /// ฟังก์ชันสาธารณะที่เรียกใช้เมื่อกดปุ่ม (OnClick)
    /// </summary>
    public void ChangeScene()
    {
        // 1. ตรวจสอบว่าชื่อ Scene ไม่ว่าง
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("Cannot change scene: Target Scene Name is null or empty.");
            return;
        }

        // 2. สั่งเปลี่ยน Scene
        // LoadScene ใช้ได้ทั้งชื่อ Scene และ Index ของ Scene
        SceneManager.LoadScene(targetSceneName);
        
        Debug.Log("Attempting to load scene: " + targetSceneName);
    }
}