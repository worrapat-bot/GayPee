using UnityEngine;
using TMPro;
using System.Text;
using System.Collections.Generic;
using System.Linq; // ต้องใช้สำหรับ LINQ และการจัดการ List/Array

public class ZalgoTextEffect : MonoBehaviour
{
    [Header("Text Settings")]
    [Tooltip("Text Component ที่ต้องการเปลี่ยน")]
    [SerializeField] private TextMeshProUGUI targetText;
    
    [Tooltip("ข้อความต้นฉบับ")]
    [SerializeField] private string originalText = "Beware the curse of the old king";
    
    [Header("Effect Settings")]
    [Tooltip("จำนวนครั้งสูงสุดในการสุ่มใส่ Zalgo Character ต่อตัวอักษร")]
    [SerializeField] private int maxZalgoPerChar = 5;
    
    [Tooltip("เปิดใช้งานการสลับตัวอักษรในคำ")]
    [SerializeField] private bool enableShuffling = true;

    // ... (ชุดตัวอักษร Zalgo Top/Mid/Down เหมือนเดิม ไม่ต้องเปลี่ยน)
    private char[] zalgoTop = new char[] 
    {
        '\u0304', '\u0306', '\u030C', '\u033F', '\u0346', '\u034A', '\u034C', 
        '\u0353', '\u0357', '\u0359', '\u035B', '\u035E', '\u0360', '\u0362', 
        '\u0364', '\u0366', '\u0336'
    };
    private char[] zalgoMid = new char[] 
    {
        '\u0315', '\u0319', '\u034E', '\u0352', '\u0355', '\u0358', '\u035D', 
        '\u036B', '\u036D', '\u036F', '\u0338', '\u0337'
    };
    private char[] zalgoDown = new char[] 
    {
        '\u0323', '\u032B', '\u032E', '\u032F', '\u0333', '\u0334', '\u033E', 
        '\u034F', '\u035F', '\u033A', '\u0347'
    };
    // ...

    private void Start()
    {
        if (targetText == null)
        {
            Debug.LogError("Target TextMeshProUGUI is not assigned!");
            return;
        }

        string intermediateText = originalText;
        
        // 1. (Option) ทำการสลับตัวอักษรในคำก่อน
        if (enableShuffling)
        {
            intermediateText = ShuffleWords(originalText);
        }
        
        // 2. ใส่คำสาป Zalgo Text
        string cursedText = ApplyZalgoEffect(intermediateText);
        
        targetText.text = cursedText;
    }

    // ====================================================================
    // 🛠️ เมธอดใหม่: การสลับตัวอักษรในแต่ละคำ (รักษาตัวอักษรตัวแรกและตัวสุดท้าย)
    // ====================================================================

    private string ShuffleWords(string text)
    {
        // แยกข้อความเป็นคำๆ โดยใช้ช่องว่างเป็นตัวแบ่ง
        string[] words = text.Split(' ');
        
        StringBuilder shuffledBuilder = new StringBuilder();
        
        System.Random random = new System.Random(); 

        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];
            
            // ไม่สลับคำที่สั้นเกินไป (เช่น มี 3 ตัวอักษรหรือน้อยกว่า)
            if (word.Length > 3)
            {
                // เก็บตัวอักษรกลางของคำ (ไม่รวมตัวแรกและตัวสุดท้าย)
                string middle = word.Substring(1, word.Length - 2);
                
                // สลับตำแหน่งตัวอักษรกลาง
                string shuffledMiddle = new string(middle.OrderBy(c => random.Next()).ToArray());
                
                // ประกอบคำใหม่: ตัวแรก + ตัวกลางที่ถูกสลับ + ตัวสุดท้าย
                string newWord = word[0] + shuffledMiddle + word[word.Length - 1];
                shuffledBuilder.Append(newWord);
            }
            else
            {
                // คำสั้นๆ หรือว่าง ให้คงเดิม
                shuffledBuilder.Append(word);
            }

            // ใส่ช่องว่างกลับเข้าไป (ยกเว้นคำสุดท้าย)
            if (i < words.Length - 1)
            {
                shuffledBuilder.Append(" ");
            }
        }

        return shuffledBuilder.ToString();
    }
    
    // ====================================================================
    // 💀 เมธอดเดิม: การใส่ Zalgo Effect (ปรับให้รับ String ที่ถูก Shuffle มาแล้ว)
    // ====================================================================
    
    // 
    public string ApplyZalgoEffect(string text)
    {
        // (โค้ด Zalgo Text เหมือนเดิม)
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        StringBuilder cursedBuilder = new StringBuilder();
        System.Random random = new System.Random(); 

        foreach (char originalChar in text)
        {
            cursedBuilder.Append(originalChar);

            if (char.IsWhiteSpace(originalChar) || char.IsPunctuation(originalChar))
            {
                continue;
            }

            int zalgoCount = random.Next(1, maxZalgoPerChar + 1);

            for (int i = 0; i < zalgoCount; i++)
            {
                int type = random.Next(0, 3);
                char zalgoChar = ' ';
                
                switch (type)
                {
                    case 0:
                        zalgoChar = zalgoTop[random.Next(zalgoTop.Length)];
                        break;
                    case 1:
                        zalgoChar = zalgoMid[random.Next(zalgoMid.Length)];
                        break;
                    case 2:
                        zalgoChar = zalgoDown[random.Next(zalgoDown.Length)];
                        break;
                }
                cursedBuilder.Append(zalgoChar);
            }
        }
        return cursedBuilder.ToString();
    }
}