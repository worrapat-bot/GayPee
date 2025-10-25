using UnityEngine;

/// <summary>
/// Script นี้มีหน้าที่ 'เปิด' หรือ 'โชว์' GameObject (Panel) ที่กำหนด
/// เมื่อถูกเรียกใช้จากปุ่ม
/// </summary>
public class UIShowPanel : MonoBehaviour
{
    [Header("UI Element")]
    [Tooltip("ลาก GameObject (Panel) ที่คุณต้องการ 'โชว์' มาใส่ที่นี่")]
    [SerializeField] private GameObject panelToShow;

    void Awake()
    {
        // ตรวจสอบว่าลาก Panel มาใส่หรือยัง
        if (panelToShow == null)
        {
            Debug.LogError("Error: 'Panel To Show' is not assigned on " + gameObject.name);
        }
        else
        {
            // ตั้งค่าเริ่มต้นให้ Panel 'ปิด' (ซ่อน) อยู่
            panelToShow.SetActive(false);
        }
    }

    /// <summary>
    /// ฟังก์ชันสำหรับ 'โชว์' Panel (ใช้ใน OnClick ของปุ่ม)
    /// </summary>
    public void ShowPanel()
    {
        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
        }
    }
}