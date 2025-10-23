using UnityEngine;

// Example scripts showing how to use the quest system

// Put this on enemies to update kill quests
public class Enemy : MonoBehaviour
{
    void Die()
    {
        // When enemy dies, update quest progress
        QuestManager.instance.UpdateQuestProgress("Enemy", 1);
        Destroy(gameObject);
    }

    // Example: call Die() when health reaches 0
    void Update()
    {
        // Press K to simulate killing this enemy (for testing)
        if (Input.GetKeyDown(KeyCode.K))
        {
            Die();
        }
    }
}

// Put this on collectible items
public class Collectible : MonoBehaviour
{
    public string collectibleId = "Coin";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            QuestManager.instance.UpdateQuestProgress(collectibleId, 1);
            Destroy(gameObject);
        }
    }
}

// Put this on NPCs that give quests
public class QuestGiver : MonoBehaviour
{
    public string questId = "quest_1";
    private QuestUI questUI;

    void Start()
    {
        questUI = FindObjectOfType<QuestUI>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Show quest when player approaches
            Quest quest = QuestManager.instance.GetQuestById(questId);
            if (quest != null && !quest.isCompleted)
            {
                questUI.ShowQuestDetails(quest);
            }
        }
    }
}