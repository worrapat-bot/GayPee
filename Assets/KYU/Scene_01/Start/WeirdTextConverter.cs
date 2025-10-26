using UnityEngine;
using TMPro; // ต้องใช้สำหรับ TextMeshPro
using System.Text; // สำหรับการจัดการ String ที่มีประสิทธิภาพ

public class WeirdTextConverter : MonoBehaviour
{
    [Header("Text Settings")]
    [Tooltip("Text Component ที่ต้องการเปลี่ยน")]
    [SerializeField] private TextMeshProUGUI targetText;
    
    [Tooltip("ข้อความต้นฉบับ")]
    [SerializeField] private string originalText = "Hello Unity 6!";
    
    // Offset (ความต่างของรหัส Unicode) ระหว่างตัวอักษรปกติกับตัวอักษรกว้างเต็ม
    private const int FULL_WIDTH_OFFSET = 65248; // รหัส Unicode FFE0

    private void Start()
    {
        // ตรวจสอบ
        if (targetText == null)
        {
            Debug.LogError("Target TextMeshProUGUI is not assigned!");
            return;
        }

        // แปลงข้อความและแสดงผล
        string weirdText = ConvertToFullWidth(originalText);
        targetText.text = weirdText;
    }

    // เมธอดสำหรับแปลงตัวอักษร
    public string ConvertToFullWidth(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        StringBuilder sb = new StringBuilder();

        foreach (char c in text)
        {
            // ตรวจสอบว่าตัวอักษรเป็นตัวอักษร/ตัวเลขภาษาอังกฤษปกติหรือไม่
            // ตัวอักษรปกติ (ASCII) อยู่ในช่วง 33 ถึง 126
            if (c >= '!' && c <= '~') 
            {
                // แปลงตัวอักษรปกติให้เป็นตัวอักษรกว้างเต็ม
                // โดยการเพิ่มค่า Offset เข้าไปในรหัส Unicode
                sb.Append((char)(c + FULL_WIDTH_OFFSET));
            }
            else
            {
                // ตัวอักษรอื่นๆ (เช่น ภาษาไทย, เว้นวรรค) ให้คงเดิม
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
    
    // เมธอดสาธารณะสำหรับให้คลาสอื่นเรียกใช้ได้
    public void SetOriginalText(string newText)
    {
        originalText = newText;
        targetText.text = ConvertToFullWidth(originalText);
    }
}