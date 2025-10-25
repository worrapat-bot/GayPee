using UnityEngine;

public class SimpleFurnace : MonoBehaviour
{
    [Header("Settings")]
    public int requiredMetal = 5;
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.F;

    [Header("Key Prefab")]
    public GameObject keyPrefab; // ลาก Key prefab มาใส่

    private int currentMetal = 0;
    private GameObject player;
    private RadialInventoryVertical inventory;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        inventory = FindObjectOfType<RadialInventoryVertical>();
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(player.transform.position, transform.position);

        // กด F เมื่ออยู่ใกล้
        if (dist < interactDistance && Input.GetKeyDown(interactKey))
        {
            TryAddMetal();
        }
    }

    void TryAddMetal()
    {
        // เช็คว่าเต็มแล้วหรือยัง
        if (currentMetal >= requiredMetal)
        {
            Debug.Log("เตาเต็มแล้ว!");
            return;
        }

        // เช็คว่ามี Metal ในมือหรือไม่
        if (inventory != null && inventory.HasItemInHand())
        {
            string itemName = inventory.GetCurrentItemName();

            if (itemName == "Metal")
            {
                // ลบ Metal ออกจากมือ
                inventory.RemoveCurrentItem();

                // เพิ่มจำนวน
                currentMetal++;
                Debug.Log($"โยน Metal เข้าเตา! ({currentMetal}/{requiredMetal})");

                // ถ้าครบ 5 → สร้าง Key
                if (currentMetal >= requiredMetal)
                {
                    SpawnKey();
                }
            }
            else
            {
                Debug.Log("ต้องถือ Metal ก่อน!");
            }
        }
        else
        {
            Debug.Log("ไม่มีของในมือ!");
        }
    }

    void SpawnKey()
    {
        Debug.Log("🔥 หลอมเสร็จแล้ว! Key ถูกสร้าง!");

        // สร้าง Key ตรงหน้าเตา
        Vector3 spawnPos = transform.position + transform.forward * 1.5f + Vector3.up * 0.5f;

        GameObject key = Instantiate(keyPrefab, spawnPos, Quaternion.identity);

        // ตั้ง item ID
        SimpleItemInteract interact = key.GetComponent<SimpleItemInteract>();
        if (interact != null)
        {
            interact.itemID = "Key";
        }

        // รีเซ็ตเตา
        currentMetal = 0;
    }

    void OnGUI()
    {
        if (player == null) return;

        float dist = Vector3.Distance(player.transform.position, transform.position);

        // แสดงข้อความเมื่ออยู่ใกล้
        if (dist < interactDistance)
        {
            // แปลง world position → screen position
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.5f);

            // ถ้าอยู่หน้ากล้อง
            if (screenPos.z > 0)
            {
                // กลับ Y (OnGUI ใช้ระบบพิกัดกลับหัว)
                screenPos.y = Screen.height - screenPos.y;

                // กำหนดขนาดกล่องข้อความ
                Rect rect = new Rect(screenPos.x - 100, screenPos.y - 50, 200, 100);

                // พื้นหลังดำ
                GUI.Box(rect, "");

                // ข้อความหลัก
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.fontSize = 24;
                style.fontStyle = FontStyle.Bold;
                style.alignment = TextAnchor.MiddleCenter;

                // เลือกสีตามสถานะ
                if (currentMetal >= requiredMetal)
                {
                    style.normal.textColor = Color.green;
                    GUI.Label(rect, "✓ READY", style);
                }
                else
                {
                    style.normal.textColor = Color.yellow;
                    GUI.Label(rect, $"Metal: {currentMetal}/{requiredMetal}", style);
                }

                // คำแนะนำ
                if (currentMetal < requiredMetal)
                {
                    Rect hintRect = new Rect(screenPos.x - 100, screenPos.y, 200, 30);
                    GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
                    hintStyle.fontSize = 14;
                    hintStyle.alignment = TextAnchor.MiddleCenter;
                    hintStyle.normal.textColor = Color.white;
                    GUI.Label(hintRect, "Press F to add Metal", hintStyle);
                }
            }
        }
    }
}