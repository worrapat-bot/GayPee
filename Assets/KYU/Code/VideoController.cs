// ต้องใส่ using นี้เสมอเมื่อใช้งาน VideoPlayer
using UnityEngine;
using UnityEngine.Video; // [สำคัญมาก] สำหรับการเข้าถึงคลาส VideoPlayer

public class VideoController : MonoBehaviour
{
    // [OOP] สร้าง Field เพื่ออ้างอิงถึง Component (Encapsulation)
    // เราสามารถลาก VideoPlayer Component มาใส่ใน Inspector ได้เลย
    [SerializeField] private VideoPlayer videoPlayer;
    
    // [OOP] สร้าง Field สำหรับ Event ต่างๆ (Event Handling)
    public bool playOnStart = true;
    
    void Start()
    {
        // ตรวจสอบว่ามี VideoPlayer Component ถูกกำหนดหรือไม่
        if (videoPlayer == null)
        {
            // พยายามหา VideoPlayer บน GameObject เดียวกัน
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer Component ไม่ได้ถูกกำหนด!");
            return;
        }

        // [Event Handling] สมัครสมาชิก Event เมื่อวิดีโอเล่นจบ
        // เมื่อวิดีโอเล่นเสร็จ จะเรียกใช้ฟังก์ชัน OnVideoFinished
        videoPlayer.loopPointReached += OnVideoFinished;
        
        // ตรวจสอบและเริ่มเล่นวิดีโอ
        if (playOnStart)
        {
            PlayVideo();
        }
    }

    // [Method] สั่งให้วิดีโอเล่น
    public void PlayVideo()
    {
        if (!videoPlayer.isPlaying)
        {
            videoPlayer.Play();
            Debug.Log("วิดีโอเริ่มเล่น");
        }
    }

    // [Method] สั่งให้วิดีโอหยุด
    public void StopVideo()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            Debug.Log("วิดีโอหยุดเล่น");
        }
    }

    // [Method] สั่งให้วิดีโอพัก
    public void PauseVideo()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            Debug.Log("วิดีโอพัก");
        }
    }

    // [Event Handler] ฟังก์ชันที่จะถูกเรียกเมื่อวิดีโอเล่นจบ (LoopPointReached Event)
    private void OnVideoFinished(VideoPlayer vp)
    {
        // vp คือ VideoPlayer ที่ส่ง Event มา ซึ่งก็คือ videoPlayer ตัวนี้เอง
        Debug.Log("วิดีโอเล่นจบแล้ว! " + vp.clip.name);
        
        // ตัวอย่างการทำบางอย่างเมื่อวิดีโอจบ เช่น โหลดฉากต่อไป
        // SceneManager.LoadScene("NextLevel");
    }

    void OnDestroy()
    {
        // [สำคัญ] ยกเลิกการสมัครสมาชิก Event เมื่อ GameObject ถูกทำลาย
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}