using UnityEngine;

// คลาสที่จัดการการตรวจจับการคลิกเมาส์ทั่วโลก
public class GlobalClickSound : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("Audio Source Component ที่จะใช้เล่นเสียง")]
    // ต้องลาก AudioSource มาใส่ใน Inspector หรือให้โค้ดดึง/เพิ่มให้
    [SerializeField] private AudioSource audioSource;
    
    [Tooltip("ไฟล์เสียง (Audio Clip) ที่จะเล่นเมื่อคลิก")]
    [SerializeField] private AudioClip clickSound;

    // เมธอด Awake ถูกเรียกก่อน Start
    private void Awake()
    {
        // ตรวจสอบและดึง AudioSource Component
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            
            // ถ้ายังไม่มี AudioSource ก็เพิ่มให้เลย
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.LogWarning("AudioSource component was missing and has been automatically added.");
            }
        }
    }

    // เมธอด Update ถูกเรียกทุกเฟรม
    private void Update()
    {
        // ตรวจจับการคลิกปุ่มซ้ายของเมาส์ (Mouse Button 0) ในเฟรมที่ผู้ใช้กดลงไป
        if (Input.GetMouseButtonDown(0))
        {
            PlayClickSound();
        }
    }

    // เมธอดสำหรับเล่นเสียง
    public void PlayClickSound()
    {
        // ตรวจสอบความถูกต้อง
        if (audioSource != null && clickSound != null)
        {
            // ใช้ PlayOneShot เพื่อให้สามารถคลิกรัวๆ แล้วเสียงเล่นซ้อนกันได้
            audioSource.PlayOneShot(clickSound);
            // Debug.Log("Mouse clicked. Playing sound: " + clickSound.name);
        }
        else if (clickSound == null)
        {
            Debug.LogError("Click Sound AudioClip is not assigned in the Inspector!");
        }
    }
}