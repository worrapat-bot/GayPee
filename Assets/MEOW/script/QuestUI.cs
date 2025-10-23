using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuestUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject questLogPanel;
    [SerializeField] private GameObject questTrackerPanel;

    [Header("Quest Log")]
    [SerializeField] private Transform activeQuestsContent;
    [SerializeField] private Transform availableQuestsContent;
    [SerializeField] private Transform completedQuestsContent;
    [SerializeField] private GameObject questItemPrefab;

    [Header("Quest Details")]
    [SerializeField] private GameObject questDetailsPanel;
    [SerializeField] private TextMeshProUGUI detailsTitle;
    [SerializeField] private TextMeshProUGUI detailsDescription;
    [SerializeField] private Transform objectivesContainer;
    [SerializeField] private GameObject objectivePrefab;
    [SerializeField] private TextMeshProUGUI rewardsText;
    [SerializeField] private Button startQuestButton;
    [SerializeField] private Button trackQuestButton;

    [Header("Quest Tracker")]
    [SerializeField] private Transform trackedQuestsContainer;
    [SerializeField] private GameObject trackedQuestPrefab;

    private Quest selectedQuest;
    private Dictionary<string, GameObject> trackedQuestObjects = new Dictionary<string, GameObject>();

    private void Start()
    {
        questLogPanel.SetActive(false);
        questDetailsPanel.SetActive(false);

        // Subscribe to quest events
        QuestManager.Instance.OnQuestStarted.AddListener(OnQuestStarted);
        QuestManager.Instance.OnQuestCompleted.AddListener(OnQuestCompleted);
        QuestManager.Instance.OnQuestUpdated.AddListener(OnQuestUpdated);

        RefreshQuestLog();
    }

    public void ToggleQuestLog()
    {
        questLogPanel.SetActive(!questLogPanel.activeSelf);
        if (questLogPanel.activeSelf)
        {
            RefreshQuestLog();
        }
    }

    private void RefreshQuestLog()
    {
        ClearQuestList(activeQuestsContent);
        ClearQuestList(availableQuestsContent);
        ClearQuestList(completedQuestsContent);

        // Populate active quests
        foreach (var quest in QuestManager.Instance.GetActiveQuests())
        {
            CreateQuestListItem(quest, activeQuestsContent, false);
        }

        // Populate available quests
        foreach (var quest in QuestManager.Instance.GetAvailableQuests())
        {
            CreateQuestListItem(quest, availableQuestsContent, false);
        }

        // Populate completed quests
        foreach (var quest in QuestManager.Instance.GetCompletedQuests())
        {
            CreateQuestListItem(quest, completedQuestsContent, true);
        }
    }

    private void ClearQuestList(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateQuestListItem(Quest quest, Transform parent, bool isCompleted)
    {
        GameObject item = Instantiate(questItemPrefab, parent);

        TextMeshProUGUI titleText = item.transform.Find("Title").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI progressText = item.transform.Find("Progress").GetComponent<TextMeshProUGUI>();

        titleText.text = quest.title;

        if (isCompleted)
        {
            progressText.text = "Completed";
            progressText.color = Color.green;
        }
        else
        {
            progressText.text = $"{Mathf.RoundToInt(quest.GetProgress() * 100)}%";
        }

        Button button = item.GetComponent<Button>();
        button.onClick.AddListener(() => ShowQuestDetails(quest));
    }

    private void ShowQuestDetails(Quest quest)
    {
        selectedQuest = quest;
        questDetailsPanel.SetActive(true);

        detailsTitle.text = quest.title;
        detailsDescription.text = quest.description;
        rewardsText.text = $"Rewards: {quest.experienceReward} XP, {quest.goldReward} Gold";

        // Clear and populate objectives
        foreach (Transform child in objectivesContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var objective in quest.objectives)
        {
            GameObject objItem = Instantiate(objectivePrefab, objectivesContainer);
            TextMeshProUGUI objText = objItem.GetComponent<TextMeshProUGUI>();

            string checkmark = objective.isCompleted ? "✓" : "○";
            objText.text = $"{checkmark} {objective.description} ({objective.currentAmount}/{objective.requiredAmount})";
            objText.color = objective.isCompleted ? Color.green : Color.white;
        }

        // Configure buttons
        if (quest.status == QuestStatus.NotStarted)
        {
            startQuestButton.gameObject.SetActive(true);
            startQuestButton.onClick.RemoveAllListeners();
            startQuestButton.onClick.AddListener(() => StartQuest(quest.id));
            trackQuestButton.gameObject.SetActive(false);
        }
        else if (quest.status == QuestStatus.InProgress)
        {
            startQuestButton.gameObject.SetActive(false);
            trackQuestButton.gameObject.SetActive(true);
            trackQuestButton.GetComponentInChildren<TextMeshProUGUI>().text = quest.isTracked ? "Untrack" : "Track";
            trackQuestButton.onClick.RemoveAllListeners();
            trackQuestButton.onClick.AddListener(() => ToggleTracking(quest.id));
        }
        else
        {
            startQuestButton.gameObject.SetActive(false);
            trackQuestButton.gameObject.SetActive(false);
        }
    }

    private void StartQuest(string questId)
    {
        QuestManager.Instance.StartQuest(questId);
        questDetailsPanel.SetActive(false);
        RefreshQuestLog();
    }

    private void ToggleTracking(string questId)
    {
        QuestManager.Instance.ToggleQuestTracking(questId);
        ShowQuestDetails(selectedQuest); // Refresh details
    }

    private void OnQuestStarted(Quest quest)
    {
        UpdateQuestTracker();
    }

    private void OnQuestCompleted(Quest quest)
    {
        UpdateQuestTracker();
        RefreshQuestLog();
    }

    private void OnQuestUpdated(Quest quest)
    {
        UpdateQuestTracker();
        if (selectedQuest != null && selectedQuest.id == quest.id)
        {
            ShowQuestDetails(quest);
        }
    }

    private void UpdateQuestTracker()
    {
        // Clear tracker
        foreach (Transform child in trackedQuestsContainer)
        {
            Destroy(child.gameObject);
        }
        trackedQuestObjects.Clear();

        // Add tracked quests
        foreach (var quest in QuestManager.Instance.GetActiveQuests())
        {
            if (quest.isTracked)
            {
                GameObject trackerItem = Instantiate(trackedQuestPrefab, trackedQuestsContainer);

                TextMeshProUGUI titleText = trackerItem.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                Transform objectivesList = trackerItem.transform.Find("Objectives");

                titleText.text = quest.title;

                foreach (var objective in quest.objectives)
                {
                    GameObject objText = new GameObject("Objective");
                    objText.transform.SetParent(objectivesList);
                    TextMeshProUGUI tmp = objText.AddComponent<TextMeshProUGUI>();
                    tmp.text = $"• {objective.description} ({objective.currentAmount}/{objective.requiredAmount})";
                    tmp.fontSize = 14;
                    tmp.color = objective.isCompleted ? Color.green : Color.white;
                }

                trackedQuestObjects[quest.id] = trackerItem;
            }
        }
    }

    public void CloseDetailsPanel()
    {
        questDetailsPanel.SetActive(false);
    }
}