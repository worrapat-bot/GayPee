using System;
using System.Collections.Generic;
using UnityEngine;

// This defines what a quest is - like a template for all quests
[System.Serializable]
public class Quest
{
    public string id;
    public string questName;
    public string description;
    public int experienceReward;
    public int goldReward;

    public QuestGoal goal;

    public bool isCompleted;
    public bool isActive;

    public Quest(string id, string name, string desc, int exp, int gold, QuestGoal questGoal)
    {
        this.id = id;
        this.questName = name;
        this.description = desc;
        this.experienceReward = exp;
        this.goldReward = gold;
        this.goal = questGoal;
        this.isCompleted = false;
        this.isActive = false;
    }

    public void Complete()
    {
        isCompleted = true;
        isActive = false;
        Debug.Log("Quest Completed: " + questName);
    }
}

// This defines what you need to do to complete a quest
[System.Serializable]
public class QuestGoal
{
    public GoalType goalType;
    public string targetId; // What to kill/collect (e.g., "Enemy", "Coin")
    public int requiredAmount;
    public int currentAmount;

    public bool IsReached()
    {
        return currentAmount >= requiredAmount;
    }

    public void UpdateProgress(int amount)
    {
        currentAmount += amount;
        if (currentAmount > requiredAmount)
            currentAmount = requiredAmount;
    }
}

public enum GoalType
{
    Kill,
    Collect,
    Reach
}