using UnityEngine;
using UnityEngine.SceneManagement; // สำคัญ: ต้องใช้สำหรับ SceneManager
using System.Collections; // ต้องใช้สำหรับ Coroutine

// คลาสที่จัดการการหน่วงเวลาเพื่อเปลี่ยน Scene
public class DelayedSceneLoader : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [Tooltip("ชื่อของ Scene ที่ต้องการเปลี่ยนไป")]
    [SerializeField] private string targetSceneName = "YourNextSceneName";
    
    [Tooltip("เวลาหน่วงเป็นวินาที (ตั้งค่าเป็น 31.0f ตามที่ต้องการ)")]
    [SerializeField] private float delayTime = 31f; 

    // เมธอด Start ถูกเรียกครั้งเดียวเมื่อ Game Object เริ่มทำงาน
    private void Start()
    {
        // 1. ตรวจสอบชื่อ Scene เผื่อผู้ใช้ลืมกำหนด
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("Target Scene Name is empty! Please assign the name of the next scene in the Inspector.");
            return;
        }

        Debug.Log($"Scene change timer started. Will switch to '{targetSceneName}' in {delayTime} seconds.");
        
        // 2. เริ่ม Coroutine เพื่อหน่วงเวลา
        StartCoroutine(LoadSceneAfterDelay(delayTime));
    }

    // Coroutine: ฟังก์ชันสำหรับรอเวลาโดยไม่บล็อกเกม
    private IEnumerator LoadSceneAfterDelay(float delay)
    {
        // รอเป็นระยะเวลา delay ที่กำหนด (31 วินาที)
        yield return new WaitForSeconds(delay);
        
        // เมื่อรอครบ 31 วินาทีแล้ว...
        
        // 3. สั่งให้ SceneManager โหลด Scene ใหม่ตามชื่อที่กำหนด
        SceneManager.LoadScene(targetSceneName);
        
        Debug.Log($"Time is up! Loading scene: {targetSceneName}");
    }

    // หลักการ OOP: เมธอด Public สำหรับให้คลาสอื่นสามารถสั่งเปลี่ยน Scene ได้ทันที
    public void LoadSceneNow()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}