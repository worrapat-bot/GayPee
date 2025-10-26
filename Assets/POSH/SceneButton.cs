using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButton : MonoBehaviour
{
    public string sceneName;          // ชื่อ Scene ที่จะไป
    public AudioSource audioSource;   // AudioSource ที่จะเล่นเสียง
    public AudioClip clickSound;      // เสียงตอนกด

    public void OnButtonClick()
    {
        // เล่นเสียงคลิก
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);

        // เรียกฟังก์ชันเปลี่ยน Scene หลังเสียงเล่นนิดหน่อย
        Invoke(nameof(ChangeScene), 0.5f);
    }

    void ChangeScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}