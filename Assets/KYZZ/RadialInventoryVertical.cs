using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Drop Settings")]
    [SerializeField] private float dropDistance = 2f;

    private bool isOpen;
    private List<InventorySlot> slots = new List<InventorySlot>();
    private int selectedIndex = -1;
    private float openProgress;

    [Header("Input")]
    [SerializeField] private KeyCode dropKey = KeyCode.Q;

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

        // ✅ เปิด–ปิด Inventory อัตโนมัติหนึ่งรอบตอนเริ่มเกม
        StartCoroutine(AutoInitInventory());
    }

    private IEnumerator AutoInitInventory()
    {
        yield return null; // รอเฟรมแรกให้ระบบโหลดกล้อง/Player เสร็จก่อน
        isOpen = true;
        UpdateSlotPositions();
        isOpen = false;
        openProgress = 0f;
        Debug.Log("✅ Inventory initialized automatically at game start");
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

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetKeyDown(dropKey))
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
            float centerOffset = (slotCount - 1) * spacing / 2f;
            float yOffset = i * spacing * openProgress - centerOffset * openProgress;
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
                // ถ้า object อยู่ในมือ ให้คืนสภาพ (unbind และ deactivate)
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
        slot.itemObject.transform.localRotation = Quaternion.Euler(90f, 90f, 90f);

        Rigidbody rb = slot.itemObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // ปิดฟิสิกส์ขณะถือ
            rb.isKinematic = true;
            // ปรับความเร็วเป็นศูนย์ - property name อาจต่างกันตาม Unity เวอร์ชัน
            // หาก Unity6 ใช้ velocity, angularVelocity ให้ใช้แบบนี้ (compatibility)
#if UNITY_202x_OR_OLDER
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
#else
            try
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            catch
            {
                // ถ้าชื่อ property ต่างใน Unity6 ก็ไม่ทำอะไรมาก (โค้ดยังคงปลอดภัย)
            }
#endif
        }

        Collider col = slot.itemObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    public void DropCurrentItem()
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Count) return;

        var slot = slots[selectedIndex];
        if (slot.isEmpty || slot.itemObject == null) return;

        Camera cam = Camera.main;
        Vector3 dropPos = cam.transform.position + cam.transform.forward * dropDistance;
        dropPos.y += 0.5f;

        GameObject droppedItem = slot.itemObject;
        droppedItem.SetActive(true);
        droppedItem.transform.SetParent(null);
        droppedItem.transform.position = dropPos;
        droppedItem.transform.rotation = Quaternion.identity;

        EnsurePhysicsComponents(droppedItem);

        Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.mass = 1f;
        // หาก Unity ไม่ใช้ linearDamping ให้ใช้ drag เป็น fallback
#if UNITY_202x_OR_OLDER
        rb.drag = 0.5f;
#else
        try { rb.isKinematic = false; } catch { } // noop to avoid compile warning for conditional; actual fallback above
#endif

        Collider col = droppedItem.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        // ✅ ทำให้เก็บได้อีก
        MakePickupable(droppedItem, slot.itemName);

        Debug.Log($"🟡 Dropped '{slot.itemName}' from inventory");

        slot.isEmpty = true;
        slot.itemObject = null;
        slot.itemName = "";
        slot.iconTexture = null;

        FindNextItem();
    }

    public void ThrowCurrentItem(float force)
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Count) return;

        var slot = slots[selectedIndex];
        if (slot.isEmpty || slot.itemObject == null) return;

        Camera cam = Camera.main;
        Vector3 throwPos = cam.transform.position + cam.transform.forward * 0.5f;

        GameObject thrownItem = slot.itemObject;
        thrownItem.SetActive(true);
        thrownItem.transform.SetParent(null);
        thrownItem.transform.position = throwPos;
        thrownItem.transform.rotation = Quaternion.identity;

        EnsurePhysicsComponents(thrownItem);

        Rigidbody rb = thrownItem.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.mass = 1f;
        // fallback to drag if linearDamping not available
#if UNITY_202x_OR_OLDER
        rb.drag = 0.5f;
