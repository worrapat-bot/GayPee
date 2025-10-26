using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

// ✅ เก็บข้อมูลทั้งหมดที่เราต้องการเซฟ
[System.Serializable]
public class GameData
{
    public string sceneName;
    public Vector3 playerPosition;
    public float playerRotY;
    public float camRotX;
}

public static class GameSaveSystem
{
    private static string savePath = Application.persistentDataPath + "/save.json";

    // ✅ บันทึกเกม
    public static void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("⚠️ ไม่เจอ Player! ใส่ Tag 'Player' ให้ GameObject ตัวผู้เล่นด้วยนะ");
            return;
        }

        GameData data = new GameData();
        data.sceneName = SceneManager.GetActiveScene().name;
        data.playerPosition = player.transform.position;
        data.playerRotY = player.transform.eulerAngles.y;

        Camera cam = player.GetComponentInChildren<Camera>();
        if (cam)
            data.camRotX = cam.transform.localEulerAngles.x;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log("💾 เซฟแล้ว: " + savePath);
    }

    // ✅ โหลดเกม
    public static void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("⚠️ ไม่มีไฟล์เซฟ!");
            return;
        }

        string json = File.ReadAllText(savePath);
        GameData data = JsonUtility.FromJson<GameData>(json);

        Debug.Log("📂 โหลดไฟล์เซฟจาก: " + savePath);

        SceneManager.sceneLoaded += (scene, mode) =>
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = data.playerPosition;
                player.transform.eulerAngles = new Vector3(0, data.playerRotY, 0);

                Camera cam = player.GetComponentInChildren<Camera>();
                if (cam)
                    cam.transform.localEulerAngles = new Vector3(data.camRotX, 0, 0);

                Debug.Log("✅ โหลดตำแหน่ง Player เรียบร้อย!");
            }
        };

        SceneManager.LoadScene(data.sceneName);
    }

    public static bool HasSaveFile()
    {
        return File.Exists(savePath);
    }

    public static void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("🗑️ ลบไฟล์เซฟแล้ว");
        }
    }
}
