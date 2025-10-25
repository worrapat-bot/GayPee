using UnityEngine;

/// <summary>
/// Script นี้มีหน้าที่ 'เปิด' หรือ 'โชว์' GameObject (Panel) หลายตัวพร้อมกัน
/// เมื่อถูกเรียกใช้จากปุ่ม (เช่น OnClick)
/// </summary>
public class UIShowPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("ลาก GameObject (Panel) ทั้งหมดที่ต้องการ 'โชว์' มาใส่ที่นี่")]
    [SerializeField] private GameObject[] panelsToShow;

    void Awake()
    {
        if (panelsToShow == null || panelsToShow.Length == 0)
        {
            Debug.LogWarning("⚠️ Warning: No panels assigned on " + gameObject.name);
            return;
        }

        // ปิดทุก panel ตอนเริ่มเกม
        foreach (var panel in panelsToShow)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }

    /// <summary>
    /// ฟังก์ชันสำหรับ 'โชว์' ทุก panel ที่กำหนดไว้
    /// ใช้ใน OnClick ของปุ่ม
    /// </summary>
    public void ShowPanels()
    {
        if (panelsToShow == null || panelsToShow.Length == 0) return;

        foreach (var panel in panelsToShow)
        {
            if (panel != null)
                panel.SetActive(true);
        }
    }

    /// <summary>
    /// ฟังก์ชันเสริม: ซ่อนทุก panel ได้ด้วย (เผื่อใช้ตอนปิด UI)
    /// </summary>
    public void HidePanels()
    {
        if (panelsToShow == null || panelsToShow.Length == 0) return;

        foreach (var panel in panelsToShow)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }
}
