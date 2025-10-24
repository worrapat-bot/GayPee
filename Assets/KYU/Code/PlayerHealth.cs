using UnityEngine;
using System.Collections;

// *******************************************************************
// คำแนะนำ: Player ควรมี Collider (ตั้งค่าเป็น Is Trigger) และ Tag เป็น "Player"
// *******************************************************************

public class PlayerHealth : MonoBehaviour
{
    // ตัวแปรสำหรับเช็คสถานะ
    public bool isHitByEnemy { get; private set; } = false;

    // ตัวแปรสำหรับจัดการการหยุดเวลา หากต้องการทำ Animation/UI ใน Unscaled Time
    [Header("Game Over Settings")]
    [Tooltip("อ้างอิงถึง UI/Camera ที่ต้องการเล่น Animation หลังเกมหยุดเวลา")]
    public GameObject gameOverUI; 
    
    // ตั้งค่าเริ่มต้น
    private void Start()
    {
        // ตรวจสอบให้แน่ใจว่า Time.timeScale ถูกรีเซ็ตเมื่อ Scene โหลด
        if (Time.timeScale != 1.0f)
        {
            Time.timeScale = 1.0f;
        }

        if (gameOverUI != null)
        {
            // ซ่อน UI Game Over เมื่อเริ่มต้น
            gameOverUI.SetActive(false); 
        }
    }

    // เมธอดนี้จะถูกเรียกเมื่อ Collider ผู้เล่น (ที่เป็น Trigger) ชนกับ Collider อื่น
    private void OnTriggerEnter(Collider other)
    {
        // 1. ตรวจสอบว่าชนกับศัตรูที่เรียก Jumpscare Sequence
        // ในโค้ด EnemyPatrol เราใช้ Tag "Player" ในการตรวจจับ แต่เราต้องแน่ใจว่า
        // Object ที่ชนเราคือ Enemy และเรายังไม่ถูกโจมตี
        
        // เราสามารถใช้ Tag "Enemy" หรือ Component (EnemyPatrol) เพื่อความแม่นยำ
        // สมมติว่า Enemy GameObject มี Tag เป็น "Enemy"
        
        if (other.CompareTag("Enemy") && !isHitByEnemy)
        {
            // 2. ล็อกสถานะว่าโดนโจมตีแล้ว
            isHitByEnemy = true;
            Debug.Log("Player was hit by enemy! Starting player death sequence.");

            // 3. จัดการ UI/Animation/Camera Shake ที่นี่ (ถ้าต้องการให้ทำทันที)
            
            // 4. รอ EnemyPatrol จัดการ Freeze Time
            // เนื่องจาก EnemyPatrol จะสั่ง Time.timeScale = 0f ให้เรา
            // เมธอดต่อไปนี้จึงต้องถูกเรียกใน Update() หรือใช้ Coroutine 
            // ที่ทำงานใน Unscaled Time
            
            StartCoroutine(HandleFreezeTimeEffects());
        }
    }
    
    // Coroutine ที่ทำงานใน Unscaled Time เพื่อแสดง UI หรือ Animation อื่นๆ 
    // หลังเกมถูก EnemyPatrol Freeze (Time.timeScale = 0f)
    private IEnumerator HandleFreezeTimeEffects()
    {
        // รอนานพอที่ EnemyPatrol จะสั่ง Time.timeScale = 0f (อาจจะรอ 1 เฟรม)
        yield return null; 

        if (Time.timeScale == 0f)
        {
            Debug.Log("Game time is frozen. Displaying Unscaled Time effects.");
            
            // TODO: จัดการ Fade Screen / Game Over UI
            if (gameOverUI != null)
            {
                gameOverUI.SetActive(true);
                // ถ้า UI มี Animation/Fade In ให้ใช้ Coroutine ที่ใช้ 
                // WaitForSecondsRealtime หรือ Time.unscaledDeltaTime
            }

            // ในตอนนี้ ทุกอย่างจะหยุดนิ่งจนกว่า EnemyPatrol จะโหลด Scene ใหม่
            // เนื่องจาก EnemyPatrol ใช้ WaitForSecondsRealtime() ในการรอดำเนินการต่อ 
        }
    }
}