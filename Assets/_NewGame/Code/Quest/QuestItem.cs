using UnityEngine;

public class QuestItem : MonoBehaviour
{
    public string questID;
    public string taskID;
    public bool isPickedUp = false;

    public void OnPickedUp()
    {
        if (!isPickedUp)
        {
            isPickedUp = true;
            QuestManager.Instance.UpdateQuestProgress(questID, taskID);
            Debug.Log("อัปเดตเควส: " + questID + " → " + taskID);
        }
    }
}
