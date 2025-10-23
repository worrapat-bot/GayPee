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
        void InitializeQuests()
        {
            // Example: Create a collection quest
            QuestGoal goal = new QuestGoal
            {
                goalType = GoalType.Collect,
                targetId = "Gold",  // What to collect
                requiredAmount = 20, // How many needed
                currentAmount = 0
            };

            Quest myQuest = new Quest(
                "quest_gold",           // Unique ID
                "Collect Gold",         // Name
                "Find 20 gold pieces",  // Description
                150,                    // XP reward
                100,                    // Gold reward
                goal
            );

            allQuests.Add(myQuest);
        }
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