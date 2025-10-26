using UnityEngine;
using System.Collections; // สำคัญ: ต้องใช้สำหรับ Coroutine

// คลาสที่จัดการการหน่วงเวลาและเล่นเสียง
public class AudioTimer : MonoBehaviour
{
    // [SerializeField] ทำให้เราสามารถลาก Audio Source และ Audio Clip มาใส่ใน Inspector ได้
    [Header("Audio Settings")]
    [Tooltip("Component Audio Source ที่จะใช้เล่นเสียง")]
    [SerializeField] private AudioSource audioSource; 
    
    [Tooltip("ไฟล์เสียง (Audio Clip) ที่ต้องการจะเล่น")]
    [SerializeField] private AudioClip soundToPlay;

    [Header("Timer Settings")]
    [Tooltip("เวลาหน่วงเป็นวินาที (ตั้งค่าเป็น 15.0f ตามที่ต้องการ)")]
    [SerializeField] private float delayTime = 15f; 

    // เมธอด Start ถูกเรียกครั้งเดียวเมื่อ Game Object เริ่มทำงาน
    private void Start()
    {
        // 1. ตรวจสอบความถูกต้องของ Component และ Clip ก่อนเริ่ม
        if (audioSource == null)
        {
            // ลองดึง AudioSource จาก Game Object เดียวกัน ถ้ายังไม่มี
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("Audio Source is missing! Please attach one or assign it in the Inspector.");
                return; // หยุดการทำงานถ้าไม่มี AudioSource
            }
        }

        if (soundToPlay == null)
        {
            Debug.LogError("Sound Clip is missing! Please assign the AudioClip in the Inspector.");
            return;
        }

        // 2. เริ่ม Coroutine เพื่อหน่วงเวลา
        StartCoroutine(PlaySoundAfterDelay(delayTime));
    }

    // Coroutine: เป็นฟังก์ชันพิเศษของ Unity สำหรับการหยุดรอโดยไม่บล็อกเกม
    private IEnumerator PlaySoundAfterDelay(float delay)
    {
        Debug.Log($"Timer started. Waiting for {delay} seconds...");
        
        // รอเป็นระยะเวลา delay ที่กำหนด
        yield return new WaitForSeconds(delay);
        
        // เมื่อรอครบ 15 วินาทีแล้ว...
        
        // 3. ตั้งค่า AudioClip และเล่นเสียง
        audioSource.clip = soundToPlay;
        audioSource.Play();
        
        Debug.Log("15 seconds passed. Playing sound: " + soundToPlay.name);
    }
}