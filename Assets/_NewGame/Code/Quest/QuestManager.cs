using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void NotifyFocusQuestItem(string questID, string itemName)
    {
        Debug.Log($"[Quest] Focused on item for quest {questID}: {itemName}");
    }

    public void NotifyItemCollected(string questID, string itemName)
    {
        Debug.Log($"[Quest] Collected {itemName} for quest {questID}");
    }

    // ✅ เพิ่มระบบนับความคืบหน้า Quest
    public void UpdateQuestProgress(string questID, int amount = 1)
    {
        Debug.Log($"[Quest] Progress updated for quest {questID} (+{amount})");
        // ภายหลังสามารถใส่ระบบเก็บ progress จริงได้
    }
}
