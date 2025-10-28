using UnityEngine;

public class QuestItem : MonoBehaviour
{
    public string questID;
    
    // ✅ แก้ไข: เปลี่ยนจาก string เป็น int เพื่อให้ตรงกับ QuestManager
    public int taskID = 0; 
    
    public bool isPickedUp = false;

    public void OnPickedUp()
    {
        if (!isPickedUp)
        {
            isPickedUp = true;
            
            // ตอนนี้ questID (string) และ taskID (int) ตรงกับที่ QuestManager ต้องการแล้ว
            QuestManager.Instance.UpdateQuestProgress(questID, taskID);
            
            Debug.Log("อัปเดตเควส: " + questID + " → Task ID: " + taskID);
        }
    }
}