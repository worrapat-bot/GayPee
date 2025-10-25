using UnityEngine;

public class QuestPaperList : MonoBehaviour
{
    [Header("Position Settings")]
    public float rightMargin = 20f;
    public float topMargin = 20f;

    [Header("Animation Settings")]
    public float slideSpeed = 8f;
    public KeyCode toggleKey = KeyCode.Tab;

    private GUIStyle paperStyle;
    private GUIStyle titleStyle;
    private GUIStyle itemStyle;
    private GUIStyle completedStyle;
    private Texture2D paperTexture;

    private bool isOpen = false;
    private float slideProgress = 0f;

    // สถานะ quest
    private bool quest1Complete = false; // มี Key
    private bool quest2Complete = false; // เปิดประตู Key
    private bool quest3Complete = false; // เปิดประตู Crowbar

    private RadialInventoryVertical inventory;

    void Start()
    {
        CreatePaperTexture();
        inventory = FindObjectOfType<RadialInventoryVertical>();
    }

    void CreatePaperTexture()
    {
        paperTexture = new Texture2D(1, 1);
        paperTexture.SetPixel(0, 0, new Color(0.95f, 0.92f, 0.8f, 0.6f));
        paperTexture.Apply();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isOpen = !isOpen;
        }

        float target = isOpen ? 1f : 0f;
        slideProgress = Mathf.Lerp(slideProgress, target, Time.deltaTime * slideSpeed);

        // เช็ค quest 1: มี Key ในคลังหรือไม่
        if (!quest1Complete && inventory != null)
        {
            if (inventory.HasItem("Key"))
            {
                quest1Complete = true;
                Debug.Log("✅ Quest 1 Complete: You have the Key!");
            }
        }
    }

    // ฟังก์ชันสำหรับ UniversalDoor เรียก
    public void OnDoorUnlocked(string itemUsed)
    {
        if (itemUsed == "Key" && !quest2Complete)
        {
            quest2Complete = true;
            Debug.Log("✅ Quest 2 Complete: Door unlocked with Key!");
        }
        else if (itemUsed == "Crowbar" && !quest3Complete)
        {
            quest3Complete = true;
            Debug.Log("✅ Quest 3 Complete: Door unlocked with Crowbar!");
        }
    }

    void OnGUI()
    {
        if (slideProgress < 0.01f) return;

        if (paperStyle == null)
        {
            paperStyle = new GUIStyle(GUI.skin.box);
            paperStyle.normal.background = paperTexture;
            paperStyle.border = new RectOffset(10, 10, 10, 10);
            paperStyle.padding = new RectOffset(15, 15, 15, 15);

            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 20;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(0.2f, 0.15f, 0.1f);
            titleStyle.alignment = TextAnchor.UpperCenter;

            itemStyle = new GUIStyle(GUI.skin.label);
            itemStyle.fontSize = 14;
            itemStyle.normal.textColor = new Color(0.3f, 0.2f, 0.15f);
            itemStyle.padding = new RectOffset(5, 5, 5, 5);

            // Style สำหรับข้อที่เสร็จแล้ว (ขีดฆ่า)
            completedStyle = new GUIStyle(itemStyle);
            completedStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 0.7f); // สีเทา
        }

        float paperWidth = 300f;
        float paperHeight = 200f;

        float hiddenX = Screen.width;
        float visibleX = Screen.width - paperWidth - rightMargin;
        float currentX = Mathf.Lerp(hiddenX, visibleX, slideProgress);

        Rect paperRect = new Rect(
            currentX,
            topMargin,
            paperWidth,
            paperHeight
        );

        GUI.Box(paperRect, "", paperStyle);

        GUILayout.BeginArea(paperRect);

        GUILayout.Space(10);
        GUILayout.Label("QUEST LIST", titleStyle);
        GUILayout.Space(10);

        DrawLine(new Color(0.6f, 0.5f, 0.4f));
        GUILayout.Space(10);

        // วาด quest แต่ละข้อ
        DrawQuestItem("- Make the key by using furnace", quest1Complete);
        DrawQuestItem("- Find door that use key", quest2Complete);
        DrawQuestItem("- Find door that use crowbar", quest3Complete);

        GUILayout.Space(10);

        if (slideProgress > 0.9f)
        {
            GUIStyle hintStyle = new GUIStyle(itemStyle);
            hintStyle.fontSize = 11;
            hintStyle.normal.textColor = new Color(0.5f, 0.4f, 0.3f, 0.7f);
            hintStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("Press TAB to hide", hintStyle);
        }

        GUILayout.EndArea();
    }

    void DrawQuestItem(string text, bool isCompleted)
    {
        GUIStyle style = isCompleted ? completedStyle : itemStyle;

        // วาดข้อความ
        Rect textRect = GUILayoutUtility.GetRect(new GUIContent(text), style);
        GUI.Label(textRect, text, style);

        // ถ้าเสร็จแล้ว วาดเส้นขีดฆ่า
        if (isCompleted)
        {
            Rect lineRect = new Rect(
                textRect.x + 5,
                textRect.y + textRect.height / 2,
                textRect.width - 10,
                2
            );
            GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            GUI.DrawTexture(lineRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        GUILayout.Space(5);
    }

    void DrawLine(Color color)
    {
        Rect rect = GUILayoutUtility.GetRect(1, 2);
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}