#endif
        rb.AddForce(cam.transform.forward * force, ForceMode.Impulse);

        Collider col = thrownItem.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        // ✅ ทำให้เก็บได้อีก
        MakePickupable(thrownItem, slot.itemName);

        Debug.Log($"🚀 Threw '{slot.itemName}' with force {force}");

        slot.isEmpty = true;
        slot.itemObject = null;
        slot.itemName = "";
        slot.iconTexture = null;

        FindNextItem();
    }

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

        selectedIndex = -1;
        ClearHandItem();
    }

    public bool HasItemInHand()
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Count) return false;
        return !slots[selectedIndex].isEmpty && slots[selectedIndex].itemObject != null;
    }

    void EnsurePhysicsComponents(GameObject obj)
    {
        Collider col = obj.GetComponent<Collider>();
        if (col == null)
        {
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                BoxCollider boxCol = obj.AddComponent<BoxCollider>();
                Bounds bounds = meshFilter.sharedMesh.bounds;
                boxCol.center = bounds.center;
                boxCol.size = bounds.size;
                Debug.Log($"✅ Added BoxCollider (size: {bounds.size}) to {obj.name}");
            }
            else
            {
                BoxCollider boxCol = obj.AddComponent<BoxCollider>();
                boxCol.size = Vector3.one * 0.5f;
                Debug.Log($"✅ Added BoxCollider (default) to {obj.name}");
            }
        }

        Collider[] allColliders = obj.GetComponents<Collider>();
        foreach (Collider c in allColliders)
        {
            if (c.isTrigger)
            {
                c.isTrigger = false;
                Debug.Log($"🔓 Disabled isTrigger on {c.GetType().Name} of {obj.name}");
            }
            c.enabled = true;
        }

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody>();
        }
        // don't modify other rb properties here; caller will set mode/mass as needed
    }

    // ✅ ฟังก์ชันใหม่: ทำให้ item ที่ drop แล้วสามารถเก็บได้อีก
    private void MakePickupable(GameObject item, string itemName)
    {
        if (item == null) return;

        // แก้ tag
        if (item.tag != "Pickup") item.tag = "Pickup";

        // เปิด object
        item.SetActive(true);

        // รีเซ็ต/ลบ SimpleItemInteract ถ้ามี เพื่อให้ instance ใหม่สะอาด
        var oldInteract = item.GetComponent<SimpleItemInteract>();
        if (oldInteract != null)
        {
            Destroy(oldInteract);
        }

        // เพิ่ม instance ใหม่ของ SimpleItemInteract (จะเรียก Start ของมัน)
        var interact = item.AddComponent<SimpleItemInteract>();
        interact.itemID = itemName;
        interact.interactKey = KeyCode.E; // ถ้าต้องการ override default
        interact.enabled = true;

        // เปิด collider และ rigidbody
        var col = item.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = false;
        }

        var rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            try
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            catch
            {
                try { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; } catch { }
            }
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        else
        {
            // ถ้าไม่มี Rigidbody ก็เพิ่ม (เพื่อให้ physics ทำงานตอน drop)
            rb = item.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        // คืนไอคอนจาก slot ถ้ามี
        foreach (var slot in slots)
        {
            if (!slot.isEmpty && slot.itemName == itemName && slot.iconTexture != null)
            {
                Sprite restoredSprite = Sprite.Create(
                    slot.iconTexture,
                    new Rect(0, 0, slot.iconTexture.width, slot.iconTexture.height),
                    new Vector2(0.5f, 0.5f)
                );
                interact.itemIcon = restoredSprite;
                break;
            }
        }

        // วางไว้หน้าผู้เล่นเล็กน้อย
        if (Camera.main != null)
        {
            Transform cam = Camera.main.transform;
            item.transform.position = cam.position + cam.forward * 1.0f + Vector3.down * 0.3f;
        }

        // รีเซ็ต rotation เล็กน้อยเพื่อกันโมเดลตั้งตรงแปลก ๆ
        item.transform.rotation = Quaternion.identity;

        Debug.Log($"🟢 '{itemName}' is now pickupable again (MakePickupable)");
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
                Rect iconRect = new Rect(rect.x + 2, rect.y + 2, size - 4, size - 4);
                GUI.color = Color.white;
                GUI.DrawTexture(iconRect, slot.iconTexture, ScaleMode.StretchToFill);
            }
        }

        GUI.color = Color.white;
    }
    public void AddItem(GameObject item, string name, Texture2D icon = null)
    {
        // ป้องกัน null
        if (item == null)
        {
            Debug.LogWarning("AddItem called with null item. Storing icon/name only.");
            // หาช่องว่างแล้วเก็บข้อมูลเบื้องต้น (ไม่มีโมเดล)
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].isEmpty)
                {
                    if (icon != null)
                    {
                        Texture2D fixedTex = new Texture2D(icon.width, icon.height, TextureFormat.RGBA32, false);
                        fixedTex.SetPixels(icon.GetPixels());
                        fixedTex.Apply();
                        icon = fixedTex;
                    }

                    slots[i].itemObject = null;
                    slots[i].itemName = name;
                    slots[i].iconTexture = icon;
                    slots[i].isEmpty = false;

                    Debug.Log($"🟡 Stored icon-only item '{name}' into slot #{i + 1}");

                    selectedIndex = i;
                    // ไม่มี object ให้ Equip แต่เรียก ClearHandItem เพื่อความแน่นอน
                    ClearHandItem();
                    return;
                }
            }

            Debug.Log("⚠️ Inventory full! Could not add item: " + name);
            return;
        }

        // หาช่องว่างแรก
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].isEmpty)
            {
                // ทำสำเนา texture เพื่อป้องกัน bug icon ดำ
                if (icon != null)
                {
                    Texture2D fixedTex = new Texture2D(icon.width, icon.height, TextureFormat.RGBA32, false);
                    fixedTex.SetPixels(icon.GetPixels());
                    fixedTex.Apply();
                    icon = fixedTex;
                }

                // 1) เอาของจริงเข้ามือทันที
                try
                {
                    item.transform.SetParent(handPoint);
                    item.transform.localPosition = Vector3.zero;
                    // ให้หมุนเริ่มต้นเป็นแนวนอน (ถ้าต้องการปรับสำหรับบางชิ้น ให้แก้ที่ prefab)
                    item.transform.localRotation = Quaternion.identity;
                }
                catch { /* ป้องกันกรณี parent null */ }

                // ปิด physics ขณะถือ
                var rbReal = item.GetComponent<Rigidbody>();
                if (rbReal != null)
                {
                    rbReal.isKinematic = true;
                    rbReal.useGravity = false;
                    try
                    {
                        // compatibility: some Unity versions use velocity, some linearVelocity
                        rbReal.linearVelocity = Vector3.zero;
                        rbReal.angularVelocity = Vector3.zero;
                    }
                    catch
                    {
                        try { rbReal.linearVelocity = Vector3.zero; rbReal.angularVelocity = Vector3.zero; } catch { }
                    }
                }

                // ปิด collider ของของจริงขณะถือ
                Collider[] colsReal = item.GetComponents<Collider>();
                foreach (var c in colsReal)
                    c.enabled = false;

                // 2) สร้างสำเนาไว้ใน inventory (สำเนาจะถูกเก็บไว้และปิดอยู่)
                GameObject stored = Instantiate(item);
                stored.name = item.name + "_INV";

                // ลบ component SimpleItemInteract บนสำเนา เพื่อไม่ให้สำเนาเป็น pickup object
                var interactComp = stored.GetComponent<SimpleItemInteract>();
                if (interactComp != null) Destroy(interactComp);

                // ตั้งสถานะฟิสิกส์ของสำเนาให้ปลอดภัย (ไม่ขยับ)
                var rbStored = stored.GetComponent<Rigidbody>();
                if (rbStored != null)
                {
                    rbStored.isKinematic = true;
                    rbStored.useGravity = false;
                    try
                    {
                        rbStored.linearVelocity = Vector3.zero;
                        rbStored.angularVelocity = Vector3.zero;
                    }
                    catch
                    {
                        try { rbStored.linearVelocity = Vector3.zero; rbStored.angularVelocity = Vector3.zero; } catch { }
                    }
                }

                Collider[] colsStored = stored.GetComponents<Collider>();
                foreach (var c in colsStored)
                    c.enabled = false;

                // ปิดสำเนาและแยกออกจาก parent
                stored.SetActive(false);
                stored.transform.SetParent(null);

                // 3) เก็บข้อมูลลง slot
                slots[i].itemObject = stored;
                slots[i].itemName = name;
                slots[i].iconTexture = icon;
                slots[i].isEmpty = false;

                Debug.Log($"🟢 Added item '{name}' to slot #{i + 1} (real -> hand; stored copy saved)");

                // 4) ทำให้ของจริงค่อยๆถูกลบหลังหน่วงสั้น ๆ (เพื่อกัน race condition)
                StartCoroutine(DestroyRealItemAfterDelay(item, 0.25f));

                // 5) **เลือกช่องใหม่เสมอ** แล้ว equip ให้ชัวร์
                selectedIndex = i;
                EquipSelectedItem();

                return;
            }
        }

        // ถ้าไม่มีช่องว่าง
        Debug.Log("⚠️ Inventory full! Could not add item: " + name);
    }


    private IEnumerator DestroyRealItemAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
            Destroy(obj);
    }


    public bool HasItem(string itemName)
    {
        foreach (var slot in slots)
        {
            if (!slot.isEmpty && slot.itemName == itemName)
                return true;
        }
        return false;
    }

    public string GetCurrentItemName()
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Count) return "";
        var slot = slots[selectedIndex];
        if (slot.isEmpty) return "";
        return slot.itemName;
    }

    public void RemoveCurrentItem()
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Count) return;
        var slot = slots[selectedIndex];
        if (slot.isEmpty) return;

        if (slot.itemObject != null)
        {
            Destroy(slot.itemObject);
        }

        Debug.Log($"🗑️ Removed '{slot.itemName}' from inventory");

        slot.isEmpty = true;
        slot.itemObject = null;
        slot.itemName = "";
        slot.iconTexture = null;

        FindNextItem();
    }
}