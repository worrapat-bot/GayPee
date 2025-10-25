using System.Collections.Generic;
using UnityEngine;

public class SimpleTodoList : MonoBehaviour
{
    [Header("ตั้งค่างาน - เพิ่มลดได้ที่นี่")]
    public List<TodoTask> tasks = new List<TodoTask>();

    [Header("Task Item Prefab")]
    public GameObject taskPrefab;

    [Header("ที่วางงาน")]
    public Transform taskContainer;

    void Start()
    {
        // ถ้ายังไม่มีงาน ให้เพิ่มตัวอย่าง
        if (tasks.Count == 0)
        {
            tasks.Add(new TodoTask { taskText = "ทำการบ้าน", isCompleted = false });
            tasks.Add(new TodoTask { taskText = "ซื้อของ", isCompleted = false });
            tasks.Add(new TodoTask { taskText = "ออกกำลังกาย", isCompleted = false });
        }

        ShowAllTasks();
    }

    void Update()
    {
        // อัพเดททุกเฟรมเพื่อไม่ให้หาย
        if (taskContainer.childCount != tasks.Count)
        {
            ShowAllTasks();
        }
    }

    public void ShowAllTasks()
    {
        // ลบของเก่าออก
        foreach (Transform child in taskContainer)
        {
            Destroy(child.gameObject);
        }

        // แสดงงานทั้งหมด
        for (int i = 0; i < tasks.Count; i++)
        {
            GameObject taskObj = Instantiate(taskPrefab, taskContainer);
            TaskItemSimple item = taskObj.GetComponent<TaskItemSimple>();

            if (item != null)
            {
                item.SetupTask(tasks[i], i, this);
            }
        }
    }

    public void ToggleTask(int index)
    {
        if (index >= 0 && index < tasks.Count)
        {
            tasks[index].isCompleted = !tasks[index].isCompleted;
            ShowAllTasks();
        }
    }

    public void RemoveTask(int index)
    {
        if (index >= 0 && index < tasks.Count)
        {
            tasks.RemoveAt(index);
            ShowAllTasks();
        }
    }
}