using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTriggerLoaderInstant : MonoBehaviour
{
    [Header("Setting")]
    public string sceneToLoad = "NextMap"; // ชื่อซีนที่จะย้ายไป

    [Header("Interact")]
    public KeyCode interactKey = KeyCode.E; // ปุ่มกด
    public float interactDistance = 3f;     // ระยะที่กดได้
    public Transform player;                // ตัวผู้เล่น

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip interactSound;

    private void Update()
    {
        if (player == null) return;

        // ตรวจระยะห่าง
        float distance = Vector3.Distance(transform.position, player.position);
        bool isInRange = distance <= interactDistance;

        // ถ้าผู้เล่นอยู่ใกล้ และกดปุ่ม
        if (isInRange && Input.GetKeyDown(interactKey))
        {
            // เล่นเสียงแบบไม่รอ
            if (audioSource && interactSound)
            {
                audioSource.PlayOneShot(interactSound);
            }

            // โหลดซีนทันที
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}