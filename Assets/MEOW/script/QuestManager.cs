using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// This manages all quests in your game - the main brain of the system
public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    public List<Quest> allQuests = new List<Quest>();
    public List<Quest> activeQuests = new List<Quest>();
    public List<Quest> completedQuests = new List<Quest>();

    // Events that other scripts can listen to
    public UnityEvent<Quest> onQuestAccepted;
    public UnityEvent<Quest> onQuestCompleted;

    void Awake()
    {
        // Singleton pattern - only one QuestManager exists
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        InitializeQuests();
    }

    void InitializeQuests()
    {
        // Create some example quests
        QuestGoal goal1 = new QuestGoal
        {
            goalType = GoalType.Kill,
            targetId = "Enemy",
            requiredAmount = 5,
            currentAmount = 0
        };
        Quest quest1 = new Quest("quest_1", "Defeat 5 Enemies", "Help clear the area of enemies", 100, 50, goal1);

        QuestGoal goal2 = new QuestGoal
        {
            goalType = GoalType.Collect,
            targetId = "Coin",
            requiredAmount = 10,
            currentAmount = 0
        };
        Quest quest2 = new Quest("quest_2", "Collect 10 Coins", "Gather coins scattered around", 50, 25, goal2);

        allQuests.Add(quest1);
        allQuests.Add(quest2);
    }

    public void AcceptQuest(Quest quest)
    {
        if (!quest.isActive && !quest.isCompleted)
        {
            quest.isActive = true;
            activeQuests.Add(quest);
            onQuestAccepted?.Invoke(quest);
            Debug.Log("Quest Accepted: " + quest.questName);
        }
    }

    public void UpdateQuestProgress(string targetId, int amount)
    {
        foreach (Quest quest in activeQuests)
        {
            if (quest.goal.targetId == targetId)
            {
                quest.goal.UpdateProgress(amount);
                Debug.Log($"Progress: {quest.questName} - {quest.goal.currentAmount}/{quest.goal.requiredAmount}");

                if (quest.goal.IsReached())
                {
                    CompleteQuest(quest);
                }
            }
        }
    }

    void CompleteQuest(Quest quest)
    {
        quest.Complete();
        activeQuests.Remove(quest);
        completedQuests.Add(quest);
        onQuestCompleted?.Invoke(quest);

        // Give rewards
        Debug.Log($"Rewards: {quest.experienceReward} XP, {quest.goldReward} Gold");
    }

    public Quest GetQuestById(string id)
    {
        return allQuests.Find(q => q.id == id);
    }
}