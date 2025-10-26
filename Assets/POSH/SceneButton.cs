using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTriggerLoader : MonoBehaviour
{
    [Header("Setting")]
    public string sceneToLoad = "NextMap"; // ชื่อซีนที่จะย้ายไป
    public string loadingSceneName = "LoadingScene"; // ซีนโหลด

    [Header("Interact")]
    public KeyCode interactKey = KeyCode.E; // ปุ่มกด
    public float interactDistance = 3f;     // ระยะที่กดได้
    public Transform player;                // ตัวผู้เล่น

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip interactSound;

    private bool isInRange = false;

    void Update()
    {
        if (player == null) return;

        // ตรวจระยะห่าง
        float distance = Vector3.Distance(transform.position, player.position);
        isInRange = distance <= interactDistance;

        // ถ้าผู้เล่นอยู่ใกล้ และกดปุ่ม
        if (isInRange && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(PlaySoundAndLoad());
        }
    }

    private System.Collections.IEnumerator PlaySoundAndLoad()
    {
        if (audioSource && interactSound)
        {
            audioSource.PlayOneShot(interactSound);
            yield return new WaitForSeconds(interactSound.length); // รอเสียงจบก่อนโหลด
        }

        // ส่งข้อมูลชื่อซีนที่จะโหลดไปยังหน้าซีนโหลด
        PlayerPrefs.SetString("NextSceneToLoad", sceneToLoad);
        SceneManager.LoadScene(loadingSceneName);
    }
}
