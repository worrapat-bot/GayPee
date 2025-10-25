using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskItemSimple : MonoBehaviour
{
    public Toggle checkbox;
    public TextMeshProUGUI taskText;
    public Button deleteButton;

    private TodoTask task;
    private int taskIndex;
    private SimpleTodoList manager;

    public void SetupTask(TodoTask taskData, int index, SimpleTodoList listManager)
    {
        task = taskData;
        taskIndex = index;
        manager = listManager;

        // แสดงข้อความ
        taskText.text = task.taskText;
        checkbox.isOn = task.isCompleted;

        // ถ้าเสร็จแล้วให้ขีดฆ่า
        if (task.isCompleted)
        {
            taskText.fontStyle = FontStyles.Strikethrough;
            taskText.color = Color.gray;
        }
        else
        {
            taskText.fontStyle = FontStyles.Normal;
            taskText.color = Color.black;
        }

        // ใส่ปุ่มกด
        checkbox.onValueChanged.RemoveAllListeners();
        checkbox.onValueChanged.AddListener(delegate { OnCheckboxClick(); });

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(delegate { OnDeleteClick(); });
    }

    void OnCheckboxClick()
    {
        manager.ToggleTask(taskIndex);
    }

    void OnDeleteClick()
    {
        manager.RemoveTask(taskIndex);
    }
}