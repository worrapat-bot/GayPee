using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoController2 : MonoBehaviour
{
    public VideoPlayer videoPlayer; // ใส่ Video Player จาก Inspector
    public string nextSceneName = "MainMenu"; // ตั้งชื่อ Scene ต่อไป

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
