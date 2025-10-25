// UIManager.cs
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("ลาก GameObject (Panel) ทั้งหมดที่ต้องการ 'ซ่อน/ปิด' มาใส่ที่นี่")]
    // ใช้ Array เพื่อให้เพิ่ม Panel ได้หลายตัว และใช้ SerializeField เพื่อให้ปรับค่าใน Inspector ได้
    [SerializeField] private GameObject[] panelsToHide;

    void Awake()
    {
        // คำเตือนเผื่อลืมกำหนด Panel ใน Inspector
        if (panelsToHide == null || panelsToHide.Length == 0)
        {
            Debug.LogWarning("⚠️ Warning: No panels assigned on " + gameObject.name + ". This script is intended to hide panels.");
        }

        // เราจะไม่กำหนดให้ปิดทั้งหมดใน Awake() เหมือน UIShowPanel 
        // เพราะปกติ UIHidePanel จะถูกใช้ปิด Panel ที่เปิดอยู่แล้ว
    }

    /// <summary>
    /// ฟังก์ชันหลักสำหรับ 'ปิด/ซ่อน' ทุก panel ที่กำหนดไว้
    /// ใช้ผูกกับปุ่ม (OnClick)
    /// </summary>
    public void HidePanels()
    {
        if (panelsToHide == null || panelsToHide.Length == 0) return;

        foreach (var panel in panelsToHide)
        {
            if (panel != null)
            {
                // ใช้ SetActive(false) เพื่อปิดการแสดงผล
                panel.SetActive(false);
            }
        }
        
        Debug.Log("✅ All assigned panels have been hidden/deactivated.");
    }

    // ----------------------------------------------------------------
    // ฟังก์ชันเสริม (เหมือนกับ ShowPanels ใน Script เดิมของคุณ)
    // ----------------------------------------------------------------

    /// <summary>
    /// ฟังก์ชันเสริม: เปิดทุก panel ได้ด้วย (เผื่อใช้ตอนสลับ UI)
    /// </summary>
    public void ShowPanels()
    {
        if (panelsToHide == null || panelsToHide.Length == 0) return;

        foreach (var panel in panelsToHide)
        {
            if (panel != null)
            {
                // ใช้ SetActive(true) เพื่อเปิดการแสดงผล
                panel.SetActive(true);
            }
        }
    }
}