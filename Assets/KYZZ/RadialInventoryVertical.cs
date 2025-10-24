using UnityEngine;
using System.Collections.Generic;

public class RadialInventoryVertical : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int slotCount = 3;
    [SerializeField] private float spacing = 100f;
    [SerializeField] private float normalSize = 60f;
    [SerializeField] private float hoverSize = 80f;
    [SerializeField] private float animSpeed = 10f;
    [SerializeField] private Vector2 screenPosition = new Vector2(150, 400);

    [Header("Visual")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    [SerializeField] private Color selectedColor = new Color(0.3f, 0.9f, 0.3f, 1f);
    [SerializeField] private Texture2D slotBackground;

    [Header("Player Hand Reference")]
    public Transform handPoint;

    [Header("🆕 Drop Settings")]
    [SerializeField] private float dropDistance = 2f; // ระยะที่วางของ (เมตร)

    private bool isOpen;
    private List<InventorySlot> slots = new List<InventorySlot>();
    private int selectedIndex = -1;
    private float openProgress;

    private class InventorySlot
    {
        public Vector2 position;
        public float currentSize;
        public string itemName;
        public GameObject itemObject;
        public Texture2D iconTexture;
        public bool isEmpty = true;
    }

    void Start()
    {
        slots.Clear();
        for (int i = 0; i < slotCount; i++)
            slots.Add(new InventorySlot());

        if (slotBackground == null)
            slotBackground = Texture2D.grayTexture;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            isOpen = true;
            Time.timeScale = 0.3f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonUp(2))
        {
            isOpen = false;
            Time.timeScale = 1f;

            if (selectedIndex >= 0)
                EquipSelectedItem();
            else
                ClearHandItem();
        }

        // ✅ กด E = วางของ (ถ้ามีของในมือ)
        if (Input.GetKeyDown(KeyCode.E))
        {
            DropCurrentItem();
        }

        openProgress = Mathf.Lerp(openProgress, isOpen ? 1f : 0f, Time.unscaledDeltaTime * 8f);

        if (isOpen && openProgress > 0.1f)
        {
            UpdateSlotPositions();
            HandleScrollSelection();
        }
    }

    void UpdateSlotPositions()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            float yOffset = i * spacing * openProgress;
            slots[i].position = screenPosition + new Vector2(0, yOffset);
        }
    }

    void HandleScrollSelection()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            if (scroll > 0f)
                selectedIndex--;
            else if (scroll < 0f)
                selectedIndex++;

            if (selectedIndex < 0) selectedIndex = slots.Count - 1;
            if (selectedIndex >= slots.Count) selectedIndex = 0;

            if (slots[selectedIndex].isEmpty)
                ClearHandItem();
            else
                EquipSelectedItem();
        }

        for (int i = 0; i < slots.Count; i++)
        {
            float targetSize = (i == selectedIndex) ? hoverSize : normalSize;
            slots[i].currentSize = Mathf.Lerp(slots[i].currentSize, targetSize, Time.unscaledDeltaTime * animSpeed);
        }
    }

    void ClearHandItem()
    {
        foreach (var slot in slots)
        {
            if (slot.itemObject != null)
            {
                slot.itemObject.transform.SetParent(null);
                slot.itemObject.SetActive(false);
            }
        }
    }

    void EquipSelectedItem()
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Count)
        {
            ClearHandItem();
            return;
        }

        var slot = slots[selectedIndex];

        if (slot.isEmpty || slot.itemObject == null)
        {
            ClearHandItem();
            return;
        }

        ClearHandItem();

        slot.itemObject.SetActive(true);
        slot.itemObject.transform.SetParent(handPoint);
        slot.itemObject.transform.localPosition = Vector3.zero;
        slot.itemObject.transform.localRotation = Quaternion.identity;
    }

    // ✅ ฟังก์ชันวางของ - เรียกจาก PlayerController ด้วย
    public void DropCurrentItem()
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Count) return;

        var slot = slots[selectedIndex];
        if (slot.isEmpty || slot.itemObject == null) return;

        // วางของหน้าผู้เล่น (สูงขึ้นหน่อย)
        Camera cam = Camera.main;
        Vector3 dropPos = cam.transform.position + cam.transform.forward * dropDistance;
        dropPos.y += 0.5f; // ⭐ ยกขึ้นสูงกว่าพื้นนิดหน่อย

        slot.itemObject.SetActive(true);
        slot.itemObject.transform.SetParent(null);
        slot.itemObject.transform.position = dropPos;
        slot.itemObject.transform.rotation = Quaternion.identity;

        // ✅ เช็คและเพิ่ม Collider ถ้ายังไม่มี
        EnsurePhysicsComponents(slot.itemObject);

        // เพิ่ม Rigidbody ถ้ายังไม่มี
        Rigidbody rb = slot.itemObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = slot.itemObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // ⭐ เปลี่ยนเป็น ContinuousDynamic
        rb.mass = 1f; // ⭐ ตั้งค่า mass ให้ชัดเจน
        rb.linearDamping = 0.5f; // ⭐ เพิ่ม drag นิดหน่อย ไม่ให้ตกเร็วเกินไป

        Debug.Log($"🟡 Dropped '{slot.itemName}' from inventory");

        // ล้างช่องนี้
        slot.isEmpty = true;
        slot.itemObject = null;
        slot.itemName = "";
        slot.iconTexture = null;

        // หาช่องถัดไปที่มีของ หรือ clear มือ
        FindNextItem();
    }

    // ✅ ฟังก์ชันปาของ - เรียกจาก PlayerController
    public void ThrowCurrentItem(float force)
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Count) return;

        var slot = slots[selectedIndex];
        if (slot.isEmpty || slot.itemObject == null) return;

        // ปาของไปทางที่กล้องมอง
        Camera cam = Camera.main;
        Vector3 throwPos = cam.transform.position + cam.transform.forward * 0.5f;

        slot.itemObject.SetActive(true);
        slot.itemObject.transform.SetParent(null);
        slot.itemObject.transform.position = throwPos;
        slot.itemObject.transform.rotation = Quaternion.identity;

        // ✅ เช็คและเพิ่ม Collider ถ้ายังไม่มี
        EnsurePhysicsComponents(slot.itemObject);

        // เพิ่ม Rigidbody และปา
        Rigidbody rb = slot.itemObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = slot.itemObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // ⭐ เปลี่ยนเป็น ContinuousDynamic
        rb.mass = 1f; // ⭐ ตั้งค่า mass ให้ชัดเจน
        rb.linearDamping = 0.5f; // ⭐ เพิ่ม drag นิดหน่อย
        rb.AddForce(cam.transform.forward * force, ForceMode.Impulse);

        Debug.Log($"🚀 Threw '{slot.itemName}' with force {force}");

        // ล้างช่องนี้
        slot.isEmpty = true;
        slot.itemObject = null;
        slot.itemName = "";
        slot.iconTexture = null;

        // หาช่องถัดไปที่มีของ
        FindNextItem();
    }

    // ✅ หาของชิ้นถัดไปหลังวาง/ปา
    void FindNextItem()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].isEmpty)
            {
                selectedIndex = i;
                EquipSelectedItem();
                return;
            }
        }

        // ไม่มีของเหลือเลย
        selectedIndex = -1;
        ClearHandItem();
    }

    // ✅ เช็คว่ามีของในมือไหม
    public bool HasItemInHand()
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Count) return false;
        return !slots[selectedIndex].isEmpty && slots[selectedIndex].itemObject != null;
    }

    // ✅ เช็คและเพิ่ม Physics Components อัตโนมัติ
    void EnsurePhysicsComponents(GameObject obj)
    {
        // เช็ค Collider - ถ้าไม่มีให้เพิ่มแบบอัตโนมัติ
        Collider col = obj.GetComponent<Collider>();
        if (col == null)
        {
            // ลองหา MeshFilter เพื่อใช้ขนาดที่เหมาะสม
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                // ใช้ BoxCollider แทน MeshCollider เพราะเสถียรกว่า
                BoxCollider boxCol = obj.AddComponent<BoxCollider>();

                // คำนวณขนาด Collider จาก Mesh bounds
                Bounds bounds = meshFilter.sharedMesh.bounds;
                boxCol.center = bounds.center;
                boxCol.size = bounds.size;

                Debug.Log($"✅ Added BoxCollider (size: {bounds.size}) to {obj.name}");
            }
            else
            {
                // ไม่มี Mesh ให้ใช้ BoxCollider ขนาดมาตรฐาน
                BoxCollider boxCol = obj.AddComponent<BoxCollider>();
                boxCol.size = Vector3.one * 0.5f; // ขนาดเล็กกว่าเดิม
                Debug.Log($"✅ Added BoxCollider (default) to {obj.name}");
            }
        }
        else
        {
            Debug.Log($"✅ {obj.name} already has {col.GetType().Name}");
        }

        // ⭐ ปิด isTrigger ทุก Collider เพื่อให้ชนได้
        Collider[] allColliders = obj.GetComponents<Collider>();
        foreach (Collider c in allColliders)
        {
            if (c.isTrigger)
            {
                c.isTrigger = false;
                Debug.Log($"🔓 Disabled isTrigger on {c.GetType().Name} of {obj.name}");
            }
        }
    }

    void OnGUI()
    {
        if (openProgress < 0.01f) return;

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            float size = slot.currentSize * openProgress;
            Rect rect = new Rect(
                slot.position.x - size / 2,
                slot.position.y - size / 2,
                size,
                size
            );

            GUI.color = (i == selectedIndex) ? selectedColor : normalColor;
            GUI.DrawTexture(rect, slotBackground);

            if (!slot.isEmpty && slot.iconTexture != null)
            {
                Rect iconRect = new Rect(rect.x + 5, rect.y + 5, size - 10, size - 10);
                GUI.color = Color.white;
                GUI.DrawTexture(iconRect, slot.iconTexture, ScaleMode.ScaleToFit);
            }
        }

        GUI.color = Color.white;
    }

    // เพิ่มของเข้า Inventory
    public void AddItem(GameObject item, string name, Texture2D icon = null)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].isEmpty)
            {
                slots[i].itemObject = item;
                slots[i].itemName = name;
                slots[i].iconTexture = icon;
                slots[i].isEmpty = false;

                item.SetActive(false);
                item.transform.SetParent(null);

                // ถ้ายังไม่เลือกอะไรเลย → auto select
                if (selectedIndex == -1)
                {
                    selectedIndex = i;
                    EquipSelectedItem();
                }

                Debug.Log($"🟢 Added item '{name}' to slot #{i + 1}");
                return;
            }
        }

        Debug.Log("⚠️ Inventory full!");
    }
}