using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    private Dictionary<string, HashSet<string>> questProgress = new Dictionary<string, HashSet<string>>();

    void Awake()
    {
        Instance = this;
    }

    public void UpdateQuestProgress(string questID, string taskID)
    {
        if (!questProgress.ContainsKey(questID))
            questProgress[questID] = new HashSet<string>();

        questProgress[questID].Add(taskID);
        Debug.Log($"[Quest] {questID}: ทำ {taskID} เสร็จแล้ว");
    }

    public bool IsTaskCompleted(string questID, string taskID)
    {
        return questProgress.ContainsKey(questID) && questProgress[questID].Contains(taskID);
    }
}
