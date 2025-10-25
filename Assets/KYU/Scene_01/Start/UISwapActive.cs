using UnityEngine;

/// <summary>
/// Script นี้ใช้สำหรับ 'สลับ' การมองเห็นของ UI สองชิ้น
/// เมื่อถูกเรียกใช้ (เช่น จากปุ่ม) จะ 'เปิด' อันหนึ่ง และ 'ปิด' อีกอันหนึ่ง
/// </summary>
public class UISwapActive : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("ลาก GameObject (Panel) ที่คุณต้องการ 'โชว์' (Show) มาใส่ที่นี่")]
    [SerializeField] private GameObject panelToShow;

    [Tooltip("ลาก GameObject (Panel) ที่คุณต้องการ 'ลบ' (Hide/ซ่อน) มาใส่ที่นี่")]
    [SerializeField] private GameObject panelToHide;

    void Awake()
    {
        // ตรวจสอบว่าลาก UI มาใส่ครบหรือยัง
        if (panelToShow == null || panelToHide == null)
        {
            Debug.LogError("Error: 'Panel To Show' or 'Panel To Hide' is not assigned on " + gameObject.name);
        }
    }

    /// <summary>
    /// ฟังก์ชันสำหรับ 'สลับ' UI (ใช้ใน OnClick ของปุ่ม)
    /// </summary>
    public void PerformSwap()
    {
        // 1. โชว์ Panel ที่ต้องการ (Show)
        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
        }

        // 2. ลบ (ซ่อน) Panel ที่ไม่ต้องการ (Hide)
        if (panelToHide != null)
        {
            panelToHide.SetActive(false);
        }
    }
}