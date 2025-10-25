// RandomVideoPlayer.cs
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI; 
using System.Collections; // สำหรับ Coroutine

public class RandomVideoPlayer : MonoBehaviour
{
    [Header("Video Components")]
    [Tooltip("ลาก VideoPlayer Component ใน Scene มาใส่")]
    public VideoPlayer videoPlayer;
    
    [Tooltip("ลาก RawImage UI Element ที่ใช้แสดงวิดีโอมาใส่")]
    public RawImage displayScreen;
    
    [Header("Clips and Timing")]
    [Tooltip("ลาก VideoClip (Asset) ที่ต้องการสุ่มเล่นมาใส่ในช่องนี้")]
    // Array นี้อนุญาตให้เพิ่มคลิปได้เรื่อยๆ ใน Inspector
    public VideoClip[] videoClips;
    
    // ตั้งเวลาเล่นคงที่ 3 วินาที (สามารถแก้ไขได้ที่นี่)
    private const float PLAYBACK_DURATION = 3f;

    void Start()
    {
        // ตรวจสอบความพร้อมของ Component ที่จำเป็น
        if (videoPlayer == null || displayScreen == null || videoClips.Length == 0)
        {
            Debug.LogError("ตั้งค่าไม่ครบ! กรุณาตรวจสอบ VideoPlayer, RawImage, และ VideoClips ใน Inspector");
            return;
        }

        // กำหนดให้ RawImage ซ่อนไว้ก่อน
        displayScreen.enabled = false;
    }

    /// <summary>
    /// เมธอดหลักที่เรียกใช้เมื่อผู้ใช้กดปุ่ม
    /// </summary>
    public void PlayRandomClip()
    {
        // ป้องกันการเล่นคลิปซ้อนกัน ถ้ากำลังเล่นอยู่ให้หยุดก่อน
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
        
        // 1. สุ่มเลือกคลิป (Random Selection)
        // Random.Range สำหรับ Array จะสุ่มตั้งแต่ 0 จนถึง videoClips.Length - 1
        int randomIndex = Random.Range(0, videoClips.Length);
        videoPlayer.clip = videoClips[randomIndex];

        // 2. เริ่มเล่นคลิป
        displayScreen.enabled = true; // แสดงหน้าจอ
        videoPlayer.Play();
        
        Debug.Log($"Start playing clip: {videoClips[randomIndex].name} for {PLAYBACK_DURATION} seconds.");

        // 3. เริ่ม Coroutine เพื่อหยุดคลิปหลัง 3 วินาที
        StartCoroutine(StopAfterDelay(PLAYBACK_DURATION));
    }

    /// <summary>
    /// Coroutine สำหรับรอเวลาแล้วหยุดวิดีโอ
    /// </summary>
    private IEnumerator StopAfterDelay(float delay)
    {
        // รอตามระยะเวลาที่กำหนด (3 วินาที)
        yield return new WaitForSeconds(delay);

        // 4. หยุดวิดีโอและซ่อนหน้าจอ
        videoPlayer.Stop();
        displayScreen.enabled = false;
        Debug.Log("Video playback finished.");
    }
}