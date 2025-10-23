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
    [SerializeField] private Color normalColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    [SerializeField] private Color hoverColor = new Color(0.8f, 0.8f, 0.2f, 0.9f);
    [SerializeField] private Color selectedColor = new Color(0.2f, 0.8f, 0.2f, 1f);

    [Header("Player Hand Reference")]
    public Transform handPoint;

    private bool isOpen;
    private List<InventorySlot> slots = new List<InventorySlot>();
    private int selectedIndex = -1;
    private float openProgress;
    private Camera cam;

    private class InventorySlot
    {
        public Vector2 position;
        public float currentSize;
        public string itemName;
        public GameObject itemObject;
        public Texture2D iconTexture;
        public bool isEmpty;

        public InventorySlot(string name = "", GameObject obj = null, Texture2D icon = null)
        {
            itemName = name;
            itemObject = obj;
            iconTexture = icon;
            isEmpty = string.IsNullOrEmpty(name);
            currentSize = 0f;
        }
    }

    void Start()
    {
        cam = Camera.main;
        InitializeSlots();
    }

    void InitializeSlots()
    {
        slots.Clear();
        for (int i = 0; i < slotCount; i++)
        {
            slots.Add(new InventorySlot());
        }
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
            slots[i].position = screenPosition + new Vector2(0, -yOffset);
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

            Debug.Log($"Selected slot: {selectedIndex}");
        }

        for (int i = 0; i < slots.Count; i++)
        {
            float targetSize = (i == selectedIndex) ? hoverSize : normalSize;
            slots[i].currentSize = Mathf.Lerp(slots[i].currentSize, targetSize, Time.unscaledDeltaTime * animSpeed);
        }
    }

    void EquipSelectedItem()
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Count) return;
        var slot = slots[selectedIndex];
        if (slot.isEmpty || slot.itemObject == null) return;

        slot.itemObject.transform.SetParent(handPoint);
        slot.itemObject.transform.localPosition = Vector3.zero;
        slot.itemObject.transform.localRotation = Quaternion.identity;

        Debug.Log($"Equipped item: {slot.itemName}");
    }

    void OnGUI()
    {
        if (openProgress < 0.01f) return;

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];
            float size = slot.currentSize * openProgress;
            Rect rect = new Rect(
                slot.position.x - size / 2,
                slot.position.y - size / 2,
                size,
                size
            );

            Color slotColor = normalColor;
            if (i == selectedIndex) slotColor = selectedColor;
            slotColor.a *= openProgress;

            GUI.color = slotColor;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);

            // ✅ ถ้ามี icon ให้แสดงรูปของไอเท็ม
            if (!slot.isEmpty && slot.iconTexture != null)
            {
                Rect iconRect = new Rect(rect.x + 5, rect.y + 5, size - 10, size - 10);
                GUI.color = Color.white;
                GUI.DrawTexture(iconRect, slot.iconTexture, ScaleMode.ScaleToFit);
            }
        }

        GUI.color = Color.white;
    }

    // ✅ เรียกจาก InteractItem ตอนเก็บของ
    public void AddItem(GameObject item, string name, Texture2D icon = null)
    {
        foreach (var slot in slots)
        {
            if (slot.isEmpty)
            {
                slot.itemObject = item;
                slot.itemName = name;
                slot.iconTexture = icon;
                slot.isEmpty = false;

                // ✅ เดิมคือจะถือของทันที
                // selectedIndex = slots.IndexOf(slot);
                // EquipSelectedItem();

                Debug.Log($"Added item: {name}");
                return;
            }
        }
    }

}
