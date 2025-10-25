using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class TaskItem
{
    public string taskName;      // ข้อความของภารกิจ
    public bool completed;       // สถานะทำสำเร็จหรือไม่
}

public class ListManager : MonoBehaviour
{
    public Transform contentParent;         // Content ของ ScrollView
    public GameObject listItemPrefab;       // Prefab ของแต่ละ Item

    public List<TaskItem> tasks = new List<TaskItem>(); // รายการใน Inspector

    void Start()
    {
        RefreshList();
    }

    // สร้าง UI ตามรายการ tasks
    public void RefreshList()
    {
        // ลบของเก่าก่อน
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // สร้าง UI ใหม่
        foreach (TaskItem task in tasks)
        {
            GameObject item = Instantiate(listItemPrefab, contentParent);
            Toggle toggle = item.GetComponentInChildren<Toggle>();
            TMP_Text text = item.GetComponentInChildren<TMP_Text>();

            text.text = task.taskName;
            toggle.isOn = task.completed;

            // เมื่อกด toggle ให้เปลี่ยนสถานะ
            toggle.onValueChanged.AddListener((value) => task.completed = value);
        }
    }
}
