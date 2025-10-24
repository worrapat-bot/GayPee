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

        Rigidbody rb = slot.itemObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
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
        rb.linearDamping = 0.5f;

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
        rb.linearDamping = 0.5f;
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
    }

    // ✅ ฟังก์ชันใหม่: ทำให้ item ที่ drop แล้วสามารถเก็บได้อีก
    void MakePickupable(GameObject item, string itemName)
    {
        if (item.tag != "Pickup")
            item.tag = "Pickup";

        var interact = item.GetComponent<SimpleItemInteract>();
        if (interact == null)
        {
            interact = item.AddComponent<SimpleItemInteract>();
        }

        interact.itemID = itemName;
        interact.enabled = true;

        var rb = item.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        var col = item.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Debug.Log($"🟢 '{itemName}' is now pickupable again!");
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
        // 🔹 หาช่องว่างแรก
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].isEmpty)
            {
                // 🔹 ทำสำเนา texture เพื่อป้องกัน bug icon ดำ
                if (icon != null)
                {
                    Texture2D fixedTex = new Texture2D(icon.width, icon.height, TextureFormat.RGBA32, false);
                    fixedTex.SetPixels(icon.GetPixels());
                    fixedTex.Apply();
                    icon = fixedTex;
                }

                // ✅ ถ้ามี SimpleItemInteract ให้ปิด script ชั่วคราวก่อน
                var interact = item.GetComponent<SimpleItemInteract>();
                if (interact != null)
                    interact.enabled = false;

                // 🔹 เซ็ตข้อมูลลงช่อง
                slots[i].itemObject = item;
                slots[i].itemName = name;
                slots[i].iconTexture = icon;
                slots[i].isEmpty = false;

                // ✅ แก้บั๊กช่องแรก (อย่า SetActive(false) ถ้ามันเป็น Flashlight)
                if (!name.ToLower().Contains("flashlight"))
                    item.SetActive(false);

                item.transform.SetParent(null);

                Debug.Log($"🟢 Added item '{name}' to slot #{i + 1}");

                // ✅ ถ้ายังไม่มีของในมือ (selectedIndex == -1) ให้เลือกช่องนี้
                if (selectedIndex == -1)
                {
                    selectedIndex = i;
                    EquipSelectedItem();
                }

                return;
            }
        }

        // 🔹 ถ้าไม่มีช่องว่างเลย
        Debug.Log("⚠️ Inventory full! Could not add item: " + name);
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
