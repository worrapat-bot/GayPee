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
            float yOffset = i * spacing * openProgress; // ✅ เอา +1 ออก ให้เริ่มจาก 0
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

    // ✅ เวอร์ชันสุดนิ่ง: ใส่ของลงช่องแรก (1) แน่ๆ และถืออัตโนมัติถ้ายังไม่มีของในมือ
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

                // ✅ ถ้ายังไม่เลือกอะไรเลย → auto select ช่องนี้
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
