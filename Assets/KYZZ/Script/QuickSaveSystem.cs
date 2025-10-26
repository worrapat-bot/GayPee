using UnityEngine;
using UnityEngine.SceneManagement;

public class QuickSaveSystem : MonoBehaviour
{
    private static QuickSaveSystem instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ✅ บันทึกเกม
    public static void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("⚠️ ไม่เจอ Player! ตรวจสอบ Tag 'Player'");
            return;
        }

        // เซฟตำแหน่ง
        PlayerPrefs.SetFloat("PlayerX", player.transform.position.x);
        PlayerPrefs.SetFloat("PlayerY", player.transform.position.y);
        PlayerPrefs.SetFloat("PlayerZ", player.transform.position.z);

        // เซฟมุมมอง
        PlayerPrefs.SetFloat("PlayerRotY", player.transform.eulerAngles.y);

        // เซฟกล้อง (ถ้ามี)
        Camera cam = player.GetComponentInChildren<Camera>();
        if (cam != null)
        {
            PlayerPrefs.SetFloat("CamRotX", cam.transform.localEulerAngles.x);
        }

        // เซฟ scene ปัจจุบัน
        PlayerPrefs.SetString("CurrentScene", SceneManager.GetActiveScene().name);

        PlayerPrefs.Save();
        Debug.Log("💾 บันทึกเกมสำเร็จ!");
    }

    // ✅ โหลดเกม
    public static void LoadGame()
    {
        if (!PlayerPrefs.HasKey("PlayerX"))
        {
            Debug.LogWarning("⚠️ ไม่มีข้อมูลเซฟ!");
            return;
        }

        // โหลด scene
        string sceneName = PlayerPrefs.GetString("CurrentScene");
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(sceneName);
        }
    }

    // ✅ หลังจากโหลด scene เสร็จ → เอาตำแหน่งกลับมา
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("⚠️ ไม่เจอ Player หลังโหลด scene!");
            return;
        }

        // โหลดตำแหน่ง
        Vector3 pos = new Vector3(
            PlayerPrefs.GetFloat("PlayerX"),
            PlayerPrefs.GetFloat("PlayerY"),
            PlayerPrefs.GetFloat("PlayerZ")
        );
        player.transform.position = pos;

        // โหลดมุมมอง
        float rotY = PlayerPrefs.GetFloat("PlayerRotY");
        player.transform.eulerAngles = new Vector3(0, rotY, 0);

        // โหลดกล้อง
        Camera cam = player.GetComponentInChildren<Camera>();
        if (cam != null && PlayerPrefs.HasKey("CamRotX"))
        {
            float camRotX = PlayerPrefs.GetFloat("CamRotX");
            cam.transform.localEulerAngles = new Vector3(camRotX, 0, 0);

            // ✅ อัพเดท rotationX ใน PlayerController
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                // ต้องใช้ Reflection เพราะ rotationX เป็น private
                var field = typeof(PlayerController).GetField("rotationX",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(pc, camRotX);
                }
            }
        }

        Debug.Log("✅ โหลดเกมสำเร็จ!");
    }

    // ✅ ลบข้อมูลเซฟ
    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey("PlayerX");
        PlayerPrefs.DeleteKey("PlayerY");
        PlayerPrefs.DeleteKey("PlayerZ");
        PlayerPrefs.DeleteKey("PlayerRotY");
        PlayerPrefs.DeleteKey("CamRotX");
        PlayerPrefs.DeleteKey("CurrentScene");
        PlayerPrefs.Save();
        Debug.Log("🗑️ ลบข้อมูลเซฟแล้ว!");
    }

    // ✅ ตรวจสอบว่ามีเซฟหรือไม่
    public static bool HasSaveData()
    {
        return PlayerPrefs.HasKey("PlayerX");
    }
}